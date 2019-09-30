using Dazzling.Studio.Utils;
using Uvendia.Domain;
using reCaptcha;
using Snuffo.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Snuffo.Web.Controllers
{
    public class ContactPageController : BaseSurfaceController
    {
        [ChildActionOnly]
        public ActionResult ContactPage()
        {
            var contact = new ContactPageModel();

            return PartialView(new ContactPageModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult HandleContactUs(ContactPageModel model)
        {
            if (!ReCaptcha.Validate(DataUtils.GetConfig("ReCaptcha:SecretKey")))
            {
                ModelState.AddModelError("", "Captcha cannot be empty");
            }

            if (ModelState.IsValid)
            {
                EMail mail = new EMail()
                {
                    Name = model.Name,
                    SenderEmail = model.EmailAddress,
                    Body = model.Message,
                    ToAddress = SnuffoSettings.Create(model.Content).SupportEmail,
                    Subject = model.Subject,
                    IsHtml = true
                };

                EmailUtils.SendEmail(mail);

                return Redirect($"/{CurrentUser.LanguageCode}/contact/submitted");
            }

            return CurrentUmbracoPage();
        }
    }
}