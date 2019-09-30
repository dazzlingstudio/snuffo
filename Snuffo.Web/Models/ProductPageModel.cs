using Uvendia.Domain;
using Uvendia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class ProductPageModel : ContentModel
    {                                                                                                                                                                                                           
        public ProductPageModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {
        }

        public ProductPageModel(IPublishedContent content) : base(content)
        {
            
        }
        
        public Product Product { get; set; }
        public Product SelectedVariant { get; set; }
        public IEnumerable<string> Images { get; set; }
        public string SelectedSize { get; set; }
        public IList<SelectListItem> Sizes { get; set; }
        public IEnumerable<Product> InterestingProducts { get; set; }

    }
}