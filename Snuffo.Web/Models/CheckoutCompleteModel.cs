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
    public class CheckoutCompleteModel : ContentModel
    {
        public CheckoutCompleteModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public CheckoutCompleteModel(IPublishedContent content) : base(content) { }

        public Order Order { get; internal set; }
    }
}