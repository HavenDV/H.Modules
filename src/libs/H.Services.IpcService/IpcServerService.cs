using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Runners;
using H.Pipes;
using H.Pipes.AccessControl;
using H.Pipes.Args;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class IpcServerService : StaticModuleService
    {
        #region Properties

        private PipeServer<string> PipeServer { get; }

        #endregion

        #region Events

        /// <summary>
        /// Invoked whenever a client connects to the server.
        /// </summary>
        public event EventHandler<string>? ClientConnected;

        /// <summary>
        /// Invoked whenever a client disconnects from the server.
        /// </summary>
        public event EventHandler<string>? ClientDisconnected;

        private void OnClientConnected(string value)
        {
            ClientConnected?.Invoke(this, value);
        }

        private void OnClientDisconnected(string value)
        {
            ClientDisconnected?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IpcServerService(string pipeName, string commandName)
        {
            pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));

            PipeServer = new PipeServer<string>(pipeName);
            PipeServer.MessageReceived += PipeServer_OnMessageReceived;
            PipeServer.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);
            PipeServer.ClientConnected += PipeServer_OnClientConnected;
            PipeServer.ClientDisconnected += PipeServer_OnClientDisconnected;
            PipeServer.AllowUsersReadWrite();
            
            AsyncDisposables.Add(PipeServer);

#pragma warning disable CA2000 // Dispose objects before losing scope
            Add(new Runner
            {
                new AsyncCommand(commandName, WriteAsync)
            });
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        #endregion

        #region Event Handlers

        private void PipeServer_OnClientConnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnClientConnected(args.Connection.Name);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private void PipeServer_OnClientDisconnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnClientDisconnected(args.Connection.Name);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private void PipeServer_OnMessageReceived(object? _, ConnectionMessageEventArgs<string?> args)
        {
            try
            {
                OnCommandReceived(args.Message ?? string.Empty);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return InitializeAsync(async () =>
            {
                await PipeServer.StartAsync(cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(string command, CancellationToken cancellationToken = default)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));

            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            
            await PipeServer.WriteAsync(command, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
