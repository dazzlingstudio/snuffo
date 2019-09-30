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
    public class PasswordRecoveryModel : ContentModel
    {
        public PasswordRecoveryModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }
        public PasswordRecoveryModel(IPublishedContent content) : base(content)
        {
        }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}