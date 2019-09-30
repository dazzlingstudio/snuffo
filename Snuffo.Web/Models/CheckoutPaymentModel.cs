using Dazzling.Studio.Utils;
using Uvendia.Domain;
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
    public class CheckoutPaymentModel : ContentModel
    {
        public CheckoutPaymentModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public CheckoutPaymentModel(IPublishedContent content) : base(content) { }

        [Required(ErrorMessage = "Please select a payment method")]
        [Range(1, 9999, ErrorMessage = "Please select a payment method")]
        public long? SelectedPaymentMethodId { get; set; }

        public List<PaymentMethod> PaymentMethods { get; set; }
                
        public string iDealIssuerId { get; set; }

        public IList<SelectListItem> iDealIssuerList { get; set; } = new List<SelectListItem>();
    }
}