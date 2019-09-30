using Auth0.AuthenticationApi.Models;
using Dazzling.Studio.Utils;
using Uvendia.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class AuthenticateModel : ContentModel
    {
        readonly Auth0Helper _auth0Helper;
        string _googleAuthUrl, _facebookAuthUrl, _twitterAuthUrl;

        public AuthenticateModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public AuthenticateModel(IPublishedContent content) : base(content)
        {  
            _auth0Helper = new Auth0Helper();
        }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]        
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string GoogleAuthUrl
        {
            get
            {   
                SetUrls();
                return _googleAuthUrl;
            }
        }

        public string FacebookAuthUrl
        {
            get
            {
                SetUrls();
                return _facebookAuthUrl;
            }
        }

        public string TwitterAuthUrl
        {
            get
            {
                SetUrls();
                return _twitterAuthUrl;
            }
        }

        public string ReturnUrl { get; set; }

        private void SetUrls()
        {
            if (!_googleAuthUrl.IsNullOrEmpty()) return;

            var client = _auth0Helper.CreateAuthenticationApiClientIfNotExists();
            var returnUrl = $"{WebUtils.GetApplicationUrlPath(HttpContext.Current)}{CurrentUser.LanguageCode}/account/login-register/";
            if (!ReturnUrl.IsNullOrEmpty())
            {
                returnUrl = $"{returnUrl}?returnUrl={ReturnUrl}";
            }

            var google_authorizationUrl = client.BuildAuthorizationUrl()
                .WithResponseType(AuthorizationResponseType.Code)
                .WithClient(Auth0Helper.Auth0ClientId)
                .WithConnection("google-oauth2")
                .WithRedirectUrl(returnUrl)
                .WithScope("openid profile email")
                .Build();

            _googleAuthUrl = google_authorizationUrl.ToString();

            var facebook_authorizationUrl = client.BuildAuthorizationUrl()
                .WithResponseType(AuthorizationResponseType.Code)
                .WithClient(Auth0Helper.Auth0ClientId)
                .WithConnection("facebook")
                .WithRedirectUrl(returnUrl)
                .WithScope("openid profile email")
                .Build();
            _facebookAuthUrl = facebook_authorizationUrl.ToString();

            var twitter_authorizationUrl = client.BuildAuthorizationUrl()
                .WithResponseType(AuthorizationResponseType.Code)
                .WithClient(Auth0Helper.Auth0ClientId)
                .WithConnection("twitter")
                .WithRedirectUrl(returnUrl)
                .WithScope("openid profile email")
                .Build();
            _twitterAuthUrl = twitter_authorizationUrl.ToString();
        }

    }
}