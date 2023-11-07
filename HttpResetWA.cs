// #r "nuget: Microsoft.Identity.Client"

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;


namespace Company.Function
{
    public static class HttpTriggerReset
    {
        [FunctionName("HttpTriggerReset")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string subscriptionId = "";
            string resourceGroup = "";
            string webAppName = "";
            string tenantId = "";
            string clientId = "";
            string clientSecret = "";
            string[] scope = new string[] { "https://management.azure.com/.default" };  // Alcance necesario para el acceso a Azure Management API
            string authority = $"https://login.microsoftonline.com/{tenantId}";


            // Crea una aplicación de cliente MSAL
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithClientSecret(clientSecret)
                .Build();
            var result = await app.AcquireTokenForClient(scope)
                .ExecuteAsync();


            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                // Se ha obtenido un token de acceso
                string accessToken = result.AccessToken;
                Console.WriteLine("Token de acceso obtenido con éxito.");
                // Define la URL del endpoint de reinicio de la aplicación web
                string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Web/sites/{webAppName}/restart?api-version=2022-03-01";
                // Crea un cliente HTTP
                using (HttpClient client = new HttpClient())
                {
                    // Agrega el token de acceso al encabezado Authorization
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    // Realiza la solicitud HTTP para reiniciar la aplicación web
                    HttpResponseMessage response = await client.PostAsync(url, null);
                    // Verifica la respuesta
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Reinicio Exitoso");
                        string responseMessage = $"Reinicio Exitoso. Código de estado: {response.StatusCode}";
                        return new OkObjectResult(responseMessage);
                    }
                    else
                    {
                        Console.WriteLine(response.ToString());
                        string responseMessage = $"Error en el reinicio. Código de estado: {response.ReasonPhrase}";
                        return new OkObjectResult(responseMessage);
                    }
                }
            }
            else
            {
                string responseMessage = "No se pudo obtener un token de acceso.";
                return new OkObjectResult(responseMessage);
            }
        }
    }
}
