﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using H.Containers;
using H.Core;
using H.Modules.UnitTests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Modules.UnitTests
{
    [TestClass]
    public class SingleTests
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
        public async Task RssNotifierTest()
        {
            await BaseModuleTest<INotifier>(
                "H.Notifiers.RssNotifier", 
                async (instance, cancellationToken) =>
                {
                    instance.SetSetting("IntervalInMilliseconds", "1000");
                    instance.SetSetting("Url", "https://www.upwork.com/ab/feed/topics/rss?securityToken=3046355554bbd7e304e77a4f04ec54ff90dcfe94eb4bb6ce88c120b2a660a42c47a42de8cfd7db2f3f4962ccb8c9a8d1bb2bff326e55b5b464816c9919c4e66c&userUid=749097038387695616&orgUid=749446993539981313");
                    
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    var value = instance.GetModuleVariableValue("$rss_last_title$");
                    Console.WriteLine($"Rss Last Title: {value}");

                    Assert.IsNotNull(value, nameof(value));
                    Assert.AreNotEqual(string.Empty, value, nameof(value));
                });
        }

        [TestMethod]
        public async Task NAudioRecorderTest()
        {
            await BaseModuleTest<IRecorder>(
                "H.Recorders.NAudioRecorder",
                async (instance, cancellationToken) =>
                {
                    instance.RawDataReceived += (_, args) =>
                    {
                        Console.WriteLine(
                            $"{nameof(instance.RawDataReceived)}: {args.RawData?.Count ?? 0}, {args.WavData?.Count ?? 0}");
                    };
                    await instance.InitializeAsync(cancellationToken);

                    await instance.StartAsync(cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    await instance.StopAsync(cancellationToken);
                });
        }

        [TestMethod]
        public async Task WitAiConverterTest()
        {
            await BaseModuleTest<IConverter>(
                "H.Converters.WitAiConverter",
                async (instance, cancellationToken) =>
                {
                    instance.SetSetting("Token", "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY");
                    
                    var bytes = ResourcesUtilities.ReadFileAsBytes("проверка_проверка_8000.wav");
                    var actual = await instance.ConvertAsync(bytes, cancellationToken);

                    Assert.AreEqual("проверка", actual);
                });
        }

        public static async Task BaseModuleTest<T>(
            string name, 
            Func<T, CancellationToken, Task> testFunc) 
            where T : class, IModule
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var cancellationToken = cancellationTokenSource.Token;

            var receivedException = (Exception?)null;
            
            await using var manager = new ModuleManager<IModule>(
                Path.Combine(Path.GetTempPath(), $"H.Containers.Tests_{name}"));
            manager.ExceptionOccurred += (_, exception) =>
            {
                Console.WriteLine($"ExceptionOccurred: {exception}");
                receivedException = exception;

                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            };

            var bytes = ResourcesUtilities.ReadFileAsBytes($"{name}.zip");

            using var instance = await manager.AddModuleAsync<T>(
                new ProcessContainer(name),
                name, 
                name, 
                bytes, 
                cancellationToken);

            Assert.IsNotNull(instance);

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
            
            var types = await manager.GetTypesAsync(cancellationToken);
            ShowList(types, "Available types");

            Console.WriteLine($"{nameof(instance.ShortName)}: {instance.ShortName}");

            var availableSettings = instance.GetAvailableSettings();
            ShowList(availableSettings, "Available settings");

            await testFunc(instance, cancellationToken);

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

        private static void ShowList<T>(ICollection<T> list, string name)
        {
            Console.WriteLine($"{name}: {list.Count}");
            foreach (var value in list)
            {
                Console.WriteLine($" - {value}");
            }

            Console.WriteLine();
        }
    }
}
