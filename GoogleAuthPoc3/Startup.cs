using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoogleAuthPoc3
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
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddOpenIdConnect(o =>
                {
                    var hostedDomain = new KeyValuePair<string, string>("hd", "crucialexams.com");

                    o.ClientId = "1059992699070-hc0haq5blluj1ijjfjmfjgk9tltoqgeo.apps.googleusercontent.com";
                    o.ClientSecret = "L8bJ2gtR7T9rw9Nmm12oxFaN";
                    o.Authority = "https://accounts.google.com";
                    o.ResponseType = "id_token token";
                    o.Scope.Add("openid");
                    o.Scope.Add("email");
                    o.Scope.Add("profile");
                    o.GetClaimsFromUserInfoEndpoint = true;
                    o.SaveTokens = true;
                    o.Events = new OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProvider = (context) =>
                        {
                            // Add domain limiting using 'hd' or 'hosted domain' parameter
                            // Docs: https://developers.google.com/identity/protocols/OpenIDConnect#hd-param
                            //context.ProtocolMessage.SetParameter(hostedDomain.Key, "asdf.com");

                            // Set up redirect URLs
                            if (context.Request.Path != "/account/external")
                            {
                                context.Response.Redirect("/account/login");
                                context.HandleResponse();
                            }

                            return Task.FromResult(0);
                        },

                        OnTokenValidated = (c) =>
                        {
                            var hdClaim = c.SecurityToken.Claims.FirstOrDefault(claim => claim.Type == hostedDomain.Key);
                            if(hdClaim?.Value == null || hdClaim.Value != hostedDomain.Value)
                            {
                                // The claim is null or value is not the trusted google domain - do not authenticate!
                                c.Fail($"Invalid claim for '{hostedDomain.Key}'!  User does not belong to trusted G Suite Domain");
                            }
                            return Task.FromResult(0);
                        }

                        
                    };
                });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
