using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Enums;
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
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Snuffo.Web.Controllers
{
    public class ShopController : BaseSurfaceController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult ShopSorting(ShopPageModel model, string c, string page)
        {
            var url = $"/{CurrentUser.LanguageCode}/shop/";
            if (!c.IsNullOrEmpty())
                url = $"{url}?c={c}";
            if (!page.IsNullOrEmpty())
                url = $"{url}{(url.Contains("?") ? "&" : "?")}page={page}";
            if (!model.SelectedSorting.IsNullOrEmpty())
                url = $"{url}{(url.Contains("?") ? "&" : "?")}sorting={model.SelectedSorting}";

            return Redirect(url.ToLower());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PostProductReview(ProductReviewModel model)
        {
            if (!ReCaptcha.Validate(DataUtils.GetConfig("reCaptcha:SecretKey")))
            {
                ModelState.AddModelError("", "Captcha cannot be empty");
            }

            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    Comment = model.Comment,
                    Email = model.Email,
                    ItemId = model.ProductId,
                    Name = model.Name,
                    Subject = model.Subject,
                    Stars = model.Stars
                };
                UvendiaContext.Reviews.Save(review);
            }

            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult UpdateCart(CartPageModel model)
        {
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            foreach (var item in model.CartItems)
            {
                currentCart.UpdateQuantity(item.Id, item.Quantity);
            }
            currentCart.UpdateCartQuantityCount();
            return Redirect($"/{CurrentUser.LanguageCode}/cart");
        }
    }
}