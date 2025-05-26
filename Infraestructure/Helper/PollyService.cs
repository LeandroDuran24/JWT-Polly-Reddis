using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Helper
{
    public static class PollyService
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Http code 408-599
                .OrResult(response => response.StatusCode == HttpStatusCode.NotFound) // Aqui puedo incluir codigos http que me interesan reintentar
                .WaitAndRetryAsync(3, retryAttempt =>//cada 3 seg
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), //El primer intento a los 2seg, segundo intento a los 4seg y tercer a los 8seg.
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"[POLLY] Retry {retryAttempt}");
                        logger.LogError($"Intento {retryAttempt} despues {timespan.TotalSeconds}s debido a: " +
                                          $"{outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    });
        }
    }
}
