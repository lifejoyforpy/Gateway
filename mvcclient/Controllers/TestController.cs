using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
namespace mvcclient.Controllers
{
    public class TestController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public  async Task<IActionResult> CallApiUsingClientCredentials()
        {               
            var client = new HttpClient();
            //var cliResponse= await client.GetDiscoveryDocumentAsync("http://localhost:5000/IdentityServer/connect/token");
            var response = await client.RequestTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "http://localhost:5000/IdentityServer/connect/token",          
                ClientId = "client",
                ClientSecret = "secret",
                Parameters={{ "scope", "api1" }},
            });
            client.SetBearerToken(response.AccessToken);
            await client.GetAsync("http://localhost:5003/MyApi/identity");
            return new JsonResult("");
        }
    }
}