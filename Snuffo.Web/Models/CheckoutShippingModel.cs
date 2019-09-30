using Dazzling.Studio.Utils;
using Uvendia.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class CheckoutShippingModel : ContentModel
    {
        public CheckoutShippingModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public CheckoutShippingModel(IPublishedContent content) : base(content) { }

        [Required(ErrorMessage = "Please select a shipping method")]
        [Range(1, 9999, ErrorMessage = "Please select a shipping method")]
        public long? SelectedShippingMethodId { get; set; }

        public List<ShippingMethod> ShippingMethods { get; set; }
    }
}