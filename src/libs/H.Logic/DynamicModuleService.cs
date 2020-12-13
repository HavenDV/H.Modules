using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using H.Containers;
using H.Core;
using H.Core.Recorders;
using H.IO.Utilities;
using H.Logic.Initializing;
using H.Modules;

namespace H.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DynamicModuleService : ServiceBase
    {
        #region Properties

        private ModuleManager<IModule> ModuleManager { get; } = new (
            Path.Combine(Path.GetTempPath(), "H.Logic"));
        
        /// <summary>
        /// 
        /// </summary>
        public ICollection<IModule> Modules { get; } = new List<IModule>();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DynamicModuleService()
        {
            AsyncDisposables.Add(ModuleManager);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return InitializeAsync(async () =>
            {
                await AddAsync<IRecorder>("H.Recorders.NAudioRecorder", cancellationToken)
                    .ConfigureAwait(false);
                await AddAsync<IRecorder>("H.Converters.WitAiConverter", cancellationToken)
                    .ConfigureAwait(false);
            }, cancellationToken);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IModule> AddAsync<T>(string name, CancellationToken cancellationToken = default)
            where T : class, IModule
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var container = new ProcessContainer(name);
#pragma warning restore CA2000 // Dispose objects before losing scope

            var instance = await ModuleManager.AddModuleAsync<T>(
                    container,
                    name,
                    name,
                    ResourcesUtilities.ReadFileAsBytes($"{name}.zip"),
                    cancellationToken)
                .ConfigureAwait(false);

            Modules.Add(instance);

            return instance;
        }
        
        #endregion
    }
}
