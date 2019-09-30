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
    public class ResetPasswordModel : ContentModel
    {
        public ResetPasswordModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent) { }

        public ResetPasswordModel(IPublishedContent content) : base(content) { }

        [Required]
        [StringLength(50, MinimumLength = 8)]
        [RegularExpression(DataUtils.PASSWORD_STRENGTH_REGEX, ErrorMessage = "Password is too weak")]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords are not identical")]
        [StringLength(50, MinimumLength = 8)]
        public string ConfirmPassword { get; set; }

        public bool RedirectToLogin { get; set; } = true;
    }
}