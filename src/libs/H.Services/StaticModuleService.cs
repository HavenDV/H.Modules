using System.Collections.Generic;
using H.Core;
using H.Logic.Initializing;

namespace H.Logic
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
