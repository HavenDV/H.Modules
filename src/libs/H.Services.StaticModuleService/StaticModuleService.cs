using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using H.Converters;
using H.Core;
using H.Recorders;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class StaticModuleService : ServiceBase, IModuleService
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IList<IModule> Modules { get; } = new List<IModule>();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string>? CommandReceived;

        private void OnCommandReceived(string value)
        {
            CommandReceived?.Invoke(this, value);
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return InitializeAsync(() =>
            {
                foreach (var module in new IModule[]
                {
                    new NAudioRecorder(),
                    new WitAiConverter
                    {
                        Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY",
                    }
                })
                {
                    module.NewCommand += (_, value) => OnCommandReceived(value);
                    module.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);

                    Modules.Add(module);
                    Disposables.Add(module);
                }

                return Task.CompletedTask;
            }, cancellationToken);
        }

        #endregion
    }
}
