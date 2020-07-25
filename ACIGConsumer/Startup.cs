using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ACIGConsumer.Factories;
using ACIGConsumer.Models;
using Core;
using Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services;
using Services.Interfaces;
using Services.RequestHandler;

namespace ACIGConsumer
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSingleton<IFileProvider>(
           new PhysicalFileProvider(
               Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            //this line to register the ApplConfig in appsettings and retreive the values
            services.Configure<ApplConfig>(Configuration.GetSection("ApplConfig"));
            RegisterPluginServices(services);
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(60);//You can set Time   
            });

        }

        public void RegisterPluginServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddTransient<IAuthService, AuthService>();
            services.TryAddScoped<ACIGDbContext, ACIGDbContext>();
            services.AddScoped<IUnitOfWorks, UnitOfWorks>();
            services.TryAddTransient<ICustomerService, CustomerService>();
            services.TryAddTransient<ApprovalsHandler, ApprovalsHandler>();
            services.TryAddTransient<PolicyHandler, PolicyHandler>();
            services.TryAddTransient<CoverageBalanceHandler, CoverageBalanceHandler>();
            services.TryAddTransient<CustomerHandler, CustomerHandler>();
            services.TryAddTransient<ProvidersHandler, ProvidersHandler>();
            services.TryAddTransient<ClaimsHandler, ClaimsHandler>();
            services.TryAddTransient<IListFactory, ListFactory>();
            services.TryAddTransient<IPolicyFactory, PolicyFactory>();
            services.TryAddTransient<IApprovalFactory, ApprovalFactory>();
            services.TryAddTransient<IFileService, FileService>();
            services.TryAddTransient<TOBHandler, TOBHandler>();
            services.TryAddTransient<GetLang, GetLang>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseForwardedHeaders();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
         
            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Index}/{id?}");
            });
            loggerFactory.AddFile("Logs/mylog-{Date}.txt");
            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            ContextHelper.Configure(httpContextAccessor);
        }
    }
}
