using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Enums;
using Uvendia.Domain.Repositories;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using reCaptcha;
using Snuffo.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Snuffo.Web.Controllers
{
    public class ProductDetailPageController : RenderMvcController
    {
        public ActionResult ProductDetailPage(ContentModel model, long p)
        {
            if (p < 1)
                throw new ArgumentNullException(nameof(p), "ProductId is required");

            var ppModel = new ProductPageModel(model.Content);

            var fetched = UvendiaContext.Products.Single(p);
            ppModel.SelectedVariant = fetched;

            if (fetched.IsVariant())
            {
                ppModel.Product = fetched.Parent.Get();
                ppModel.SelectedSize = fetched["Size"] as string;

            }
            else
            {
                ppModel.Product = fetched;
            }

            ppModel.Images = ppModel.Product.Properties.Where(x => x.Property.DataType == DataType.CloudinaryImage && x.Value != null).Select(x => x.GetValue<string>());

            var categories = ppModel.Product.Categories.Select(c => c.Id).ToArray();
            ppModel.InterestingProducts = UvendiaContext.Products.GetTopInterestingProducts(8, categories);

            if (ppModel.Product.HasVariant)
            {
                bool hasSize = ppModel.Product.Variants.Any(x => x["Size"] != null);
                if (hasSize)
                {
                    ppModel.Sizes = new List<SelectListItem>();
                    foreach (var variant in ppModel.Product.Variants)
                    {
                        var item = new SelectListItem() { Text = variant["Size"] as string, Value = variant["Size"] as string };
                        ppModel.Sizes.Add(item);

                        if (string.Equals(ppModel.SelectedSize, variant["Size"] as string))
                        {
                            ppModel.SelectedVariant = variant;
                        }
                    }
                    if (ppModel.SelectedSize == null)
                    {
                        ppModel.SelectedVariant = ppModel.Product.Variants[0];
                    }
                }

            }

            return CurrentTemplate(ppModel);
        }
    }
}