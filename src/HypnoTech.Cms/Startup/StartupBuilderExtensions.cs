using System;
using HypnoTech.Cms.Helpers;
using HypnoTech.Cms.Mvc;
using HypnoTech.Cms.Services;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Emails;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.AutoRegisterDi;
using Vereyon.Web;
using HypnoTech.Common.Services;

namespace HypnoTech.Cms.Startup
{
    public static class StartupBuilderExtensions
    {
        public static IMvcBuilder AddCms(this IServiceCollection services, IConfiguration configuration, Action<MvcOptions> setupAction = null)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            //This is needed to allow AjaxPartialViewResult to be written.
            //See: https://stackoverflow.com/questions/47735133/asp-net-core-synchronous-operations-are-disallowed-call-writeasync-or-set-all
            //TODO: Work out what is needed to convert whatever option is throwing this error to async.
            //TODO: Check on server what needs to be done as we will not be hiding behind IIS there.
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => 
                { 
                    options.LoginPath = "/Home/Login";
                    options.Cookie.IsEssential = true;
                });

            var mvc = services.AddMvc()
                .ConfigureApplicationPartManager(m => m.FeatureProviders.Add(new CmsControllerFeatureProvider()))
                .AddControllersAsServices();

            services.Configure<MvcOptions>(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
                options.Conventions.Add(new CmsControllerRouteConvention());
            });

            services.UseEmails();
            services.AddFlashMessage();

            services.Configure<CmsOptions>(configuration.GetSection("cms"));

            services.AddTransient(typeof(ICmsService<>), typeof(CmsService<>));
            services.AddTransient<IUploadedFilesService, UploadedFilesService>();

            services.RegisterAssemblyPublicNonGenericClasses()
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces();

            services.AddTransient<LinkHelper>();

            services.Configure(setupAction);

            return mvc;
        }

        public static void ConfigureCms(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
                app.UseHsts();
            }

            app.UseSecureResponseHeaders(config);

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                CheckConsentNeeded = context => false,
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
