using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Orleans;

namespace WebOrleansClient.Controllers
{
    public class OrleansController : ApiController
    {
        private static readonly Assembly _interfaces = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.ManifestModule.Name.Equals("azureworker.interfaces.dll", StringComparison.OrdinalIgnoreCase));

        // GET api/orleans
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/orleans")] /*grain/{grainInterfaceType}/{grainName}/{operation}*/
        public async Task<IHttpActionResult> RunGrainNoParams(/*string grainInterfaceType, string grainName, string operation*/)
        {
            var grainInterfaceType = @"IGreetGrain";
            var grainName = @"Alice";
            var operation = @"Greet";

            grainInterfaceType = $@"AzureWorker.Interfaces.{grainInterfaceType}";

            var client = DependencyResolver.Current.GetService<IClusterClient>();

            var getGrainMethod = client.GetType()
                .GetMethod(@"GetGrain", new[] { typeof(string), typeof(string) })
                .MakeGenericMethod(_interfaces.GetType(grainInterfaceType));

            var targetGrain = getGrainMethod.Invoke(client, new[] { grainName, Type.Missing });

            // we only support the string-invoke async<string> methods for grains. Keep this in mind!
            var operationResult = await (Task<string>)targetGrain.GetType()
                .GetMethod(operation, new[] { typeof(string) })
                .Invoke(targetGrain, new[] { @"Bob" });

            if (operationResult != null)
            {
                return Ok(operationResult);
            }

            return Ok();
        }

        //// POST api/orleans
        //[HttpPost]
        //[Route(@"grain/{grainName}/{operation}")]
        //public IHttpActionResult RunGrainWithParams(string grainName, string operation, [FromBody]JObject parameters)
        //{
        //    // get the grain
        //    // run the method w/ parameters
        //    // determine if there's a return value
        //    // serialize and return, else return OK()

        //    return Ok();
        //}
    }
}
