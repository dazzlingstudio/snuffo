using Uvendia.Domain;
using Uvendia.Domain.Repositories;
using Uvendia.Domain.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class AccountAddressModel : AccountProfileModel
    {
        private IList<SelectListItem> _countries;

        public AccountAddressModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }

        public AccountAddressModel(IPublishedContent content) : base(content) { }
        
        public Address ContactAddress { get; set; }
        public Address ShippingAddress { get; set; }

        public bool HasSameAddress { get; set; } = true;

        public IList<SelectListItem> Countries
        {
            get
            {
                if (_countries == null)
                {
                    _countries = new List<SelectListItem>();
                    var allCountries = ISO3166.Country.List;
                    //var countries = UvendiaContext.WebSnuffoSettings.GetSettings<AvailableInCountry>()?.Countries ?? new List<Uvendia.Domain.Entities.Country>();
                    allCountries.ForEach(x => _countries.Add(new SelectListItem() { Text = x.Name, Value = x.TwoLetterCode }));
                }
                return _countries;
            }
        }
    }
}