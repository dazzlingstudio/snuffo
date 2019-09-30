using Dazzling.Studio.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace Snuffo.Web.Controllers
{
    public class BaseSurfaceController : SurfaceController
    {
        protected void SuppressValidation(string criteriaToNotMatch)
        {
            var keys = ModelState.Keys.Select(k => k).Where(k => k.Contains(criteriaToNotMatch)); // == false);
            keys.ForEach(key => ModelState[key].Errors.Clear());
        }

        /// <summary>
		/// Sets the alert message to be show for the user.
		/// </summary>
		/// <param name="message">Message to be shown.</param>
		public void ShowAlertMessage(string title, string msg, AlertMessageType messageType = AlertMessageType.Info)
        {
            SnuffoSettings.ShowMessage(TempData, title, msg, messageType);
        }
    }
}