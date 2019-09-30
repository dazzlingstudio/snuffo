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
    public class RegisterModel : ContentModel
    {
        public RegisterModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }
        public RegisterModel(IPublishedContent content) : base(content)
        {

        }

        [Required]
        [StringLength(500)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(500)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(500)]
        public string EmailAddress { get; set; }
        [Required]
        [StringLength(500)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 8)]
        [RegularExpression(DataUtils.PASSWORD_STRENGTH_REGEX, ErrorMessage = "Password is too weak")]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords are not identical")]
        [StringLength(50, MinimumLength = 8)]
        public string ConfirmPassword { get; set; }
    }
}