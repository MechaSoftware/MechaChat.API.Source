using Microsoft.AspNetCore.Http;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace MechaChat.API.Filters
{
    public class SchemesFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry reg, IApiExplorer explorer)
        {
            swaggerDoc.schemes = new string[] { "https" };
            swaggerDoc.basePath = "https://api.mecha.chat";
            swaggerDoc.host = "https://api.mecha.chat";
        }
    }
}
