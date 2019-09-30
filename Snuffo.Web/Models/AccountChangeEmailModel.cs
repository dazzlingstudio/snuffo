using Dazzling.Studio.Utils;
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
    public class AccountChangeEmailModel : ContentModel
    {
        public AccountChangeEmailModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public AccountChangeEmailModel(IPublishedContent content) : base(content) { }

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [Compare("EmailAddress", ErrorMessage = "EmailAddresses are not identical")]
        [StringLength(150)]
        [EmailAddress]
        public string ConfirmEmailAddress { get; set; }
    }
}