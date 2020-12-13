using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H.Logic.Initializing
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceBase : IServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        public State InitializeState { get; set; } = State.NotStarted;

        /// <summary>
        /// 
        /// </summary>
        public State DisposeState { get; set; } = State.NotStarted;

        /// <summary>
        /// 
        /// </summary>
        protected List<IServiceBase> Dependencies { get; } = new ();

        /// <summary>
        /// 
        /// </summary>
        protected List<IAsyncDisposable> AsyncDisposables { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        protected List<IDisposable> Disposables { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InitializeAsync(Func<Task>? func = null, CancellationToken cancellationToken = default) =>
            await RunAsync(
                state => InitializeState = state,
                InitializeState,
                async () =>
                {
                    await Task
                        .WhenAll(Dependencies
                            .Select(dependency => dependency.InitializeAsync(null, cancellationToken)))
                        .ConfigureAwait(false);

                    if (func != null)
                    {
                        await func().ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask DisposeAsync() => await RunAsync(
            state => DisposeState = state, 
            DisposeState, 
            async () =>
            {
                await Task
                    .WhenAll(Dependencies
                        .Select(dependency => dependency.DisposeAsync().AsTask()))
                    .ConfigureAwait(false);
                
                foreach (var disposable in Disposables)
                {
                    disposable.Dispose();
                }

                foreach (var disposable in AsyncDisposables)
                {
                    await disposable.DisposeAsync().ConfigureAwait(false);
                }

                GC.SuppressFinalize(this);
            }).ConfigureAwait(false);

        private static async Task RunAsync(Action<State> setState, State currentState, Func<Task> func)
        {
            if (currentState is not State.NotStarted)
            {
                return;
            }

            setState(State.InProgress);

            await func().ConfigureAwait(false);

            setState(State.Completed);
        }
    }
}
