using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MultiSpa.Web
{
	public class AngularDefaultRouteMiddleware
	{
		private readonly RequestDelegate _next;

		public AngularDefaultRouteMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.Request.Path == "/")
			{
				await context.Response.WriteAsync(await GetAngularSpa(context.Request.PathBase));
			}
			else
			{
				await _next(context);
				if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
				{
					await context.Response.WriteAsync(await GetAngularSpa(context.Request.PathBase));
				}
			}
		}

		private async Task<string> GetAngularSpa(string pathBase)
		{
			string html = await System.IO.File.ReadAllTextAsync($"{Environment.CurrentDirectory}\\wwwroot\\index.html");
			return html.Replace("<base href='/'>", $"<base href='{pathBase}'>");
		}
	}

	public static class AngularDefaultRouteMiddlewareExtensions
	{
		public static IApplicationBuilder UseAngularDefaultRoute(this IApplicationBuilder app)
		{
			return app.UseMiddleware<AngularDefaultRouteMiddleware>();
		}
	}
}