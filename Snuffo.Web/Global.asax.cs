using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web;
using System.Security.Cryptography;
using System.Web.Security;
using System.Web.Mvc;
using Dazzling.Studio.Utils;
using Umbraco.Web.Composing;
using Snuffo.Web.Controllers;
using System.Web.Helpers;

namespace Snuffo.Web
{
    public class MvcApplication : UmbracoApplication
    {
        public override void Init()
        {
            HttpApplication app = this as HttpApplication;
            app.PreRequestHandlerExecute += PreRequestHandlerExecute;

            base.Init();
        }

        private new void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (!Context.Request.IsSecureConnection)
            {
                Response.Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
            }
        }

        protected override void OnApplicationError(object sender, EventArgs e)
        {
            var error = HttpContext.Current.Server.GetLastError();
            if (error is CryptographicException)
            {
                FormsAuthentication.SignOut();
            }
            var errorInfo = new HandleErrorInfo(error, "Controller", "Action");
            error.HelpLink = $"{WebUtils.GetApplicationUrlPath(HttpContext.Current).TrimEnd('/')}{HttpContext.Current.Request.RawUrl}";

            // Mail Exception.
            MailController.Instance(new HttpRequestWrapper(HttpContext.Current.Request), Current.UmbracoContext.PublishedRequest.PublishedContent, "").MailError(errorInfo).SendAsync();
        }
    }
}