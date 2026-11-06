using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Services;
using HypnoTech.Common.StaticWebAssets;
using HypnoTech.Frontend.Ajax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using HypnoTech.Common.Security;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Caching;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

namespace HypnoTech.Frontend.Startup
{
    public static class StartupExtensions
    {
        public static void UseHypnoTech(this IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddTransient<IUploadedFilesService, UploadedFilesService>();
            services.TryAddSingleton<IActionResultExecutor<AjaxPartialViewResult>, AjaxPartialViewResultExecutor>();
            services.AddScoped<ApiAuthorizeAttribute>();
            StaticFiles.RegisterStaticFiles(env);

            //This is needed to allow AjaxPartialViewResult to be written.
            //See: https://stackoverflow.com/questions/47735133/asp-net-core-synchronous-operations-are-disallowed-call-writeasync-or-set-all
            //TODO: Work out what is needed to convert whatever option is throwing this error to async.
            //TODO: Check on server what needs to be done as we will not be hiding behind IIS there.
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddMvc();
        }

        public static void ConfigureHypnoTech(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles();
            }
            else
            {
                app.UseExceptionHandler("/Error/500");

                app.UseHsts();

                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = context =>
                    {
                        context.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=31536000";
                    }
                });
            }

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                CheckConsentNeeded = context => false,
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Strict
            });

            app.UseSecureResponseHeaders(config);

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            NavBarCache.Init(app.ApplicationServices.GetService<INavBarRepository>());
        }
    }
}
