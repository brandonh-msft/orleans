using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Silo
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private ISiloHost siloHost;

        public override void Run()
        {
            Trace.TraceInformation("Silo is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();

                runCompleteEvent.WaitOne();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            bool result = base.OnStart();

            Trace.TraceInformation("Silo has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Silo is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Silo has stopped");
        }

        private Task RunAsync(CancellationToken cancellationToken)
        {
            var siloEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["OrleansSiloEndpoint"].IPEndpoint;

            var builder = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = @"ForFunctions";
                    options.ServiceId = @"AzureFunctionsSample";
                })
                .ConfigureEndpoints(siloEndpoint.Address, siloEndpoint.Port, 44567)
                // We use Development Clustering so we can specify a Static Endpoint for the Silo.
                // This needs to be the *public* IP address of the cloud service hosting it.
                .UseDevelopmentClustering(siloEndpoint);

            siloHost = builder.Build();

            return siloHost.StartAsync(cancellationToken);
        }
    }
}
