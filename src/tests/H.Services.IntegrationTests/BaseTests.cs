using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Services.IntegrationTests
{
    public static class BaseTests
    {
        public static async Task Start5SecondsStart5SecondsStopTestAsync(
            RecognitionService recognitionService, 
            CancellationToken cancellationToken = default)
        {
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
        }
    }
}
