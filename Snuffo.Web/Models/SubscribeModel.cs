using Uvendia.Domain;
using Uvendia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class SubscribeModel : ContentModel
    {
        public SubscribeModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {
        }

        public SubscribeModel(IPublishedContent content) : base(content)
        {
            // Format the datetime in an inventive way
            this.Timestamp = DateTime.UtcNow.ToString("ffffHHMMtssddyyyymm");
        }

        [Required(ErrorMessage = "E-mail is required")]
        [EmailAddress(ErrorMessage = "E-mail is invalid")]
        public string Email { get; set; }
        public string HoneyPot { get; set; }
        public string Timestamp { get; set; }
    }
}