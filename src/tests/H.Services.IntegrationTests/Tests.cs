using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Utilities;
using H.Logic;
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

            await using var service = new RecognitionService(
                new ModuleService(
                    new DynamicModuleService(), 
                    new StaticModuleService()));

            var exceptions = new ExceptionsBag();
            service.ExceptionOccurred += (_, exception) =>
            {
                Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                exceptions.OnOccurred(exception);

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            };
            service.CommandReceived += (_, value) => Console.WriteLine($"{nameof(service.CommandReceived)}: {value}");
            service.PreviewCommandReceived += (_, value) => Console.WriteLine($"{nameof(service.PreviewCommandReceived)}: {value}");

            await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            
            await service.StopAsync(cancellationToken);

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
