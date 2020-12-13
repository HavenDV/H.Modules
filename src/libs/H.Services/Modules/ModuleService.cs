using System;
using System.Collections.Generic;
using System.Linq;
using H.Core;
using H.Services.Core;

namespace H.Services.Modules
{
    /// <summary>
    /// 
    /// </summary>
    public class ModuleService : ServiceBase, IModuleService
    {
        #region Properties

        private DynamicModuleService DynamicModuleService { get; }
        private StaticModuleService StaticModuleService { get; }

        /// <summary>
        /// 
        /// </summary>
        public IList<IModule> Modules => StaticModuleService.Modules
            .Concat(DynamicModuleService.Modules)
            .ToList();

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
