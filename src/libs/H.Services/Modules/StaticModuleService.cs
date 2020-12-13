using System.Collections.Generic;
using H.Core;
using H.Services.Core;

namespace H.Services.Modules
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
    }
}
