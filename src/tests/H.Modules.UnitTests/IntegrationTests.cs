using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Containers;
using H.Core;
using H.Modules.UnitTests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Modules.UnitTests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                Application.Clear();
                Application.GetPathAndUnpackIfRequired();
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        [TestMethod]
        public async Task RecorderConverterStreamingRecognitionTest()
        {
            await BaseModuleTest<IRecorder, IConverter>(
                "H.Recorders.NAudioRecorder",
                "H.Converters.WitAiConverter",
                async (recorder, converter, cancellationToken) =>
                {
                    converter.SetSetting("Token", "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY");

                    await recorder.InitializeAsync(cancellationToken);
                    await recorder.StartAsync(cancellationToken);

                    using var recognition = await converter.StartStreamingRecognitionAsync(cancellationToken);
                    recognition.AfterPartialResults += (_, args) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} AfterPartialResults: {args.Text}");
                    recognition.AfterFinalResults += (_, args) => Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} AfterFinalResults: {args.Text}");

                    var wavHeader = recorder.WavHeader?.ToArray() ??
                                    throw new InvalidOperationException("Recorder Wav Header is null");
                    await recognition.WriteAsync(wavHeader, cancellationToken);

                    if (recorder.RawData != null)
                    {
                        await recognition.WriteAsync(recorder.RawData.ToArray(), cancellationToken);
                    }

                    // ReSharper disable once AccessToDisposedClosure
                    recorder.RawDataReceived += async (_, args) =>
                    {
                        if (args.RawData == null)
                        {
                            return;
                        }

                        await recognition.WriteAsync(args.RawData.ToArray(), cancellationToken);
                    };

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    await recorder.StopAsync(cancellationToken);
                    await recognition.StopAsync(cancellationToken);
                });
        }

        [TestMethod]
        public async Task RecorderConverterConvertTest()
        {
            await BaseModuleTest<IRecorder, IConverter>(
                "H.Recorders.NAudioRecorder",
                "H.Converters.WitAiConverter",
                async (recorder, converter, cancellationToken) =>
                {
                    converter.SetSetting("Token", "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY");

                    await recorder.InitializeAsync(cancellationToken);
                    await recorder.StartAsync(cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    await recorder.StopAsync(cancellationToken);

                    var bytes = recorder.WavData?.ToArray();
                    if (bytes == null)
                    {
                        return;
                    }

                    var result = await converter.ConvertAsync(bytes, cancellationToken);
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
            Func<T1, T2, CancellationToken, Task> testFunc) 
            where T1 : class, IModule
            where T2 : class, IModule
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var cancellationToken = cancellationTokenSource.Token;

            var receivedException = (Exception?)null;
            
            await using var manager = new ModuleManager<IModule>(
                Path.Combine(Path.GetTempPath(), $"H.Containers.Tests_{name1}_{name2}"));
            manager.ExceptionOccurred += (_, exception) =>
            {
                Console.WriteLine($"ExceptionOccurred: {exception}");
                receivedException = exception;

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
                instance.NewCommand += (_, command) =>
                {
                    Console.WriteLine($"{nameof(instance.NewCommand)}: {command}");
                };
                instance.ExceptionOccurred += (_, exception) =>
                {
                    Console.WriteLine($"{nameof(instance.ExceptionOccurred)}: {exception}");
                };
                instance.LogReceived += (_, log) =>
                {
                    Console.WriteLine($"{nameof(instance.LogReceived)}: {log}");
                };
            }
            
            await testFunc(instance1, instance2, cancellationToken);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            if (receivedException != null)
            {
                Assert.Fail(receivedException.ToString());
            }
        }
    }
}
