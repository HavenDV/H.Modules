using System;
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

        private IModuleService ModuleService { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public IRecorder Recorder => (IRecorder)ModuleService
            .Modules
            .First(module => module.ShortName.EndsWith("Recorder", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        public IConverter Converter => (IConverter)ModuleService
            .Modules
            .First(module => module.ShortName.EndsWith("Converter", StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Constructors

        /// <param name="moduleService"></param>
        public ModuleFinder(IModuleService moduleService)
        {
            ModuleService = moduleService ?? throw new ArgumentNullException(nameof(moduleService));
            
            Dependencies.Add(moduleService);
        }

        #endregion
    }
}
