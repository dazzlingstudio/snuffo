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
    public class AccountProfileModel : ContentModel
    {
        public AccountProfileModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public AccountProfileModel(IPublishedContent content) : base(content) { }

        [Required]
        public string UserId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FullName
        {
            get
            {
                if (!FirstName.IsNullOrEmpty() || !LastName.IsNullOrEmpty())
                    return $"{FirstName} {LastName}".Trim();
                return string.Empty;
            }
        }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public string Picture { get; set; }
        public string Phone { get; set; }
        public bool SubscribedToNewsletter { get; set; }
    }
}