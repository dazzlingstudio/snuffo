using Snuffo.Web.Models;
using Uvendia.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Uvendia.Domain.Cart;
using Dazzling.Studio.Utils;

namespace Snuffo.Web.Controllers
{
    public class LanguageCurrencySelectController : BaseSurfaceController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LanguageCurrencySelect(LanguageCurrencySelectModel model)
        {
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            if (!currentCart.HasCartItems())
            {
                SnuffoSettings.SetCurrency(model.SelectedCurrency);
            }
            else
                base.ShowAlertMessage("Multiple Currencies Detected", "It is not allowed to add multiple type of currencies to your shopping cart", AlertMessageType.Error);


            return CurrentUmbracoPage();
        }
    }
}