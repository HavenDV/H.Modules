using System;
using System.Collections.Generic;
using System.Linq;
using H.Core.Converters;
using H.Core.Recorders;

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
        public IConverter Converter => (IConverter)ModuleServices
            .SelectMany(service => service.Modules)
            .First(module => module.ShortName.EndsWith("Converter", StringComparison.OrdinalIgnoreCase));

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
    }
}
