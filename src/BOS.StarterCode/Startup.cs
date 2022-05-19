using BOS.Auth.Client;
using BOS.Email.Client;
using BOS.IA.Client;
using BOS.StarterCode.Data;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Policy.Auth;
using BOS.StarterCode.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Multitenancy.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
namespace BOS.StarterCode
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddControllers().AddNewtonsoftJson();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<Context>(options => options.UseMySql(Configuration.GetConnectionString("StarterCode-MySqlDB")));
            //***If your Application is Multitenant Application, please comment the above one line and uncomment the below line*** 
            //services.AddDbContext<Context>(o => { });
            AddCors(services);
            //Configuring BOS Services
            HttpClient httpclient = new HttpClient();
            services.AddScoped<IAuthClient>(s => new AuthClient(httpclient));
            services.AddScoped<IIAClient>(s => new IAClient(httpclient));
            services.AddScoped<IEmailClient>(s => new EmailClient(httpclient));
            services.AddScoped<IMultitenancyClient>(s => new MultitenancyClient(httpclient, "", null));
            //Configuring Auth Policy
            ConfigureAuthPolicies(services, Configuration);
            services.AddSingleton<IAuthorizationHandler, AdminOnlyHandler>();
            services.AddSingleton<IAuthorizationHandler, IsAuthenticatedHandler>();
            services.AddScoped<SessionHelpers, SessionHelpers>();
            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            int sessionTimeOut = Configuration["SessionIdleTimeout"] != null ? Convert.ToInt16(Configuration["SessionIdleTimeout"]) : 20; //Setting the default idle session timeout to 20 minutes if not in the appsettings file
            services.AddSession(
                s =>
                {
                    s.IdleTimeout = TimeSpan.FromMinutes(sessionTimeOut);
                    s.Cookie.HttpOnly = true;
                }
            );
            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (ctx, next) =>
            {
                await next();
                if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = ctx.Request.Path.Value;
                    ctx.Items["originalPath"] = originalPath;
                    ctx.Request.Path = "/Home/PageNotFound";
                    await next();
                }
            });
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();
            var cachePeriod = env.EnvironmentName == "Development" ? "600" : "604800";
            app.UseStaticFiles(
               new StaticFileOptions
               {
                   OnPrepareResponse = ctx =>
                   {
                       ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                   }
               });
            app.UseCookiePolicy();
            app.UseSession();
            app.UseCors("CorsPolicy");
            //app.UseAppConfigurationMiddleware();
            //The Landping page of the application changes based on whether or not the BOS APIs are enabled
            string landingPage = "Auth";
            var enabledBOSapis = Configuration.GetSection("BOS:EnabledAPIs").Get<List<string>>();
            if (enabledBOSapis != null)
            {
                if (enabledBOSapis.Contains("Authentication"))
                {
                    landingPage = "Auth";
                }
                else
                {
                    landingPage = "Home";
                }
            }
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=" + landingPage + "}/{action=Index}/{id?}");
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
                RequestPath = "/images"
            });
        }
        private void ConfigureAuthPolicies(IServiceCollection services, IConfiguration configuration)
        {
            // configure jwt authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Auth");
                options.LoginPath = new PathString("/Auth");
                options.LogoutPath = new PathString("/Auth");
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.Requirements.Add(new IsAuthenticatedRequirement());
                    policy.Requirements.Add(new AdminOnlyRequirement(new string[] { "Admin", "Super Admin" }));
                });
                options.AddPolicy("IsAuthenticated", policy =>
                {
                    policy.Requirements.Add(new IsAuthenticatedRequirement());
                });
            });
        }
        private void AddCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
            options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            });
        }
    }
}
