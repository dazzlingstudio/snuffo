using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web.WebApi;
using System.Net.Http;
using System.Net;
using Umbraco.Web;
using Uvendia.Domain.Repositories;
using Uvendia.Domain;
using System.IO;
using System.Web.Routing;
using Umbraco.Web.Mvc;
using System.Web.Mvc;
using Dazzling.Studio.Utils;
using Snuffo.Web.Models;
using reCaptcha;
using Uvendia.Domain.Cart;
using Snuffo.Web;
using Umbraco.Web.Models;
using System.Globalization;
using System.Threading;
using Uvendia.Domain.Enums;

namespace Snuffo.Web.Controllers
{
    public class ServiceController : SurfaceController
    {
        public ServiceController()
        {

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactPageModel model)
        {
            if (!ReCaptcha.Validate(DataUtils.GetConfig("reCaptcha:SecretKey")))
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
                    ToAddress = model.EmailAddressTo,
                    Subject = "New message",
                    IsHtml = true
                };

                EmailUtils.SendEmail(mail);

                return Redirect($"/{CurrentUser.LanguageCode}/contact/confirm-contact/");
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Subscribe(SubscribeModel model)
        {
            if (!model.HoneyPot.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "");
            }
            if (!DateTime.TryParseExact(model.Timestamp, "ffffHHMMtssddyyyymm", null, DateTimeStyles.None, out DateTime commentDate) 
                && commentDate.Day == DateTime.UtcNow.Day
                && commentDate.Month == DateTime.UtcNow.Month
                && commentDate.Year == DateTime.UtcNow.Year
                && commentDate.Hour == DateTime.UtcNow.Hour)
            {
                ModelState.AddModelError("", "");
            }

            if (ModelState.IsValid)
            {
                // Persist to database
                var sub = UvendiaContext.Subscriptions.Single($"{nameof(Subscription.Email)}=@Email", new { model.Email });
                if (sub == null)
                {
                    UvendiaContext.Subscriptions.Insert<long>(
                        new Subscription
                        {
                            Email = model.Email,
                            IsActive = true,
                            ModifiedBy = CurrentUser.UserId,
                            CreatedBy = CurrentUser.UserId
                        });
                }
                else
                {
                    sub.IsActive = true;
                    sub.ModifiedOn = DateTime.Now;
                    sub.ModifiedBy = CurrentUser.UserId;
                    UvendiaContext.Subscriptions.Save(sub);
                }
                SnuffoSettings.ShowMessage(TempData, "Subscribed", "Your e-mail has been subscribed successfully");
            }

            return CurrentUmbracoPage();
        }

        [HttpGet]
        public JsonResult GetTotalOrders()
        {
            long totalOrders = 0;

            if (CurrentUser.IsAuthenticated)
            {
                totalOrders = UvendiaContext.Orders.Count("CreatedBy = @createdBy", new { createdBy = CurrentUser.UserId });
            }

            return Json(new { TotalOrders = totalOrders }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NotChildAction]
        public ActionResult GetTopSellers()
        {
            var prods = UvendiaContext.Products.GetTopInterestingProducts(3);

            return PartialView("~/views/partials/Product/_ProductEntry.cshtml", prods);
        }

        [HttpGet]
        [NotChildAction]
        public ActionResult GetNewProducts()
        {
            var prods = UvendiaContext.Products.GetNewestProducts();

            return PartialView("~/views/partials/Product/_ProductEntry.cshtml", prods);
        }

        [HttpGet]
        [NotChildAction]
        public ActionResult GetTopRatedProducts()
        {
            IEnumerable<Product> prods = UvendiaContext.Products.GetTopRatedProducts();
            if (prods.IsNullOrEmpty())
            {
                prods = UvendiaContext.Products.GetRandomProducts();
            }

            return PartialView("~/views/partials/Product/_ProductEntry.cshtml", prods);
        }

        [HttpGet]
        [NotChildAction]
        public ActionResult GetCart()
        {
            var model = new CartShopModel();

            return PartialView("~/views/partials/_CartShop.cshtml", model);
        }

        [HttpGet]
        [NotChildAction]
        public ActionResult GetProductPrice(long p, string size)
        {
            var product = UvendiaContext.Products.Single(p);
            var variant = product.Variants.FirstOrDefault(x => string.Equals((x["Size"] as string), size, StringComparison.InvariantCultureIgnoreCase));

            return PartialView("~/views/partials/Product/_ProductDetailPrice.cshtml", variant);
        }

        [HttpPost]
        [NotChildAction]
        public ActionResult AddToCart(Dictionary<string, string> item)
        {
            if (!item["culture"].IsNullOrEmpty())
                Thread.CurrentThread.CurrentCulture = new CultureInfo(item["culture"]);

            
            Product product_to_add = UvendiaContext.Products.Single<long>(long.Parse(item["p"]));
            string size = item["size"];
            if (product_to_add.HasVariant)
            {
                if (size.IsNullOrEmpty())
                    product_to_add = product_to_add.Variants.First();
                else
                    product_to_add = product_to_add.Variants.First(x => string.Equals(x["Size"] as string, size, StringComparison.InvariantCultureIgnoreCase));
            }

            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            currentCart.AddProduct(product_to_add, int.Parse(item["qnty"]), SnuffoSettings.GetCurrency(), 0, item["thumb"]);

            return PartialView("~/views/partials/_CartShop.cshtml", new CartShopModel());
        }

        [HttpPost]
        [NotChildAction]
        public ActionResult ClearCart()
        {
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            currentCart.ClearCart();

            return PartialView("~/views/partials/_CartShop.cshtml", new CartShopModel());
        }

        [HttpPost]
        [NotChildAction]
        public ActionResult RemoveFromCart(Guid ci)
        {
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            currentCart.DeleteCartItem(ci);
            currentCart.UpdateCartQuantityCount();

            return PartialView("~/views/partials/_CartShop.cshtml", new CartShopModel());
        }

        [HttpGet]
        [NotChildAction]
        public ActionResult GetOrderDetails(long o)
        {
            var order = UvendiaContext.Orders.Single(o);

            return PartialView("~/views/partials/account/_AccountOrderDetails.cshtml", order);
        }

        [HttpPost]
        [NotChildAction]
        public ActionResult GetProductStockIndication(IEnumerable<long> productIds)
        {
            var indications = UvendiaContext.Products.GetTotalSoldItems(productIds.ToArray(), OrderStatus.Started, OrderStatus.Sent, OrderStatus.InProgress, OrderStatus.Closed);
            return Json(indications, JsonRequestBehavior.AllowGet);
        }
    }
}