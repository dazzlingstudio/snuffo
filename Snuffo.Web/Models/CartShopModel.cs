using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Snuffo.Web.Models
{
    public class CartShopModel
    {
        public CartShopModel()
        {
            CurrentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            if (CurrentCart.HasCartItems())
            {
                var items = CurrentCart.GetCartItemsByCookieId();
                if (!items.IsNullOrEmpty())
                {                    
                    CartItems = items.OrderBy(c => c.Name);
                    var productIds = items.Select(x => x.ProductId);
                    Subtotal = items.Sum(x => (x.Quantity * x.UnitPrice));
                }

                var checkoutUrl = $"/{CurrentUser.LanguageCode}/cart/checkout-address/";
                CheckoutUrl = (!CurrentUser.IsAuthenticated)
                    ? $"/{CurrentUser.LanguageCode}/cart/checkout-login/?returnUrl={checkoutUrl}"
                    : checkoutUrl;
            }
        }

        public CurrentCart CurrentCart { get; set; }

        public IEnumerable<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal Subtotal { get; set; }
        public string CheckoutUrl { get; set; }
    }
}