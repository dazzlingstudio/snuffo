using Snuffo.Web.Models;
using Uvendia.Domain;
using Mvc.Mailer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Dazzling.Studio.Utils;
using System.Net.Mail;

namespace Snuffo.Web.Controllers
{
    public class MailController : MailerBase
    {
        const string BASE_URL = "https://www.missblackhairnederland.nl";
                
        string _systemEmail;
        HttpRequestBase _request;
        SnuffoSettings _settings;
        readonly string _language = "en";

        private MailController(HttpRequestBase request, IPublishedContent content, SnuffoSettings settings = null, string language = "en")
        {
            _settings = settings ?? SnuffoSettings.Create(content);            
            _systemEmail = _settings.StoreEmail;
            _request = request;
            _language = language;

            if (_request != null)
            {
                var data = new RouteData();
                data.DataTokens["mail"] = "Mailing";
                this.ControllerContext = new ControllerContext(_request.RequestContext.HttpContext, data, this);
            }
        }

        public static MailController Instance(HttpRequestBase request, IPublishedContent content, string language)
        {
            return new MailController(request, content, language:language);
        }

        public static MailController Instance(HttpRequestBase request, SnuffoSettings settings, string language)
        {
            return new MailController(request, null, settings, language);
        }

        public static MailController Instance(SnuffoSettings settings, string language)
        {
            return new MailController(null, null, settings, language);
        }

        public virtual MvcMailMessage RegistrationConfirmMail(AccountProfileModel customer)
        {
            ViewData.Model = customer;
            string subject = _language.ToUpper() == "EN" ? "Confirm your registration" : "Bevestig je aanmelding";
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = $"~/Views/Mailing/RegistrationConfirmMail.{_language.ToUpper()}.cshtml";
                x.To.Add(customer.EmailAddress);
            });
        }

        public virtual MvcMailMessage PasswordRecovery(Auth0.ManagementApi.Models.User user)
        {
            ViewData.Model = user;
            string subject = _language.ToUpper() == "EN" ? "Password recovery" : "Wachtwoord herstel";
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = $"~/Views/Mailing/PasswordRecoveryMail.{_language.ToUpper()}.cshtml";
                x.To.Add(user.Email);
            });
        }

        public virtual MvcMailMessage MailError(HandleErrorInfo errorInfo)
        {
            if (!_request.IsLocal)
            {
                ViewData.Model = errorInfo;
                return Populate(x =>
                {
                    x.Subject = string.Format("{0} | Error occurred", SnuffoSettings.STORE_NAME);
                    x.ViewName = "~/Views/Mailing/ErrorInfo.cshtml";
                    x.To.Add(_systemEmail);
                });
            }
            else
            {
                return new MvcMailMessage();
            }
        }

        public MvcMailMessage ConfirmOrderMail(Order order)
        {
            string subject = string.Format("{0} | {1}", SnuffoSettings.STORE_NAME, order.OrderNumber);

            //if (!_request.IsLocal)
            //{
            //    EmailUtils.SendEmail(new EMail()
            //    {
            //        Subject = string.Concat("Bestelling geplaatst met nummer ", order.ID.ConvertToOrderNr()),
            //        Body = "Bestelling geplaatst op missblackhairnederland.nl",
            //        IsHtml = true,
            //        ToAddress = _systemEmail
            //    });
            //}

            ViewData.Model = order;
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = $"~/Views/Mailing/ConfirmOrderMail.{_language.ToUpper()}.cshtml";
                x.To.Add(order.GetCustomerEmailAddress());
            });
        }

        public virtual MvcMailMessage SendTicketsPDF(Order order, Attachment attach = null)
        {
            string subject = string.Format("{0} Ticket | {1}", SnuffoSettings.STORE_NAME, order.OrderNumber);

            //if (!_request.IsLocal)
            //{
            //    string body = $"{order.TicketOrderDetails.Count} ticket(s) besteld op <a href='{BASE_URL}'>missblackhairnederland.nl</a>";
            //    EmailUtils.SendEmail(new EMail()
            //    {
            //        Body = body,
            //        Subject = string.Concat("Bestelling geplaatst met nummer: ", order.OrderNumber),
            //        IsHtml = true,
            //        ToAddress = _systemEmail,
            //        //Attachments = new Attachment[] { attach }
            //    });
            //}

            ViewData.Model = order;
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = $"~/Views/Mailing/TicketMail.{_language.ToUpper()}.cshtml";
                if (attach != null)
                {
                    x.Attachments.Add(attach);
                }
                x.To.Add(order.GetCustomerEmailAddress());
            });
        }
    }
}