using System.Web.Http.ExceptionHandling;

namespace MechaChat.API.ErrorHandling
{
    public class OopsHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            context.Result = new OopsResult(context.Request);
        }
    }
}
