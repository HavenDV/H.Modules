using System;
using System.Collections.Generic;
using System.Linq;
using H.Core;
using H.Core.Converters;
using H.Core.Recorders;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ModuleService : ServiceBase
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public DynamicModuleService DynamicModuleService { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public StaticModuleService StaticModuleService { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IModule> Modules => StaticModuleService.Modules
            .Concat(DynamicModuleService.Modules);

        /// <summary>
        /// 
        /// </summary>
        public IRecorder Recorder => (IRecorder)Modules
            .First(module => module.ShortName.EndsWith("Recorder", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        public IConverter Converter => (IConverter)Modules
            .First(module => module.ShortName.EndsWith("Converter", StringComparison.OrdinalIgnoreCase));

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dynamicModuleService"></param>
        /// <param name="staticModuleService"></param>
        public ModuleService(
            DynamicModuleService dynamicModuleService, 
            StaticModuleService staticModuleService)
        {
            DynamicModuleService = dynamicModuleService ?? throw new ArgumentNullException(nameof(dynamicModuleService));
            StaticModuleService = staticModuleService ?? throw new ArgumentNullException(nameof(staticModuleService));
            
            Dependencies.AddRange(new IServiceBase[]{ DynamicModuleService, StaticModuleService });
        }

        #endregion
    }
}
