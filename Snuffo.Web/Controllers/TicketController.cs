using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Enums;
using Uvendia.Domain.Factories;
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
    public class TicketController : BaseSurfaceController
    {
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult BuyTickets(EventDetailModel model, string imageId)
        {            
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            var tickets = model.Event.TicketsSale.ToList();
            model.Event.LazyLoadProperties();

            for (int i = 0; i < tickets.Count; i++)
            {
                string dropdownName = string.Concat("TicketType", i);

                int ticketsQuantityToBuy = !Request.Form[dropdownName].IsNullOrEmpty() ? int.Parse(Request.Form[dropdownName]) : 0;
                ViewData[dropdownName] = ticketsQuantityToBuy;
                
                if (ticketsQuantityToBuy > 0)
                {
                    currentCart.AddTicket(                        
                        tickets[i].Id,
                        model.Event.DisplayName(),
                        ticketsQuantityToBuy,                        
                        metaData: imageId);
                }
            }

            return Redirect($"/{CurrentUser.LanguageCode}/cart/");
        }
    }
}