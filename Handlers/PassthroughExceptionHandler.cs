using System.Runtime.ExceptionServices;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace MechaChat.API.ErrorHandling
{
    public class PassthroughExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            var info = ExceptionDispatchInfo.Capture(context.Exception);
            info.Throw();
        }
    }
}
