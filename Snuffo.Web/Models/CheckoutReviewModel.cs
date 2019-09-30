using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class CheckoutReviewModel : ContentModel
    {
        public CheckoutReviewModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public CheckoutReviewModel(IPublishedContent content) : base(content) { }

        public CurrentCart CurrentCart { get; internal set; }

        public IEnumerable<CartItem> CartItems { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<TicketSale> TicketsSale { get; set; }

        public Order Order { get; internal set; }
    }
}