using Dazzling.Studio.Utils;
using Uvendia.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class LoginModel : ContentModel
    {
        public LoginModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public LoginModel(IPublishedContent content) : base(content)
        {   
            AuthenticateModel = new AuthenticateModel(content);
            RegisterModel = new RegisterModel(content);
        }

        public AuthenticateModel AuthenticateModel { get; set; }
        public RegisterModel RegisterModel { get; set; }

        internal string GetRedirectUri()
        {
            return $"{WebUtils.GetApplicationUrlPath(HttpContext.Current)}{CurrentUser.LanguageCode}/account/login-register/";
        }
    }
}