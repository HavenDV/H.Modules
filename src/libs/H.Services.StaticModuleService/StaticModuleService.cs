using System;
using System.Collections.Generic;
using H.Core;
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

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modules"></param>
        public StaticModuleService(params IModule[] modules)
        {
            modules = modules ?? throw new ArgumentNullException(nameof(modules));

            foreach (var module in modules)
            {
                Add(module);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        public void Add(IModule module)
        {
            module = module ?? throw new ArgumentNullException(nameof(module));
            module.NewCommand += (_, value) => OnCommandReceived(value);
            module.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);

            Modules.Add(module);
            Disposables.Add(module);
        }

        #endregion
    }
}
