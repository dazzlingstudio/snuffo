using Uvendia.Domain;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using Umbraco.Web;
using Umbraco.Web.Security;

[assembly: OwinStartup("SnuffoOwinStartup", typeof(Snuffo.Web.SnuffoOwinStartup))]

namespace Snuffo.Web
{
    public class SnuffoOwinStartup : UmbracoDefaultOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            base.Configuration(app);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            // Enable Kentor Cookie Saver middleware
            app.UseKentorOwinCookieSaver();

            // Set Cookies as default authentication type
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString($"/{CurrentUser.LanguageCode}/account/login-register/")
            });

            // We need to set these values again after our custom changes. Otherwise preview doesn't work.
            app.UseUmbracoBackOfficeCookieAuthentication(this.UmbracoContextAccessor, this.RuntimeState, this.Services.UserService,  this.GlobalSettings, this.UmbracoSettings.Security)
                .UseUmbracoBackOfficeExternalCookieAuthentication(this.UmbracoContextAccessor, this.RuntimeState, this.GlobalSettings)
                .UseUmbracoPreviewAuthentication(this.UmbracoContextAccessor, this.RuntimeState, this.GlobalSettings, this.UmbracoSettings.Security);            
        }
    }
}