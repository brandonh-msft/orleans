using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace WebOrleansClient
{
    public class WebRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private IClusterClient orleansClient;

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            var dataConnectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");
            var clusterId = RoleEnvironment.DeploymentId;

            var builder = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterId;
                    options.ServiceId = "AzureWorkerRoleSample";
                })
                .UseAzureStorageClustering(config => config.ConnectionString = dataConnectionString);

            orleansClient = builder.Build();

            orleansClient.Connect(async ex =>
            {
                Trace.TraceInformation(ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(3));
                return true;
            }).Wait();

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("OrleansClient is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("OrleansClient has stopped");
        }
    }
}
