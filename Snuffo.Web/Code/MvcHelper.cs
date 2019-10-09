using Uvendia.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Snuffo.Web
{
    public static class MvcHelper
    {
        public static void CheckAuthenticated(this HtmlHelper html, string langCode)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                var context = html.ViewContext.RequestContext.HttpContext;
                
                SnuffoSettings.ShowMessage(html.ViewContext.TempData, "Inloggen", "Je moet eerst ingelogd zijn");
                var redirectUrl = string.Concat("/", langCode, "/account/login-register/?returnUrl=/", langCode, "/account/my-profile/");

                context.Response.Redirect(redirectUrl);
            }
        }
    }
}