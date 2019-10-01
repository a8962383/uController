using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
// This assembly attribute is part of the generated code to help register the routes
[assembly: Microsoft.AspNetCore.Routing.EndpointRouteProviderAttribute(typeof(Samples.MyHandler_Generated))]
namespace Samples
{
    public class MyHandler_Generated : Microsoft.AspNetCore.Routing.IEndpointRouteProvider
    {
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task Get(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var context = httpContext;
            await handler.Get(context);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task Blah(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var result = handler.Blah();
            await new uController.ObjectResult(result).ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task StatusCode(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var statusValue = httpContext.Request.RouteValues["status"]?.ToString();
            if (statusValue == null || !System.Int32.TryParse(statusValue, out var status))
            {
                status = default;
            }
            var result = handler.StatusCode(status);
            await result.ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task SlowTaskStatusCode(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var result = await handler.SlowTaskStatusCode();
            await result.ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task FastValueTaskStatusCode(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var loggerFactory = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var result = await handler.FastValueTaskStatusCode(loggerFactory);
            await result.ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task DoAsync(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var context = httpContext;
            var q = httpContext.Request.Query["q"].ToString();
            await handler.DoAsync(context, q);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task HelloDavid(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var result = handler.HelloDavid();
            await new uController.ObjectResult(result).ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task GetAsync(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var name = httpContext.Request.RouteValues["name"]?.ToString();
            var result = await handler.GetAsync(name);
            await new uController.ObjectResult(result).ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task Hello(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var result = handler.Hello();
            await new uController.ObjectResult(result).ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task Post(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var reader = httpContext.RequestServices.GetRequiredService<uController.IHttpRequestReader>();
            var obj = (System.Text.Json.JsonElement)await reader.ReadAsync(httpContext, typeof(System.Text.Json.JsonElement));
            var result = handler.Post(obj);
            await result.ExecuteAsync(httpContext);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task PostAForm(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            var form = await httpContext.Request.ReadFormAsync();
            handler.PostAForm(form);
        }
        
        [System.Diagnostics.DebuggerStepThroughAttribute]
        public async System.Threading.Tasks.Task Authed(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var handler = new Samples.MyHandler();
            handler.Authed();
        }
        
        void Microsoft.AspNetCore.Routing.IEndpointRouteProvider.MapRoutes(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder routes)
        {
            routes.Map("/", Get).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/blah", Blah).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/status/{status}", StatusCode).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/slow/status/{status}", SlowTaskStatusCode).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/fast/status/{status}", FastValueTaskStatusCode).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/lag", DoAsync).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/hey/david", HelloDavid).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/hey/{name?}", GetAsync).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/hello", Hello).WithMetadata(new uController.HttpGetAttribute());
            routes.Map("/", Post).WithMetadata(new uController.HttpPostAttribute());
            routes.Map("/post-form", PostAForm).WithMetadata(new uController.HttpPostAttribute());
            routes.Map("/auth", Authed).WithMetadata(new uController.HttpGetAttribute(), new Microsoft.AspNetCore.Authorization.AuthorizeAttribute());
        }
    }
}
