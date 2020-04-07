using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace cw3.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var path = httpContext.Request.Path;
            var method = httpContext.Request.Method;
            String queryString = httpContext.Request.QueryString.ToString();
            var body = "";
            using (var read = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true)) 
            {
                body = await read.ReadToEndAsync();
                httpContext.Request.Body.Position = 0;
            }
            var pathFile = @"data.txt";

            string[] lines = { "Path: "+path, "Method: "+method, "QueryString: "+queryString , "Body: "+body,"*********" };
            File.AppendAllLines(pathFile, lines);

            if (_next != null) 
            {
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                await _next(httpContext);
            }
        }
    }
}
