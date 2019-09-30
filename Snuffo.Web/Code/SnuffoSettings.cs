using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Snuffo.Web
{
    public class SnuffoSettings
    {
        public string ApplicationName { get; set; }
        public int StoreId { get; set; }
        public string StoreEmail { get; set; }
        public string SupportEmail { get; set; }
        public string Phone { get; set; }
        public string OpeningTime { get; set; }
        public string MetaFacebookImageURL { get; set; }
        public string KvK { get; set; }

        public string BankAccount { get; set; }
        public string BTWNr { get; set; }
        public string InstagramUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }

        public const string STORE_NAME = "Snuffo";

        private static string SELECTED_CURRENCY_COOKIE_NAME => $".{STORE_NAME.ToLower()}_currency";

        //private static CustomSettings _settings = null;

        public static SnuffoSettings Create(IPublishedContent content)
        {
            var homepage = content.Root();
            var settings = new SnuffoSettings()
            {
                StoreId = homepage.Value<int>("storeId"),
                ApplicationName = homepage.Value<string>("applicationName"),
                StoreEmail = homepage.Value<string>("storeEmail"),
                SupportEmail = homepage.Value<string>("supportEmail"),
                Phone = homepage.Value<string>("phone"),
                MetaFacebookImageURL = homepage.Value<string>("metaFacebookImageURL"),
                KvK = homepage.Value<string>("kvk"),
                BankAccount = homepage.Value<string>("bankAccount"),
                BTWNr = homepage.Value<string>("bTWNr"),
                OpeningTime = homepage.Value<string>("openTime"),
                InstagramUrl = homepage.Value<string>("instagramUrl"),
                FacebookUrl = homepage.Value<string>("facebookUrl"),
                TwitterUrl = homepage.Value<string>("twitterUrl")                
            };
            return settings;
        }
                
        public static string ConvertMoney(decimal value, string currency = null)
        {
            string retVal = string.Empty;
            CultureInfo culture;
            if (currency.IsNullOrEmpty()) currency = GetCurrency();

            switch (currency)
            {
                case "EUR":
                    culture = CultureInfo.CreateSpecificCulture("nl-NL");
                    retVal = value.ConvertToMoney(culture);
                    break;
                default:                
                    culture = CultureInfo.CreateSpecificCulture("en-US");
                    retVal = value.ConvertToMoney(culture);                
                    break;
            }
            return retVal;
        }

        public static void ShowMessage(TempDataDictionary tempData, string title, string msg, AlertMessageType messageType = AlertMessageType.Info)
        {
            tempData["WEB_ALERT_MESSASGE"] = msg;
            tempData["ALERT_MESSAGE_TYPE"] = messageType.ToString().ToLower();
            tempData["WEB_ALERT_TITLE"] = title;
        }

        public static string GetCurrency()
        {
            var cookie = HttpContext.Current.Request.Cookies[SELECTED_CURRENCY_COOKIE_NAME];
            if (cookie == null)
            {
                var currentCart = CurrentCart.Create(STORE_NAME);
                if (currentCart.ItemsCount > 0)
                {
                    var order = currentCart.GetOrder();
                    var currency = order.GetOrderCurrency();
                    if (!currency.IsNullOrEmpty())
                    {
                        SetCurrency(currency);
                        return currency;
                    }
                }
            }
            return cookie != null ? cookie.Value : "EUR";
        }

        public static void SetCurrency(string selectedCurrency)
        {            
            var cookie = HttpContext.Current.Request.Cookies[SELECTED_CURRENCY_COOKIE_NAME];
            if (cookie == null || cookie.Value.IsNullOrEmpty())
            {
                cookie = new HttpCookie(SELECTED_CURRENCY_COOKIE_NAME, selectedCurrency);
                cookie.Expires = DateTime.Now.AddDays(CurrentCart.VALID_TOTAL_DAYS);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                cookie.Value = selectedCurrency;
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
    }
}