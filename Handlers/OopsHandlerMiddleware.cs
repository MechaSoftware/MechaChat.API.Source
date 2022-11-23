using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace MechaChat.API.ErrorHandling
{
    public class OopsHandlerMiddleware : OwinMiddleware
    {
        public OopsHandlerMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                context.Response.StatusCode = 200;
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                Console.WriteLine(ex);
                await context.Response.WriteAsync($"Something went wrong!");
            }
        }
    }
}
