using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                Interval = TimeSpan.FromSeconds(3),
            };
        }

        public static INotifier CreateTimerNotifierWithDeskbandDateTimeEach1Seconds()
        {
            return new TimerNotifier
            {
                CommandFactory = () => $"deskband {DateTime.Now:T}",
                Interval = TimeSpan.FromSeconds(1),
            };
        }

        public static INotifier CreateTimerNotifierWithSleep5000Each1Seconds()
        {
            return new TimerNotifier
            {
                Command = "sleep 5000",
                Interval = TimeSpan.FromSeconds(1),
            };
        }

        public static INotifier CreateTimerNotifierWithSyncSleep5000Each3Seconds()
        {
            return new TimerNotifier
            {
                Command = "sync-sleep 5000",
                Interval = TimeSpan.FromSeconds(3),
            };
        }

        public static IRunner CreateRunnerWithPrintCommand()
        {
            return new Runner
            {
                SyncAction.WithSingleArgument("print", Console.WriteLine, "value"),
            };
        }

        public static IRunner CreateRunnerWithSyncSleepCommand()
        {
            return new Runner
            {
                SyncAction.WithSingleArgument(
                    "sync-sleep",
                    argument => Thread.Sleep(Convert.ToInt32(argument)),
                    "millisecondsDelay"),
            };
        }

        public static IRunner CreateRunnerWithSleepCommand()
        {
            return new Runner
            {
                AsyncAction.WithSingleArgument(
                    "sleep", 
                    (argument, cancellationToken) => Task.Delay(Convert.ToInt32(argument), cancellationToken),
                    "millisecondsDelay"),
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
