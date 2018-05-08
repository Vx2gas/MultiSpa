﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiSpa.Web.Data;
using MultiSpa.Web.Models;
using MultiSpa.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using System.Threading;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Http.Extensions;

namespace MultiSpa.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.    
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, EmailSender>();

			services.AddMvc()
				.AddRazorPagesOptions(options =>
				{
					options.Conventions.AuthorizeFolder("/members");
				});


			// In production, the Angular files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";

            });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
			

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Map(new PathString("/members"), appRenewals =>
            {
				//app.UseAngularDefaultRoute();

				//appRenewals.UsePathBase(new PathString("/"));


				//appRenewals.UseRewriter(new Microsoft.AspNetCore.Rewrite.RewriteOptions()
				//	.AddRewrite(@"^members/", "/", false));

				// setup custom middleware to set the current thread principal to the user
				appRenewals.Use(async (context, next) =>
				{
					var httpContext = (context as HttpContext);
					//httpContext.Request.PathBase = new PathString("/members");
					if (httpContext.User.Identity.IsAuthenticated)
					{
						Thread.CurrentPrincipal = httpContext.User;
						await next();
					}
					else
					{
						
						context.Response.Redirect($"/account/login?ReturnUrl={context.Request.GetEncodedPathAndQuery()}");
					}
				});

				appRenewals.UseSpaStaticFiles();
				//appRenewals.UsePathBase(new PathString("/members"));
				appRenewals.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        //spa.UseAngularCliServer(npmScript: "start");

						spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
					}
                });
            });

        }
    }
}