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
    public class ShopPageController : RenderMvcController
    {
        public ActionResult ShopPage(ContentModel model, int page = 1, string sorting = "")
        {
            var settings = SnuffoSettings.Create(model.Content);
            var shopPageModel = new ShopPageModel(model.Content);

            shopPageModel.Categories = UvendiaContext.Categories.GetByStoreId(settings.StoreId);
            shopPageModel.PageIndex = page;

            long totalRows;
            long categoryId;
            long.TryParse(Request.QueryString["c"], out categoryId);
            long priceDefinitionId = UvendiaContext.PriceDefinitions.Single(SnuffoSettings.GetCurrency()).Id;
            var sortBy = sorting.IsNullOrEmpty() ? ProductSortyBy.Popularity : (ProductSortyBy)Enum.Parse(typeof(ProductSortyBy), sorting, true);
                        
            var products = UvendiaContext.Products.GetProductsPaged(shopPageModel.PageIndex, 
                shopPageModel.PageSize, 
                out totalRows, categoryId > 0 ? categoryId : (long?)null, 
                true, 
                priceDefinitionId, 
                sortyBy: sortBy);

            shopPageModel.TotalRows = totalRows;
            shopPageModel.Products = products;
            shopPageModel.SelectedSorting = sortBy.ToString();

            return CurrentTemplate(shopPageModel);
        }

        
    }
}