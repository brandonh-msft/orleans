using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Unity;
using Unity.Mvc5;

namespace WebOrleansClient
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers
            RegisterOrleansClient(container);

            // e.g. container.RegisterType<ITestService, TestService>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static async void RegisterOrleansClient(UnityContainer container)
        {
            var dataConnectionString = CloudConfigurationManager.GetSetting("DataConnectionString");
            var clusterId = RoleEnvironment.DeploymentId;

            var builder = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterId;
                    options.ServiceId = "AzureWorkerRoleSample";
                })
                .UseAzureStorageClustering(c => c.ConnectionString = dataConnectionString);

            var orleansClient = builder.Build();

            await orleansClient.Connect(async ex =>
            {
                Trace.TraceInformation(ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(3));
                return true;
            });

            container.RegisterInstance(orleansClient);
        }
    }
}