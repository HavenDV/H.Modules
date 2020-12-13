using System.Collections.Generic;
using H.Core;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class StaticModuleService : ServiceBase
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public List<IModule> Modules { get; } = new ();

        #endregion
    }
}
