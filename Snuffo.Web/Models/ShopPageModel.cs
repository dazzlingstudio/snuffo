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
    public class ShopPageModel : ContentModel
    {
        public ShopPageModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {
        }

        public ShopPageModel(IPublishedContent content) : base(content)
        {
            TotalRows = 0;
            PageIndex = 1;
            PageSize = 21;
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }

        public int TotalPages { get { return (int)Math.Ceiling(((double)TotalRows) / PageSize); } }

        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }

        public int ShowingCount
        {
            get
            {
                int retVal = PageIndex * PageSize;
                if (retVal > TotalRows)
                    retVal = TotalRows;

                return retVal;
            }
        }

        public string SelectedSorting { get; set; }

        IList<SelectListItem> _sorting = null;
        public IList<SelectListItem> Sorting
        {
            get
            {
                if (_sorting == null)
                {
                    _sorting = new List<SelectListItem>();
                    _sorting.Add(new SelectListItem() { Text = "Popularity", Value = ProductSortyBy.Popularity.ToString() });
                    _sorting.Add(new SelectListItem() { Text = "Low - High Price", Value = ProductSortyBy.Cheapest.ToString() });
                    _sorting.Add(new SelectListItem() { Text = "High - Low Price", Value = ProductSortyBy.MostExpensive.ToString() });
                    _sorting.Add(new SelectListItem() { Text = "Average Rating", Value = ProductSortyBy.BestReviewed.ToString() });
                    _sorting.Add(new SelectListItem() { Text = "A - Z Order", Value = ProductSortyBy.Title_AZ.ToString() });
                    _sorting.Add(new SelectListItem() { Text = "Z - A Order", Value = ProductSortyBy.Title_ZA.ToString() });
                }
                return _sorting;
            }
        }
    }
}