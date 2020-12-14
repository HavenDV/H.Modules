using System;
using System.Linq;
using H.Core.Notifiers;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Runners;
using H.Notifiers;
using H.Recognizers;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    public static class TestModules
    {
        public static IRecorder CreateDefaultRecorder()
        {
            if (!NAudioRecorder.GetAvailableDevices().Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            return new NAudioRecorder();
        }
        
        public static IRecognizer CreateDefaultRecognizer()
        {
            return new WitAiRecognizer
            {
                Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
            };
        }

        public static INotifier CreateTimerNotifierWithPrintHelloWorldEach3Seconds()
        {
            return new TimerNotifier
            {
                Command = "print Hello, World!",
                IntervalInMilliseconds = 3000,
            };
        }

        public static IRunner CreateRunnerWithPrintCommand()
        {
            return new Runner
            {
                Command.WithSingleArgument("print", Console.WriteLine),
            };
        }
    }
}
