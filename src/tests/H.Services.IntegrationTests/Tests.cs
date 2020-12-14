﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Recognizers;
using H.Core.Utilities;
using H.Notifiers;
using H.Recorders;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class Tests
    {
        public static IRecorder CreateRecorder()
        {
            if (!NAudioRecorder.GetAvailableDevices().Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            return new NAudioRecorder();
        }

        public static IRecognizer CreateRecognizer() => new WitAiRecognizer
        {
            Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
        };

        [TestMethod]
        public async Task RecognitionServiceTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                CreateRecorder(),
                CreateRecognizer(),
                new TimerNotifier
                {
                    Command = "print Hello, World!",
                    IntervalInMilliseconds = 3000,
                });
            await using var recognitionService = new RecognitionService(new ModuleFinder(moduleService));

            var exceptions = new ExceptionsBag();
            foreach (var service in new ICommandProducer[] { moduleService, recognitionService })
            {
                service.ExceptionOccurred += (_, exception) =>
                {
                    Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                    exceptions.OnOccurred(exception);

                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                };
                service.CommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(service.CommandReceived)}: {value}");
                };
            }
            recognitionService.PreviewCommandReceived += (_, value) =>
            {
                Console.WriteLine($"{nameof(recognitionService.PreviewCommandReceived)}: {value}");
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
