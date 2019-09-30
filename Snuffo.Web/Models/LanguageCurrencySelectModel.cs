using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;

namespace Snuffo.Web.Models
{
    public class LanguageCurrencySelectModel : ContentModel
    {
        public LanguageCurrencySelectModel() : this(Current.UmbracoContext.PublishedRequest.PublishedContent)
        {

        }
        public LanguageCurrencySelectModel(IPublishedContent content) : base(content)
        {
            var currency = SnuffoSettings.GetCurrency();
            SelectedCurrency = currency;                    
        }

        public string SelectedCurrency { get; set; }
        public string SelectedLanguage { get; set; } = CurrentUser.LanguageCode;

        public string SelectedCurrencyText
        {
            get
            {
                var currency = Currencies?.FirstOrDefault(x => x.Value == SelectedCurrency);

                if (currency != null)
                {
                    return $"{currency.Text}";
                }
                return string.Empty;
            }
        }

        IList<SelectListItem> _currencies;
        public IList<SelectListItem> Currencies
        {
            get
            {
                if (_currencies == null)
                {
                    _currencies = new List<SelectListItem>();
                    var priceDefs = UvendiaContext.PriceDefinitions.All();
                    priceDefs.ForEach(pd => {
                        _currencies.Add(new SelectListItem() { Text = $"{pd.Currency}", Value = pd.Currency, Selected = pd.Currency == SelectedCurrency });
                    });
                }
                return _currencies;
            }
            set { _currencies = value; }
        }

        IList<SelectListItem> _languages;
        public IList<SelectListItem> Languages
        {
            get
            {
                if (_languages == null)
                {
                    _languages = new List<SelectListItem>();
                    var langs = UmbracoUtils.GetLanguages();
                    langs.ForEach(p => {                        
                        var text = GetLanguageName(p.CultureInfo.NativeName);
                        _languages.Add(new SelectListItem() { Text = text, Value = p.CultureInfo.TwoLetterISOLanguageName.ToUpper(), Selected = p.IsoCode == SelectedLanguage });
                    });
                }
                return _languages;
            }
            set { _languages = value; }
        }

        public string GetLanguageName(string nativeName = null)
        {
            nativeName = nativeName.IsNullOrEmpty() ? Thread.CurrentThread.CurrentUICulture.Parent.DisplayName : nativeName;
            return nativeName;
        }
    }
}