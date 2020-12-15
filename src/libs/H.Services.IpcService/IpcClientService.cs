using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Runners;
using H.Pipes;
using H.Pipes.Args;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class IpcClientService : StaticModuleService
    {
        #region Properties

        private PipeClient<string> PipeClient { get; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 
        /// </summary>
        public Func<ConnectionEventArgs<string>, string>? ConnectedCommandFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Func<ConnectionEventArgs<string>, string>? DisconnectedCommandFactory { get; set; }

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

                if (ConnectedCommandFactory != null)
                {
                    OnCommandReceived(ConnectedCommandFactory(args));
                }
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

                if (DisconnectedCommandFactory != null)
                {
                    OnCommandReceived(DisconnectedCommandFactory(args));
                }
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
        /// Writes command to server with timeout from property <see cref="Timeout"/>.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(string command, CancellationToken cancellationToken = default)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));

            using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.CancelAfter(Timeout);

            await PipeClient.WriteAsync(command, source.Token).ConfigureAwait(false);
        }

        #endregion
    }
}
