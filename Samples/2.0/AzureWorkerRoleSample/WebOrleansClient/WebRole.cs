using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans;

namespace WebOrleansClient
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public override void Run()
        {
            base.Run();
        }
    }
}
