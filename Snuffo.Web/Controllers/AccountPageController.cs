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
using Snuffo.Web.Models;
using Uvendia.Domain.Repositories;

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

                returnUrl = returnUrl == "/" ? $"/{CurrentUser.LanguageCode}/account/my-profile/" : returnUrl;
                
                return RedirectToLocal(returnUrl, loginModel);
            }

            return CurrentTemplate(loginModel);
        }

        public ActionResult Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return Redirect(string.Concat("/", CurrentUser.LanguageCode, "/account/login-register/"));
        }

        [Authorize]
        public ActionResult AccountOrders(ContentModel model)
        {
            return CurrentTemplate(model);
        }

        [Authorize]
        public ActionResult AccountAddress(ContentModel model)
        {
            var aam = new AccountAddressModel(model.Content);
            var addresess = UvendiaContext.Addresses.AllByUserId(CurrentUser.UserId);
            var shipAddress = addresess.FirstOrDefault(x => x.AddressType == Uvendia.Domain.Enums.AddressType.ShipAddress);
            if (shipAddress == null)
            {
                aam.ShippingAddress = new Address();
                aam.HasSameAddress = true;
            }
            else
            {
                aam.HasSameAddress = false;
                aam.ShippingAddress = shipAddress;
            }

            aam.ContactAddress = addresess.FirstOrDefault(x => x.AddressType == Uvendia.Domain.Enums.AddressType.Default) ?? new Address();

            return CurrentTemplate(aam);
        }

        [Authorize]
        public ActionResult AccountChangeEmail(ContentModel model)
        {
            return CurrentTemplate(model);
        }

        [Authorize]
        public ActionResult AccountChangePassword(ContentModel model)
        {
            return CurrentTemplate(model);
        }

        [Authorize]
        public ActionResult AccountDelete(ContentModel model)
        {
            return CurrentTemplate(model);
        }

        [Authorize]
        public ActionResult AccountProfile(ContentModel model)
        {
            var auth0Helper = new Auth0Helper();
            var user = auth0Helper.GetAuth0User(CurrentUser.UserId);
            var customer = new AccountProfileModel(model.Content)
            {
                UserId = user.UserId,
                EmailAddress = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.UserMetadata != null ? user.UserMetadata.Phone : "",
                SubscribedToNewsletter = user.UserMetadata != null ? Convert.ToBoolean(user.UserMetadata.SubscribedToNewsletter) : false,
            };

            return CurrentTemplate(customer);
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