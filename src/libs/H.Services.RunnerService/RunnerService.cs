using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Utilities;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RunnerService : ServiceBase
    {
        #region Properties

        private ModuleFinder ModuleFinder { get; }

        #endregion
        
        #region Constructors

        /// <param name="moduleFinder"></param>
        /// <param name="commandProducers"></param>
        public RunnerService(ModuleFinder moduleFinder, params ICommandProducer[] commandProducers)
        {
            ModuleFinder = moduleFinder ?? throw new ArgumentNullException(nameof(moduleFinder));
            commandProducers = commandProducers ?? throw new ArgumentNullException(nameof(commandProducers));

            foreach (var producer in commandProducers)
            {
                producer.CommandReceived += OnCommandReceived;

                Dependencies.Add(producer);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RunAsync(string command, CancellationToken cancellationToken = default)
        {
            var values = command.SplitOnlyFirstIgnoreQuote(' ');
            var name = values.ElementAt(0);
            var argument = values.ElementAtOrDefault(1) ?? string.Empty;
            
            var call = ModuleFinder.TryPrepareCall(name, argument);
            if (call == null)
            {
                return;
            }
            
            await call.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Event Handlers

        private async void OnCommandReceived(object _, string value)
        {
            try
            {
                await RunAsync(value).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        #endregion
    }
}
