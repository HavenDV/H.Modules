using System;
using System.Linq;
using H.Core.Notifiers;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Runners;
using H.Notifiers;
using H.Recognizers;
using H.Recorders;
using H.Runners;
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

        public static INotifier CreateTimerNotifierWithDeskbandDateTimeEach1Seconds()
        {
            return new TimerNotifier
            {
                CommandFactory = () => $"deskband {DateTime.Now:T}",
                IntervalInMilliseconds = 2000,
            };
        }

        public static IRunner CreateRunnerWithPrintCommand()
        {
            return new Runner
            {
                Command.WithSingleArgument("print", Console.WriteLine),
            };
        }

        public static IRunner CreateTelegramRunner()
        {
            return new TelegramRunner
            { 
                Token = "1492150165:AAEq8RUEX1YOKjrMgMA8I-HHxrAy7dSmCvY",
                UserId = 482553595,
            };
        }
    }
}
