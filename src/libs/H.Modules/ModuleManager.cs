using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Containers;

namespace H.Modules
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModule"></typeparam>
    public class ModuleManager<TModule> : IAsyncDisposable 
        where TModule : class
    {
        #region Properties

        private string Folder { get; }

        private Dictionary<string, IContainer> Containers { get; } = new ();
        private Dictionary<string, TModule> Modules { get; } = new ();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        private void OnExceptionOccurred(Exception exception)
        {
            ExceptionOccurred?.Invoke(this, exception);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        public ModuleManager(string folder)
        {
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSubModule"></typeparam>
        /// <param name="container"></param>
        /// <param name="name"></param>
        /// <param name="typeName"></param>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TSubModule> AddModuleAsync<TSubModule>(
            IContainer container,
            string name,
            string typeName, 
            byte[] bytes,
            CancellationToken cancellationToken = default)
            where TSubModule : class, TModule
        {
            container = container ?? throw new ArgumentNullException(nameof(container));
            
            TSubModule? instance = null;
            try
            {
                //container.MethodsCancellationToken = cancellationTokenSource.Token,
                container.ExceptionOccurred += (_, exception) =>
                {
                    OnExceptionOccurred(exception);
                };

                await container.InitializeAsync(cancellationToken).ConfigureAwait(false);
                await container.StartAsync(cancellationToken).ConfigureAwait(false);

                var subFolder = Path.Combine(Folder, name);
                if (Directory.Exists(subFolder))
                {
                    Directory.Delete(subFolder, true);
                }
                Directory.CreateDirectory(subFolder);
                var path = Path.Combine(subFolder, $"{name}.zip");
                File.WriteAllBytes(path, bytes);

                ZipFile.ExtractToDirectory(path, subFolder);

                await container.LoadAssemblyAsync(Path.Combine(subFolder, $"{name}.dll"), cancellationToken).ConfigureAwait(false);

                instance = await container.CreateObjectAsync<TSubModule>(typeName, cancellationToken).ConfigureAwait(false);

                Containers.Add(name, container);
                Modules.Add(name, instance);
            }
            catch (Exception)
            {
                container.Dispose();
                if (instance is IDisposable instanceDisposable)
                {
                    instanceDisposable.Dispose();
                }
                if (Containers.ContainsKey(name))
                {
                    Containers.Remove(name);
                }
                throw;
            }

            return instance ??
                   throw new InvalidOperationException("Instance is null");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, IList<string>>> GetTypesAsync(
            CancellationToken cancellationToken = default)
        {
            var values = await Task.WhenAll(
                Containers
                    .Select(async pair => (pair.Key, await pair.Value.GetTypesAsync(cancellationToken).ConfigureAwait(false))))
                .ConfigureAwait(false);

            return values.ToDictionary(
                pair => pair.Key, 
                pair => pair.Item2);
        }

        /// <summary>
        /// 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var pair in Modules)
            {
                if (pair.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                if (pair.Value is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
            }
            Modules.Clear();

            foreach (var pair in Containers)
            {
                pair.Value.Dispose();
            }
            Containers.Clear();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Event Handlers



        #endregion
    }
}
