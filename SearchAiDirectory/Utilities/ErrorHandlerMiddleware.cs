namespace SearchAiDirectory.Utilities;

public class ErrorHandlerMiddleware(IServiceScopeFactory serviceScopeFactory, RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            var newLog = new AppLog
            {
                User = context.User is not null && context.User.Claims is not null && context.User.Claims.Any(a => a.Type == "unique_name")
                ? context.User.Claims.First(f => f.Type == "unique_name").Value : string.Empty,

                UserIp = !string.IsNullOrEmpty(context.Connection.RemoteIpAddress.ToString())
                ? context.Connection.RemoteIpAddress.ToString() : string.Empty,

                Host = context.Request.Host.HasValue
                ? context.Request.Host.Value : string.Empty,

                Url = context.Request.GetDisplayUrl(),

                Code = error is KeyNotFoundException ? (int)HttpStatusCode.NotFound
                : error is ApplicationException ? (int)HttpStatusCode.BadRequest
                : error is UnauthorizedAccessException ? (int)HttpStatusCode.Forbidden
                : error is ArgumentNullException ? (int)HttpStatusCode.BadRequest
                : error is InvalidOperationException ? (int)HttpStatusCode.BadRequest
                : error is NotSupportedException ? (int)HttpStatusCode.NotImplemented
                : (int)HttpStatusCode.InternalServerError,

                Message = error.Message.Length > 500
                ? error.Message[..500] : error.Message,

                StackTrace = !string.IsNullOrEmpty(error.StackTrace)
                ? error.StackTrace.Length > 1000 ? error.StackTrace[..1000] : error.StackTrace.ToString() : string.Empty,

                Created = DateTime.UtcNow
            };

            using var scope = serviceScopeFactory.CreateScope();
            ILogServices logService = scope.ServiceProvider.GetRequiredService<ILogServices>();
            await logService.AddAppLog(newLog);

            context.Response.Redirect("/Error");
        }
    }
}
