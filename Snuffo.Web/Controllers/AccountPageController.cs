using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using RestSharp;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Snuffo.Web.Controllers
{
    public class AccountPageController : RenderMvcController
    {
        private AuthenticationApiClient _client;
        private Auth0Helper _auth0Helper;

        public AccountPageController()
        {
            _auth0Helper = new Auth0Helper();
            _client = _auth0Helper.CreateAuthenticationApiClientIfNotExists();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Login(ContentModel model, string returnUrl = "/", string code = "")
        {
            var loginModel = new Models.LoginModel(model.Content);
            if (!code.IsNullOrEmpty())
            {
                var result = _client.GetTokenAsync(new AuthorizationCodeTokenRequest
                {
                    ClientId = Auth0Helper.Auth0ClientId,
                    ClientSecret = Auth0Helper.Auth0ClientSecret,
                    Code = code,
                    RedirectUri = loginModel.GetRedirectUri()
                }).GetAwaiter().GetResult();


                UserInfo user = _client.GetUserInfoAsync(result.AccessToken).GetAwaiter().GetResult();

                // Authenticate
                _auth0Helper.Authenticate(user, true, AuthenticationManager);

                returnUrl = returnUrl == "/" ? $"/{CurrentUser.LanguageCode}/" : returnUrl;
                
                return RedirectToLocal(returnUrl, loginModel);
            }

            return CurrentTemplate(loginModel);
        }

        public ActionResult Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return Redirect(string.Concat("/", CurrentUser.LanguageCode, "/"));
        }

        private ActionResult RedirectToLocal(string returnUrl, ContentModel model)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect($"/{CurrentUser.LanguageCode}/");
            }
        }
    }
}