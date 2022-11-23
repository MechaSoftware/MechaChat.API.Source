using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net;
using static MechaChat.API.Handlers.MySQLHandler;
using System.Net.Http;
using CryptSharp;
using System;
using Newtonsoft.Json;
using MechaChat.API.Models;
using MechaChat.API.Attributes;
using MechaChat.API.Handlers;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;

namespace MechaChat.API.Controllers
{
    [RoutePrefix("downloads")]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = false, PreflightMaxAge = 86400)]
    public class DownloadsController : ApiController
    {
        public static Functions Functions = new Functions();

        /// <summary>
        /// Downloads the MechaChat client.
        /// </summary>
        /// <response code="200">The download has started.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status200OK)]
        [Route("distributions/app/installers/latest")]
        [Authorize(Roles = "system")]
        [HttpGet]
        public async Task<HttpResponseMessage> Download(ChangeStatusModel userChangeStatusForm)
        {
            var platformQuery = Request.GetOwinContext().Request.Query.Get("platform");

            if (platformQuery != null)
            {
                if (platformQuery == "windows")
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);

                    using (var httpClient = new HttpClient())
                    {
                        var res = await httpClient.GetAsync("https://dl.mecha.chat/app/windows/MechaChat_Installer.exe");

                        response.Content = new StreamContent(await res.Content.ReadAsStreamAsync());

                        return response;
                    }
                }
                else if (platformQuery == "macos")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "{ \"info\": [ \"The MacOS Installer is not available at this time!\" ] }");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "{ \"available_platforms\": [ \"windows\", \"macos\" ] }");
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "{ \"platform\": [ \"This field is required\" ] }");
            }
        }
    }
}
