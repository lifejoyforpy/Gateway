using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace webapi
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("cookie", options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            }).AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "http://localhost:5000/IdentityServer";
                options.SignInScheme = "cookie";
                options.RequireHttpsMetadata = false;
                options.ClientId = "mvc";
                options.ClientSecret = "secret";
                options.ResponseType = "code id_token";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("api1");
                options.Scope.Add("offline_access");

            }).AddIdentityServerAuthentication(options =>
            {
                options.Authority= "http://localhost:5000/IdentityServer";
                options.RequireHttpsMetadata = false;
                options.ApiSecret = "secret123";
                options.ApiName = "api1";
                options.SupportedTokens = SupportedTokens.Both;
            });
            services.AddAuthorization(option =>
            {
                //默认 只写 [Authorize]，表示使用oidc进行认证
                option.DefaultPolicy = new AuthorizationPolicyBuilder("oidc").RequireAuthenticatedUser().Build();
                //ApiController使用这个  [Authorize(Policy = "ApiPolicy")]，使用jwt认证方案
                option.AddPolicy("ApiPolicy", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
          

            app.UseSpaStaticFiles();
            app.Use(async (context, next) =>
            {
                context.Request.Host = HostString.FromUriComponent(new Uri("http://localhost:5000/"));
                await next.Invoke();
            });
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
