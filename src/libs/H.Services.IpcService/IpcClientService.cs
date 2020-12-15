using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Runners;
using H.Pipes;
using H.Pipes.Args;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class IpcClientService : StaticModuleService
    {
        #region Properties

        private PipeClient<string> PipeClient { get; }

        #endregion

        #region Events

        /// <summary>
        /// Invoked whenever a client connects to the server.
        /// </summary>
        public event EventHandler<string>? Connected;

        /// <summary>
        /// Invoked whenever a client disconnects from the server.
        /// </summary>
        public event EventHandler<string>? Disconnected;

        private void OnConnected(string value)
        {
            Connected?.Invoke(this, value);
        }

        private void OnDisconnected(string value)
        {
            Disconnected?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IpcClientService(string pipeName, string commandName)
        {
            pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));
            commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));

            PipeClient = new PipeClient<string>(pipeName);
            PipeClient.MessageReceived += PipeServer_OnMessageReceived;
            PipeClient.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);
            PipeClient.Connected += PipeServer_OnConnected;
            PipeClient.Disconnected += PipeServer_OnDisconnected;
            
            AsyncDisposables.Add(PipeClient);

#pragma warning disable CA2000 // Dispose objects before losing scope
            Add(new Runner
            {
                new AsyncCommand(commandName, WriteAsync)
            });
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        #endregion

        #region Event Handlers

        private void PipeServer_OnConnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnConnected(args.Connection.Name);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private void PipeServer_OnDisconnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnDisconnected(args.Connection.Name);
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
                await PipeClient.ConnectAsync(cancellationToken).ConfigureAwait(false);
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
            
            await PipeClient.WriteAsync(command, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
