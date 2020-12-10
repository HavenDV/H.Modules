using System;
using System.Collections.Generic;
using H.Core;

namespace H.Modules.UnitTests.Extensions
{
    internal static class LogExtensions
    {
        public static Action<string> LogAction { get; set; } = Console.WriteLine;

        public static void Log(this string value, string name)
        {
            LogAction($"{name}: {value}");
        }
        
        public static void EnableLog(this IModule module)
        {
            module.NewCommand += (_, command) =>
            {
                LogAction($"{nameof(module.NewCommand)}: {command}");
            };
            module.ExceptionOccurred += (_, exception) =>
            {
                LogAction($"{nameof(module.ExceptionOccurred)}: {exception}");
            };
            module.LogReceived += (_, log) =>
            {
                LogAction($"{nameof(module.LogReceived)}: {log}");
            };
            module.NewCommandAsync += (_, args) =>
            {
                LogAction($"{nameof(module.NewCommandAsync)}: {args.Text}");
            };
            module.SettingsSaved += (_, _) =>
            {
                LogAction($"{nameof(module.SettingsSaved)}");
            };
        }

        public static void Log<T>(this ICollection<T> list, string name)
        {
            LogAction($"{name}: {list.Count}");
            foreach (var value in list)
            {
                LogAction($" - {value}");
            }

            LogAction(string.Empty);
        }
    }
}
