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
            SiteAssetManifest.Register(env);
            services.AddHostedService<NavBarCacheHostedService>();

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

        }
    }
}
