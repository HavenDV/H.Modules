using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Utilities;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task RecognitionServiceTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var ipcService = new IpcClientService("H.DeskBand", "deskband");
            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTimerNotifierWithPrintHelloWorldEach3Seconds(),
                TestModules.CreateRunnerWithPrintCommand(),
                TestModules.CreateTelegramRunner()
            );
            await using var moduleFinder = new ModuleFinder(moduleService, ipcService);
            await using var recognitionService = new RecognitionService(moduleFinder);
            await using var runnerService = new RunnerService(moduleFinder, moduleService, recognitionService, ipcService);
            
            var exceptions = new ExceptionsBag();
            foreach (var service in new IServiceBase[] { moduleService, recognitionService, moduleFinder, runnerService, ipcService })
            {
                service.ExceptionOccurred += (_, exception) =>
                {
                    Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                    exceptions.OnOccurred(exception);

                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                };
            }
            foreach (var service in new ICommandProducer[] { moduleService, recognitionService, ipcService })
            {
                service.CommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(service.CommandReceived)}: {value}");
                };
            }

            await recognitionService.Start5SecondsStart5SecondsStopTestAsync(cancellationToken);

            exceptions.EnsureNoExceptions();
        }
    }
}
