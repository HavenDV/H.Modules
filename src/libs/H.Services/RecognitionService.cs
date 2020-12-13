using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Converters;
using H.Core.Recorders;
using H.Core.Utilities;
using H.Logic.Initializing;

namespace H.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RecognitionService : ServiceBase
    {
        #region Properties

        private ModuleService ModuleService { get; }

        private IConverter Converter => ModuleService.Converter;
        private IRecorder Recorder => ModuleService.Recorder;
        
        private IStreamingRecognition? CurrentRecognition { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string>? PreviewCommandReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string>? CommandReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        private void OnPreviewCommandReceived(string value)
        {
            PreviewCommandReceived?.Invoke(this, value);
        }
        
        private void OnCommandReceived(string value)
        {
            CommandReceived?.Invoke(this, value);
        }

        private void OnExceptionOccurred(Exception value)
        {
            ExceptionOccurred?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <param name="moduleService"></param>
        public RecognitionService(ModuleService moduleService)
        {
            ModuleService = moduleService ?? throw new ArgumentNullException(nameof(moduleService));
            
            Dependencies.Add(ModuleService);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(null, cancellationToken).ConfigureAwait(false);
            }
            
            if (CurrentRecognition != null)
            {
                await StopAsync(cancellationToken).ConfigureAwait(false);
            }
            
            var exceptions = new ExceptionsBag();
            exceptions.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
            
            // TODO: EXCLUDE WRITE WAV HEADER FROM LOGIC.
            CurrentRecognition = await Converter.StartStreamingRecognitionAsync(
                Recorder, true, exceptions, cancellationToken)
                .ConfigureAwait(false);
            CurrentRecognition.PartialResultsReceived += (_, value) => OnPreviewCommandReceived(value);
            CurrentRecognition.FinalResultsReceived += (_, value) => OnCommandReceived(value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentRecognition == null)
            {
                return;
            }
            
            await CurrentRecognition.StopAsync(cancellationToken).ConfigureAwait(false);

            CurrentRecognition.Dispose();
            CurrentRecognition = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);

            await base.DisposeAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
