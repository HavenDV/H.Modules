using System;
using System.Collections.Generic;
using System.Linq;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Runners;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ModuleFinder : ServiceBase
    {
        #region Properties

        private ICollection<IModuleService> ModuleServices { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public IRecorder Recorder => (IRecorder)ModuleServices
            .SelectMany(service => service.Modules)
            .First(module => module.ShortName.EndsWith("Recorder", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        public IRecognizer Recognizer => (IRecognizer)ModuleServices
            .SelectMany(service => service.Modules)
            .First(module => module.ShortName.EndsWith("Recognizer", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IRunner> Runners => ModuleServices
            .SelectMany(service => service.Modules)
            .Where(module => module is IRunner)
            .Cast<IRunner>();

        #endregion

        #region Constructors

        /// <param name="moduleServices"></param>
        public ModuleFinder(params IModuleService[] moduleServices)
        {
            ModuleServices = moduleServices ?? throw new ArgumentNullException(nameof(moduleServices));

            foreach (var moduleService in moduleServices)
            {
                Dependencies.Add(moduleService);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public IEnumerable<ICall> GetCalls(string name, params string[] arguments)
        {
            return Runners
                .Select(runner => runner.TryPrepareCall(name, arguments))
                .Where(call => call != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<ICall>();
        }

        #endregion
    }
}
