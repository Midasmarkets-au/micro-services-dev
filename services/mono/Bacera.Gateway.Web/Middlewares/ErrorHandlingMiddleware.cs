using Bacera.Gateway.MyException;

namespace Bacera.Gateway.Web.Middlewares;

public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (TokenInvalidException e)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(e.Message);
        }
        catch (BadHttpRequestException ex) when (ex.Message.Contains("Request body too large"))
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Request body too large");
        }
        // catch (Exception)
        // {
        //     // var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        //     if (!Startup.IsProduction()) throw;
        //     context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //     await context.Response.WriteAsync("");
        // }
    }
}