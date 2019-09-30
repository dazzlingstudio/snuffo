using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Repositories;
using Microsoft.Owin.Security.Cookies;
using Snuffo.Web;
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
    public class CartPageController : RenderMvcController
    {
        public ActionResult CartPage(ContentModel model)
        {
            var cartModel = new CartPageModel();

            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            cartModel.CartItems = currentCart.GetCartItemsByCookieId().OrderBy(c => c.Name).ToList();
            if (!cartModel.CartItems.IsNullOrEmpty())
            {
                var productIds = cartModel.CartItems.Where(c => c.ProductId.HasValue).Select(c => c.ProductId);
                if (productIds.Any())
                {
                    cartModel.Products = UvendiaContext.Products.All(productIds.ToArray()).ToList();
                }
                var ticketSaleIds = cartModel.CartItems.Where(c => c.TicketSaleId.HasValue).Select(c => c.TicketSaleId);
                if (ticketSaleIds.Any())
                {
                    cartModel.TicketsSale = UvendiaContext.TicketSales.All(ticketSaleIds.ToArray()).ToList();
                }
                cartModel.Subtotal = currentCart.CalculateSubtotal(cartModel.CartItems);
            }

            var checkoutUrl = $"/{CurrentUser.LanguageCode}/cart/checkout-address/";
            cartModel.CheckoutUrl = (!CurrentUser.IsAuthenticated)
                    ? $"/{CurrentUser.LanguageCode}/cart/checkout-login/?returnUrl={checkoutUrl}"
                    : checkoutUrl;

            return CurrentTemplate(cartModel);
        }
    }
}