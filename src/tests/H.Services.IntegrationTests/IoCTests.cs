using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using H.Core.Recorders;
using H.Core.Runners;
using H.Recognizers;
using H.Core.Utilities;
using H.Notifiers;
using H.Recorders;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class IoCTests
    {
        public static IRecorder CreateRecorder()
        {
            if (!NAudioRecorder.GetAvailableDevices().Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            return new NAudioRecorder();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(CreateRecorder())
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(new WitAiRecognizer
                {
                    Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
                })
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(new TimerNotifier
                {
                    Command = "print Hello, World!",
                    IntervalInMilliseconds = 3000,
                })
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(new Runner
                {
                    Command.WithSingleArgument("print", Console.WriteLine),
                })
                .AsImplementedInterfaces();
            builder
                .RegisterType<StaticModuleService>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            builder
                .RegisterType<ModuleFinder>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            builder
                .RegisterType<RecognitionService>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            builder
                .RegisterType<RunnerService>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();

            return builder.Build();
        }

        [TestMethod]
        public async Task RecognitionServiceTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var container = CreateContainer();
            var exceptions = new ExceptionsBag();
            
            foreach (var service in container.Resolve<IEnumerable<IServiceBase>>())
            {
                service.ExceptionOccurred += (_, exception) =>
                {
                    Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                    exceptions.OnOccurred(exception);

                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                };
            }
            foreach (var service in container.Resolve<IEnumerable<ICommandProducer>>())
            {
                service.CommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(service.CommandReceived)}: {value}");
                };
            }
            
            var recognitionService = container.Resolve<RecognitionService>();
            recognitionService.PreviewCommandReceived += (_, value) =>
            {
                Console.WriteLine($"{nameof(RecognitionService.PreviewCommandReceived)}: {value}");
            };

            await recognitionService.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await recognitionService.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            
            await recognitionService.StopAsync(cancellationToken);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            exceptions.EnsureNoExceptions();
        }
    }
}
