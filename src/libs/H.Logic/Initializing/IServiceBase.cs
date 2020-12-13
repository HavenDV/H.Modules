using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Logic.Initializing
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServiceBase : IAsyncDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public State InitializeState { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public State DisposeState { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task InitializeAsync(Func<Task>? func = null, CancellationToken cancellationToken = default);
    }
}
