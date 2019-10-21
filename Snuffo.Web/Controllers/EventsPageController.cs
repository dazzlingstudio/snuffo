using AutoMapper;
using Dazzling.Studio.CheckoutService;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Repositories;
using ING.iDealAdvanced;
using Snuffo.Web;
using Snuffo.Web.Controllers;
using Snuffo.Web.Models;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Dazzling.Studio.Utils;
using System.Collections.Generic;
using Uvendia.Domain.Enums;
using Uvendia.Domain.Factories;
using System.Threading;
using Dazzling.Studio.Utils.Media;

namespace Snuffo.Web.Controllers
{
    public class EventsPageController : RenderMvcController
    {
        public ActionResult EventDetailPage(ContentModel model, long? e)
        {
            if (!long.TryParse(Request.QueryString["e"], out long id))
            {
                return Redirect(string.Concat("/", CurrentUser.LanguageCode, "/page-not-found"));
            }
            var evnt = UvendiaContext.Events.Single(id);
            if (evnt == null)
            {
                return Redirect(string.Concat("/", CurrentUser.LanguageCode, "/page-not-found"));
            }
            var imageUrl = CloudinaryService.GetCloudinaryUrl(evnt["Image 1"].ConvertTo<string>(), 886, 575, CropType.Fit, gravityType: GravityType.None, effect: Effect.Incognito);

            var edm = new EventDetailModel(model.Content);
            edm.Event = evnt;
            edm.ImageUrl = imageUrl;

            return CurrentTemplate(edm);
        }

        public ActionResult DownloadTicketsPage(ContentModel model, string id)
        {
            var order = UvendiaContext.Orders.Single($"{nameof(Order.ExternalId)}=@ExternalId", new { ExternalId = id });

            if (order != null && order.Paid && order.OrderStatus != OrderStatus.Cancelled)
            {                   
                var reportModel = ReportCreator.GenerateTicketPDF(order, Thread.CurrentThread.CurrentCulture.Name);
                var stream = ReportCreator.GenerateReportAsStream(reportModel, out string mimeType);
                return File(stream, mimeType, reportModel.FileName);
            }
            else
            {
                return Redirect($"/{CurrentUser.LanguageCode}/error-400/");
            }
        }
    }
}