using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Runners;
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
        private ConcurrentDictionary<ICall, CancellationTokenSource> CancellationTokenSources { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentDictionary<ICall, Task> Tasks { get; } = new ();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICall>? CallRunning;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICall>? CallRan;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICall>? CallCancelled;

        private void OnCallRunning(ICall value)
        {
            CallRunning?.Invoke(this, value);
        }

        private void OnCallRan(ICall value)
        {
            CallRan?.Invoke(this, value);
        }

        private void OnCallCancelled(ICall value)
        {
            CallCancelled?.Invoke(this, value);
        }

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

            var tasks = new List<Task>();
            foreach (var call in ModuleFinder.GetCalls(name, argument))
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                CancellationTokenSources.TryAdd(call, source);
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        OnCallRunning(call);

                        await call.RunAsync(source.Token).ConfigureAwait(false);

                        OnCallRan(call);
                    }
                    catch (OperationCanceledException)
                    {
                        OnCallCancelled(call);
                        
                        throw;
                    }
                    finally
                    {
                        if (CancellationTokenSources.TryRemove(call, out var tokenSource))
                        {
                            tokenSource.Dispose();
                        }
                    }
                }, source.Token);
                
                Tasks.TryAdd(call, task);
                
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        public void CancelCall(ICall call)
        {
            if (!CancellationTokenSources.TryGetValue(call, out var source))
            {
                return;
            }
            
            source.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelAll()
        {
            foreach (var call in Tasks.Keys)
            {
                CancelCall(call);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async ValueTask DisposeAsync()
        {
            CancelAll();

            try
            {
                await Task.WhenAll(Tasks.Values).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            
            Tasks.Clear();

            foreach (var source in CancellationTokenSources.Values)
            {
                source.Dispose();
            }
            CancellationTokenSources.Clear();
            
            await base.DisposeAsync().ConfigureAwait(false);
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
