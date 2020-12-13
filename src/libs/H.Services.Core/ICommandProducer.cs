using System;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandProducer : IServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<string>? CommandReceived;
    }
}
