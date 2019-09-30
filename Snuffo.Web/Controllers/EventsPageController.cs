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

namespace Snuffo.Web.Controllers
{
    public class EventsPageController : RenderMvcController
    {
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