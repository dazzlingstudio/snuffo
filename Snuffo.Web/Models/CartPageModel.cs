using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class CartPageModel : ContentModel
    {
        public CartPageModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {
        }

        public CartPageModel(IPublishedContent content) : base(content)
        {
        }

        public List<CartItem> CartItems { get; set; }

        public List<Product> Products { get; set; }

        public List<TicketSale> TicketsSale { get; set; }

        public decimal Subtotal { get; set; }

        public string CheckoutUrl { get; set; }
    }
}