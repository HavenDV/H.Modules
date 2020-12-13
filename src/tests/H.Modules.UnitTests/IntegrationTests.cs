using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using H.Containers;
using H.Core;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Utilities;
using H.IO.Utilities;
using H.Modules.UnitTests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Modules.UnitTests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        [Ignore("Recorders are not work on GitHub Actions.")]
        public async Task RecorderConverterStreamingRecognitionTest()
        {
            await BaseModuleTest<IRecorder, IRecognizer>(
                "H.Recorders.NAudioRecorder",
                "H.Converters.WitAiConverter",
                async (exceptions, recorder, recognizer, cancellationToken) =>
                {
                    recognizer.SetSetting("Token", "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY");
                    
                    using var recognition = await recognizer.StartStreamingRecognitionAsync(recorder, true, exceptions, cancellationToken);
                    recognition.PartialResultsReceived += (_, value) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} {nameof(recognition.PartialResultsReceived)}: {value}");
                    recognition.FinalResultsReceived += (_, value) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} {nameof(recognition.FinalResultsReceived)}: {value}");

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    
                    await recognition.StopAsync(cancellationToken);
                });
        }

        [TestMethod]
        [Ignore("Recorders are not work on GitHub Actions.")]
        public async Task RecorderConverterConvertTest()
        {
            await BaseModuleTest<IRecorder, IRecognizer>(
                "H.Recorders.NAudioRecorder",
                "H.Converters.WitAiConverter",
                async (_, recorder, recognizer, cancellationToken) =>
                {
                    recognizer.SetSetting("Token", "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY");
                    
                    await recorder.StartAsync(cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    await recorder.StopAsync(cancellationToken);

                    var result = await recognizer.ConvertAsync(recorder.WavData, cancellationToken);
                    Console.WriteLine(result);
                });
        }

        private static IContainer CreateContainer(string name)
        {
            return new ProcessContainer(name)
            {
                LaunchInCurrentProcess = false,
            };
        }

        public static async Task BaseModuleTest<T1, T2>(
            string name1,
            string name2,
            Func<ExceptionsBag, T1, T2, CancellationToken, Task> testFunc) 
            where T1 : class, IModule
            where T2 : class, IModule
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var cancellationToken = cancellationTokenSource.Token;

            var exceptions = new ExceptionsBag();
            
            await using var manager = new ModuleManager<IModule>(
                Path.Combine(Path.GetTempPath(), $"H.Containers.Tests_{name1}_{name2}"));
            manager.ExceptionOccurred += (_, exception) =>
            {
                Console.WriteLine($"ExceptionOccurred: {exception}");
                exceptions.OnOccurred(exception);

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            };
            
            using var instance1 = await manager.AddModuleAsync<T1>(
                CreateContainer(name1),
                name1, 
                name1,
                ResourcesUtilities.ReadFileAsBytes($"{name1}.zip"),
                cancellationToken);
            using var instance2 = await manager.AddModuleAsync<T2>(
                CreateContainer(name2),
                name2,
                name2,
                ResourcesUtilities.ReadFileAsBytes($"{name2}.zip"),
                cancellationToken);

            Assert.IsNotNull(instance1);
            Assert.IsNotNull(instance2);

            foreach (var instance in new IModule[] { instance1, instance2 })
            {
                instance.EnableLog();
            }
            
            await testFunc(exceptions, instance1, instance2, cancellationToken);

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
