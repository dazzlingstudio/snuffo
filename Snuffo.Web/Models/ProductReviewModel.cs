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
    public class ProductReviewModel : ContentModel
    {
        public ProductReviewModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }
        public ProductReviewModel(IPublishedContent content) : base(content)
        {

        }

        [DisplayName("Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [DisplayName("E-mail")]
        [EmailAddress(ErrorMessage = "E-mail is invalid")]
        public string Email { get; set; }
        
        [DisplayName("Stars")]
        [Range(0.5F, 5, ErrorMessage = "Stars must be between 0.5 and 5")]
        public float Stars { get; set; }

        [Required]
        public long ProductId { get; set; }

        [DisplayName("Subject")]
        [StringLength(225, ErrorMessage = "Subject must be 255 characters long")]
        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [DisplayName("Comment")]
        [Required(ErrorMessage = "Comment is required")]
        public string Comment { get; set; }
    }
}