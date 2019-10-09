using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi.Models;
using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Enums;
using Uvendia.Domain.Factories;
using Uvendia.Domain.Repositories;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using reCaptcha;
using Snuffo.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Snuffo.Web.Controllers
{
    public class AccountController : BaseSurfaceController
    {
        private AuthenticationApiClient _client;
        private Auth0Helper _auth0Helper;

        public AccountController()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Authenticate(Models.AuthenticateModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = _client.GetTokenAsync(new ResourceOwnerTokenRequest
                    {
                        ClientId = Auth0Helper.Auth0ClientId,
                        ClientSecret = Auth0Helper.Auth0ClientSecret,
                        Scope = "openid profile email",
                        Realm = Auth0Helper.Connection, // Specify the correct name of your DB connection
                        Username = model.Email,
                        Password = model.Password
                    }).Result;

                    // Get user info from token
                    UserInfo user = _client.GetUserInfoAsync(result.AccessToken).Result;
                    if (user.EmailVerified.GetValueOrDefault())
                    {
                        _auth0Helper.Authenticate(user, model.RememberMe, AuthenticationManager);
                        return RedirectToLocal(returnUrl, model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please check your inbox. Your e-mail address need to be verified.");
                    }
                }
                catch (Exception e)
                {
                    HandleAuth0Exception(e);
                }
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Register(Models.RegisterModel model)
        {
            if (!ReCaptcha.Validate(DataUtils.GetConfig("reCaptcha:SecretKey")))
            {
                ModelState.AddModelError("", "Captcha cannot be empty");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var users = _auth0Helper.SearchAuth0UserByEmail(model.EmailAddress);
                    if (!users.Any())
                    {
                        var newUser = new SignupUserRequest();
                        newUser.ClientId = Auth0Helper.Auth0ClientId;
                        newUser.Connection = Auth0Helper.Connection;
                        newUser.Email = model.EmailAddress;
                        newUser.Password = model.Password;
                        newUser.UserMetadata = new { model.FirstName, model.LastName, model.PhoneNumber };

                        SignupUserResponse response = _client.SignupUserAsync(newUser).Result;

                        var customer = new AccountProfileModel(model.Content)
                        {
                            UserId = $"auth0|{response.Id}",
                            EmailAddress = model.EmailAddress,
                            FirstName = model.FirstName,
                            LastName = model.LastName
                        };

                        MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).RegistrationConfirmMail(customer).Send();
                        return Redirect($"/{CurrentUser.LanguageCode}/account/confirm-your-email-address/");
                    }
                    else
                    {
                        var unverifiedUser = users.FirstOrDefault(u => u.UserId.StartsWith("auth0") && u.EmailVerified == false);
                        if (unverifiedUser != null)
                        {
                            return Redirect($"/{CurrentUser.LanguageCode}/account/resend-email-verification/?u={unverifiedUser.UserId}");
                        }
                        ModelState.AddModelError("", "E-mail adress already exists");
                    }
                }                
                catch (Exception e)
                {
                    HandleAuth0Exception(e);
                }
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ResendEmailVerification(ContentModel model, string u)
        {
            var unverifiedUser = _auth0Helper.GetAuth0User(u);
            var customer = new AccountProfileModel(model.Content)
            {
                UserId = unverifiedUser.UserId,
                EmailAddress = unverifiedUser.Email,
                FirstName = unverifiedUser.FirstName,
                LastName = unverifiedUser.LastName
            };

            MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).RegistrationConfirmMail(customer).Send();
            return Redirect($"/{CurrentUser.LanguageCode}/account/confirm-your-email-address/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordRecovery(PasswordRecoveryModel model)
        {
            User user = _auth0Helper.SearchAuth0UserByEmail(model.Email)?.SingleOrDefault(x => x.UserId.StartsWith("auth0"));
            if (user == null) ModelState.AddModelError("", "E-mail not found");

            if (ModelState.IsValid)
            {
                MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).PasswordRecovery(user).Send();
                return Redirect($"/{CurrentUser.LanguageCode}/account/change-password-email/");
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            User user = null;
            var userId = Request.QueryString["u"] ?? CurrentUser.UserId;

            if (!userId.IsNullOrEmpty()) user = _auth0Helper.GetAuth0User(userId);
            if (user == null) ModelState.AddModelError("", "User not found");

            if (ModelState.IsValid)
            {
                try
                {
                    _auth0Helper.ChangePassword(userId, model.Password);

                    SnuffoSettings.ShowMessage(TempData, "Reset Password", "Your password has been changed", AlertMessageType.Success);
                    if (model.RedirectToLogin)
                    {
                        return Redirect($"/{CurrentUser.LanguageCode}/account/login-register/");
                    }
                }
                catch (Exception e)
                {
                    HandleAuth0Exception(e);
                }
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AccountProfile(AccountProfileModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _auth0Helper.UpdateAuth0User(model);

                    SnuffoSettings.ShowMessage(TempData, "Profile Updated", "Your profile has been updated", AlertMessageType.Success);
                }
                catch (Exception e)
                {
                    HandleAuth0Exception(e);
                }
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmailAddress(AccountChangeEmailModel model)
        {
            var exists = _auth0Helper.SearchAuth0UserByEmail(model.EmailAddress)?.Any(x => x.UserId.StartsWith("auth0") && x.UserId != CurrentUser.UserId);
            if (exists.GetValueOrDefault()) ModelState.AddModelError("", "E-mail address already exists");

            if (ModelState.IsValid)
            {
                try
                {
                    _auth0Helper.ChangeEmail(CurrentUser.UserId, model.EmailAddress);
                    this.AuthenticationManager.SignOut();

                    SnuffoSettings.ShowMessage(TempData, "E-mail Address Updated", "Your E-mail address has been updated. Please login again.", AlertMessageType.Success);
                    return Redirect($"/{CurrentUser.LanguageCode}/account/login-register/");
                }
                catch (Exception e)
                {
                    HandleAuth0Exception(e);
                }
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAccountAddress(AccountAddressModel model)
        {
            model.ContactAddress.AddressTypeId = AddressType.Default.Id;
            if (model.ShippingAddress != null) model.ShippingAddress.AddressTypeId = AddressType.ShipAddress.Id;

            base.SuppressValidation(model.GetPropertyName(x => x.UserId));
            base.SuppressValidation(model.GetPropertyName(x => x.EmailAddress));
            base.SuppressValidation(model.GetPropertyName(x => x.FirstName));
            base.SuppressValidation(model.GetPropertyName(x => x.LastName));
            base.SuppressValidation(model.GetPropertyName(x => x.Phone));
            
            if (model.HasSameAddress)
            {   
                base.SuppressValidation(model.GetPropertyName(x => x.ShippingAddress));
                model.ShippingAddress = null;
                UvendiaContext.Addresses.DeleteByUseryIdAndAddressType(CurrentUser.UserId, AddressType.ShipAddress);
            }

            if (ModelState.IsValid)
            {
                model.ContactAddress.CreatedBy = CurrentUser.UserId;
                var addresses = new List<Address>() { model.ContactAddress };
                if (model.ShippingAddress != null)
                {
                    model.ShippingAddress.Phone = model.ContactAddress.Phone;
                    model.ShippingAddress.CreatedBy = CurrentUser.UserId;
                    addresses.Add(model.ShippingAddress);
                }

                UvendiaContext.Addresses.Save(addresses);
                SnuffoSettings.ShowMessage(TempData, "Updated", "Your address updated successfuly.", AlertMessageType.Success);
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAccount(ContentModel model)
        {   
            if (ModelState.IsValid)
            {
                try
                {
                    _auth0Helper.DeleteAuth0User(CurrentUser.UserId);
                    this.AuthenticationManager.SignOut();

                    SnuffoSettings.ShowMessage(TempData, "Account Deleted", "Your account has been deleted.", AlertMessageType.Info);
                    return Redirect($"/{CurrentUser.LanguageCode}/");
                }
                catch (Exception e)
                {
                    HandleAuth0Exception(e);
                }
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadPDF(ContentModel model, long invoice = 0, long tickets = 0)
        {
            var order = UvendiaContext.Orders.Single((invoice > 0 ? invoice : tickets));

            if (invoice > 0)
            {
                var rm = ReportCreator.GenerateOrderInvoicePDF(order, Thread.CurrentThread.CurrentCulture.Name);
                var stream = ReportCreator.GenerateReportAsStream(rm, out string mimeType);
                return File(stream, mimeType, rm.FileName);
            }
            else if (tickets > 0)
            {
                var reportModel = ReportCreator.GenerateTicketPDF(order, Thread.CurrentThread.CurrentCulture.Name);
                var stream = ReportCreator.GenerateReportAsStream(reportModel, out string mimeType);
                return File(stream, mimeType, reportModel.FileName);
            }
            else
            {
                ShowAlertMessage("Tickets not available", "Sorry, our system couldn't find any tickets to download", AlertMessageType.Error);
                return Redirect($"/{CurrentUser.LanguageCode}/");
            }            
        }

        private ActionResult RedirectToLocal(string returnUrl, ContentModel model)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect($"/{CurrentUser.LanguageCode}/account/my-profile/");
            }
        }

        private void HandleAuth0Exception(Exception e)
        {
            if (e.InnerException is ApiException)
            {
                var apiEx = ((ApiException)e.InnerException);
                ModelState.AddModelError("", apiEx.ApiError.Message ?? apiEx.ApiError.Error);
            }
            else if (e.InnerException != null)
                ModelState.AddModelError("", e.InnerException.Message);
            else
                ModelState.AddModelError("", e.Message);
        }
    }
}