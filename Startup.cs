using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Linq;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Owin;
using MechaChat.API;
using MechaChat.API.ErrorHandling;
using MechaChat.API.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MultipartDataMediaFormatter;
using MultipartDataMediaFormatter.Infrastructure;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using WebApiThrottle;
using MechaChat.API.Attributes;
using System.Web.Http.Dependencies;
using NSwag.AspNet.Owin;
using NSwag.AspNet.Owin.Middlewares;
using NSwag.Generation.WebApi;
using NSwag.AspNetCore;
using Microsoft.Extensions.FileProviders;
using System.Web.Handlers;
using Microsoft.DotNet.PlatformAbstractions;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Web.Http.Results;
using Azure.Core;
using Microsoft.Owin.FileSystems;
using NSwag.Annotations;
using NSwag.AspNetCore.Middlewares;
using NSwag.Collections;
using System.Web.Http.Routing;
using MechaChat.API.Filters;

[assembly: OwinStartup(typeof(Startup))]

namespace MechaChat.API
{
    public class Startup
    {
        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        [Obsolete]
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MessageHandlers.Add(new ThrottlingHandler()
            {
                Policy = new ThrottlePolicy(perSecond: 50)
                {
                    IpThrottling = true,
                    ClientThrottling = true
                },
                Repository = new MemoryCacheRepository(),
                QuotaExceededResponseCode = (System.Net.HttpStatusCode)429,
                QuotaExceededMessage = "Too many requests! We can only allow {0} per {1}"
            });

            config.MapHttpAttributeRoutes();

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter(new MultipartFormatterSettings()));

            config.Services.Replace(typeof(IExceptionHandler), new PassthroughExceptionHandler());

            config.EnableCors();

            var serviceProvider = BuildServiceProvider();
            config.DependencyResolver = new DefaultDependencyResolver(serviceProvider);

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Robots-Tag", new[] { "noindex, nofollow, noarchive, nosnippet" });
                context.Response.Headers.Add("Content-Type", new[] { "application/json" });
                context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
                context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Access-Control-*, Origin, X-Requested-With, Content-Type, Accept, Authorization" });
                context.Response.Headers.Add("Access-Control-Expose-Headers", new[] { "Access-Control-*, Origin, X-Requested-With, Content-Type, Accept, Authorization" });
                context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "HEAD, GET, POST, OPTIONS, PUT, PATCH, DELETE" });
                context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "false" });
                context.Response.Headers.Add("Access-Control-Max-Age", new[] { "86400" });

                await next();
            });

            config.Routes.MapHttpRoute("brew_coffee", "brew/coffee");

            app.Map("/brew/coffee", spa =>
            {
                spa.Run((context) =>
                {
                    context.Response.StatusCode = 418;
                    return context.Response.WriteAsync("I'm a tea pot error");
                });
            });

            string root = AppDomain.CurrentDomain.BaseDirectory;
            var physicalFileSystem = new PhysicalFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Swagger"));

            var options = new Microsoft.Owin.StaticFiles.FileServerOptions
            {
                RequestPath = PathString.Empty,
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };

            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.EnableDirectoryBrowsing = false;

            app.Map("/docs", spa =>
            {
                spa.UseFileServer(options);
            });

            app.UseSwaggerUi3((c) =>
            {
                c.DocumentPath = "/docs/v1/swagger.json";
                c.Path = "/v1/docs";
                c.EnableTryItOut = false;
                c.ServerUrl = "https://api.mecha.chat/v1";
                c.CustomStylesheetPath = "/docs/assets/dark-theme.css";
                c.CustomJavaScriptPath = "/docs/assets/custom-logo.js";
                c.DocumentTitle = "MechaChat v1 Docs";
                c.PostProcess = (doc) =>
                {
                    doc.Info = new NSwag.OpenApiInfo
                    {
                        Title = "MechaChat v1 Docs",
                        Version = "v1"
                    };
                    doc.Host = "https://api.mecha.chat/v1";
                };
            });

            /*
            app.UseSwaggerUi3((c) =>
            {
                c.DocumentPath = "/docs/v1/swagger.json";
                c.Path = "/v2/docs";
                c.EnableTryItOut = false;
                c.ServerUrl = "https://api.mecha.chat/v2";
                c.CustomStylesheetPath = "/docs/assets/dark-theme.css";
                c.CustomJavaScriptPath = "/docs/assets/custom-logo.js";
                c.DocumentTitle = "MechaChat v2 Docs";
                c.PostProcess = (doc) =>
                {
                    doc.Info = new NSwag.OpenApiInfo
                    {
                        Title = "MechaChat v2 Docs",
                        Version = "v2"
                    };
                    doc.Host = "https://api.mecha.chat/v2";
                };
            });
            */

            app.Map("/version", spa =>
            {
                spa.Run((context) =>
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";

                    var jsonVersion = JsonConvert.SerializeObject(new { version = typeof(Program).Assembly.GetName().Version.ToString(1) });

                    return context.Response.WriteAsync(jsonVersion);
                });
            });

            app.Use<APIAutoKeyMiddleware>().Use<OopsHandlerMiddleware>().UseWebApi(config);

            config.EnsureInitialized();
        }
    }

    public class DefaultDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider provider;

        public DefaultDependencyResolver(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public object GetService(Type serviceType)
        {
            return provider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return provider.GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
