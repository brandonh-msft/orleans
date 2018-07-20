using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AzureWorker.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Orleans;
using Orleans.Configuration;

namespace FunctionClient
{
    public static class Function1
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private static IClusterClient orleansClient;

        [FunctionName("Function1")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, Microsoft.Azure.WebJobs.ExecutionContext execContext, TraceWriter log)
        {
            if (orleansClient == null)
            {
                var builder = new ClientBuilder()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = @"ForFunctions";
                        options.ServiceId = @"AzureFunctionsSample";
                    })
                    // Static clustering allows us to point the Function to a *specific* Silo instance.
                    // This needs to be the *public* IP of the Silo cloud service
                    .UseStaticClustering(new IPEndPoint(new IPAddress(new byte[] { 13, 66, 198, 247 }), 44567));

                orleansClient = builder.Build();

                orleansClient.Connect(async ex =>
                {
                    log.Info(ex.Message);

                    await Task.Delay(TimeSpan.FromSeconds(3));
                    return true;
                }).Wait();
            }

            var aliceGrain = orleansClient.GetGrain<IGreetGrain>("Alice");
            string greet = await aliceGrain.Greet("greetName");

            log.Info(greet);

            return new OkObjectResult(greet);
        }
    }
}
