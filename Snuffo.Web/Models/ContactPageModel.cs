using Dazzling.Studio.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ContactPageModel : ContentModel
    {
        public ContactPageModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }
        public ContactPageModel(IPublishedContent content) : base(content) { }

        [Required(ErrorMessage = "Your name is required")]
        [DisplayName("Your name")]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Your e-mail is required")]
        [StringLength(255)]
        [EmailAddress(ErrorMessage = "E-mail is invalid")]
        [DisplayName("Your e-mail")]
        public string EmailAddress { get; set; }

        public string EmailAddressTo { get; set; }

        [Required(ErrorMessage = "Your message is required")]
        public string Message { get; set; }
    }
}