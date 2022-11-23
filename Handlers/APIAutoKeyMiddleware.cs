using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Net.Http.Headers;
using System.Security.Claims;
using WebApiThrottle;

namespace MechaChat.API.Handlers
{
    public class APIAutoKeyMiddleware : OwinMiddleware
    {
        public APIAutoKeyMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var headerTokenAuth = context.Request.Headers.Get("Authorization");

            if (headerTokenAuth != null)
            {
                try
                {
                    if (headerTokenAuth.StartsWith("Bot"))
                    {
                        string headerToken = headerTokenAuth.Replace("Bot ", "");

                        var GetAPIKeyData = await MySQLHandler.SelectSingleQuery<Models.UserAPIKeysModel>($"SELECT * FROM `user_apikeys` WHERE `api_key` = '{headerToken}' AND `api_type` = 'bot' LIMIT 1");

                        if (GetAPIKeyData != null)
                        {
                            //new ThrottlePolicyRule().LimitPerSecond = 50;

                            var apiKeyClaim = new Claim("apikey", headerToken);
                            var subject = new Claim(ClaimTypes.Role, "bot");
                            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { apiKeyClaim, subject }, "ApiKey"));
                            context.Authentication.User = principal;

                            context.Response.StatusCode = 200;
                            await Next.Invoke(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("API key rejected or not found");
                        }
                    }
                    else if(headerTokenAuth.StartsWith("System"))
                    {
                        string headerToken = headerTokenAuth.Replace("System ", "");

                        var GetAPIKeyData = await MySQLHandler.SelectSingleQuery<Models.UserAPIKeysModel>($"SELECT * FROM `user_apikeys` WHERE `api_key` = '{headerToken}' AND `api_type` = 'system' LIMIT 1");

                        if (GetAPIKeyData != null)
                        {
                            var apiKeyClaim = new Claim("apikey", headerToken);
                            var subject = new Claim(ClaimTypes.Role, "system");
                            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { apiKeyClaim, subject }, "ApiKey"));
                            context.Authentication.User = principal;

                            context.Response.StatusCode = 200;
                            await Next.Invoke(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("API key rejected or not found");
                        }
                    }
                    else if (headerTokenAuth.StartsWith("User"))
                    {
                        string headerToken = headerTokenAuth.Replace("User ", "");

                        var GetAPIKeyData = await MySQLHandler.SelectSingleQuery<Models.UserAPIKeysModel>($"SELECT * FROM `user_apikeys` WHERE `api_key` = '{headerToken}' AND `api_type` = 'user' LIMIT 1");

                        if (GetAPIKeyData != null)
                        {
                            var apiKeyClaim = new Claim("apikey", headerToken);
                            var subject = new Claim(ClaimTypes.Role, "user");
                            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { apiKeyClaim, subject }, "ApiKey"));
                            context.Authentication.User = principal;

                            context.Response.StatusCode = 200;
                            await Next.Invoke(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("API key rejected or not found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API key rejected or not found");
            }
        }
    }
}
