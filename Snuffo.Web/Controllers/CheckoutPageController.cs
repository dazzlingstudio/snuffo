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
using CS = Dazzling.Studio.CheckoutService.Enums;
using Dazzling.Studio.Utils;
using System.Collections.Generic;
using Dazzling.Studio.CheckoutService.Enums;
using Uvendia.Domain.Enums;
using Uvendia.Domain.Exceptions;

namespace Uvendia.Cms.Controllers
{
    public class CheckoutPageController : RenderMvcController
    {
        static readonly IMapper OrderMapper;
        static CheckoutPageController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Order, CheckoutOrder>()
                  .ForMember(dest => dest.Company, opt => opt.MapFrom(src => SnuffoSettings.STORE_NAME))
                  .ForMember(dest => dest.OrderReference, opt => opt.MapFrom(src => src.OrderNumber))
                  .ForMember(dest => dest.IssuerId, opt => opt.MapFrom(src => src.MetaData))
                  .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => string.Equals(src.PaymentMethod.Name, CS.PaymentMethod.iDEAL.ToString(), StringComparison.InvariantCultureIgnoreCase) ? CS.PaymentMethod.iDEAL : CS.PaymentMethod.PayPal))
                  .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                  .ForMember(dest => dest.CallBackUrlOnPaymentSucceed, opt => opt.Ignore())
                  .ForMember(dest => dest.CallBackUrlOnPaymentFailed, opt => opt.Ignore())
                  .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => SnuffoSettings.GetCurrency()))
                  .ForMember(dest => dest.ShippingFee, opt => opt.MapFrom(src => src.CalculateShippingFee()))
                  .ForMember(dest => dest.ServiceFee, opt => opt.MapFrom(src => src.CalculatePaymentFee()))
                  .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.CalculateSubTotalWithoutVAT()))
                  .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.CalculateTotal()))
                  .ForMember(dest => dest.Discount, opt => opt.Ignore())
                  .ForMember(dest => dest.VAT, opt => opt.MapFrom(src => src.CalculateVAT()));

                cfg.CreateMap<ProductOrderDetail, CheckoutOrderDetail>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Product.Sku))
                  .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                  .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.DisplayName(null)))
                  .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.CalculatePriceWithDiscount()));

                cfg.CreateMap<TicketOrderDetail, CheckoutOrderDetail>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TicketSale.Id))
                  .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                  .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TicketSale.Description))
                  .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.CalculatePriceWithDiscount()));
            });

            OrderMapper = config.CreateMapper();
            //Mapper.AssertConfigurationIsValid();
        }

        public ActionResult CheckoutPaymentPage(ContentModel model)
        {
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            var order = currentCart.GetOrder();

            if (order.HasOrderProductDetails() && !order.ShippingMethodId.HasValue)
            {
                return Redirect(string.Concat("/", CurrentUser.LanguageCode, "/cart/checkout-shipping"));
            }

            var cpm = new CheckoutPaymentModel(model.Content);

            cpm.PaymentMethods = UvendiaContext.PaymentMethods.All().Where(p => p.Enabled).ToList();

            cpm.SelectedPaymentMethodId = order.PaymentMethodId;
            cpm.iDealIssuerId = order.MetaData;

            try
            {
                var ideal = cpm.PaymentMethods.Single(x => x.Name.Equals("ideal", StringComparison.InvariantCultureIgnoreCase));
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;                

                Connector connector = new Connector();
                connector.MerchantId = ideal["MerchantID"];
                connector.SubId = ideal["SubID"];
                connector.ExpirationPeriod = ideal["ExpirationPeriod"];
                connector.MerchantReturnUrl = new Uri(string.Format(ideal["MerchantReturnURL"], CurrentUser.LanguageCode));

                ING.iDealAdvanced.Data.Issuers issuers = connector.GetIssuerList();

                foreach (var country in issuers.Countries)
                {
                    foreach (var issuer in country.Issuers)
                    {
                        cpm.iDealIssuerList.Add(new SelectListItem() { Text = issuer.Name, Value = issuer.Id.ToString() });
                    }
                }
            }
            catch (ING.iDealAdvanced.Data.IDealException iex)
            {
                // request consumerMessage	
                MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).MailError(new HandleErrorInfo(iex, "CheckoutPaymentPage", "LoadIssueList")).SendAsync();
            }
            catch (Exception ex)
            {
                MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).MailError(new HandleErrorInfo(ex, "CheckoutPaymentPage", "LoadIssueList")).SendAsync();
                //throw;
            }

            return CurrentTemplate(cpm);
        }

        public ActionResult CheckoutReviewPage(ContentModel model)
        {
            var crm = new CheckoutReviewModel(model.Content);
            crm.CurrentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            crm.Order = crm.CurrentCart.GetOrder();

            if (!crm.Order.PaymentMethodId.HasValue)
            {
                return Redirect(string.Concat("/", CurrentUser.LanguageCode, "/cart/checkout-payment"));
            }

            crm.CartItems = crm.CurrentCart.GetCartItemsByCookieId();

            var productIds = crm.CartItems.Where(c => c.ProductId.HasValue).Select(c => c.ProductId);
            if (productIds.Any())
            {
                crm.Products = UvendiaContext.Products.All(productIds.ToArray());
            }

            var ticketSaleIds = crm.CartItems.Where(c => c.TicketSaleId.HasValue).Select(c => c.TicketSaleId);
            if (ticketSaleIds.Any())
            {
                crm.TicketsSale = UvendiaContext.TicketSales.All(ticketSaleIds.ToArray());
            }

            return CurrentTemplate(crm);
        }

        public ActionResult CheckoutCompletedPage(ContentModel model)
        {
            var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
            if (currentCart.ItemsCount == 0) return Redirect($"/{CurrentUser.LanguageCode}/cart/");
            try
            {
                var order = currentCart.GetOrder();

                var checkoutOrder = OrderMapper.Map<CheckoutOrder>(order);
                string baseUrl = WebUtils.GetApplicationUrlPath(HttpContext.ApplicationInstance.Context);
                checkoutOrder.CallBackUrlOnPaymentSucceed = $"{baseUrl}{CurrentUser.LanguageCode}/checkout-completed";
                checkoutOrder.CallBackUrlOnPaymentFailed = $"{baseUrl}{CurrentUser.LanguageCode}/checkout-cancelled";

                checkoutOrder.OrderDetails.AddRange(OrderMapper.Map<List<CheckoutOrderDetail>>(order.ProductOrderDetails));
                checkoutOrder.OrderDetails.AddRange(OrderMapper.Map<List<CheckoutOrderDetail>>(order.TicketOrderDetails));

                var payment = PaymentBase.CreatePayment(checkoutOrder);
                if (payment.PaymentMethod == CS.PaymentMethod.PayPal)
                {
                    PayPalPayment paypal = payment as PayPalPayment;
                    paypal.IsSandbox = string.Equals(order.PaymentMethod["Mode"], "sandbox");
                    paypal.ClientId = order.PaymentMethod["ClientId"];
                    paypal.ClientSecret = order.PaymentMethod["ClientSecret"];
                }

                if (Request.QueryString.Count == 0)
                {
                    var response = payment.DoPayment();
                    order.PaymentTransactionId = response.PaymentTransactionId;

                    // Save the order
                    currentCart.SaveOrder(order);

                    return Redirect(response.PaymentServiceUrl);
                }

                order.PayerId = Request.QueryString["PayerID"];
                if (payment.RequestPaymentStatus(order.PaymentTransactionId, order.PayerId, out string message) != PaymentStatus.Success)
                {
                    order.PaymentError = message;
                    // Save order
                    order.OrderStatusId = OrderStatus.Cancelled.Id;
                    currentCart.SaveOrder(order);

                    return Redirect($"/{CurrentUser.LanguageCode}/checkout-cancelled/");
                }
                // Tickets are sent immediately
                order.OrderStatusId = (order.ProductOrderDetails.IsNullOrEmpty()) ? OrderStatus.Sent.Id : OrderStatus.Started.Id;

                order.Paid = true;
                order.PaidAmount = order.CalculateTotal();

                currentCart.CompleteOrder(order);

                if (order.ProductOrderDetails.Any())
                {
                    // Send order confirmation mail.
                    MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).ConfirmOrderMail(order).Send();
                }
                if (order.TicketOrderDetails.Any()) // tickets bought
                {
                    MailController.Instance(Request, model.Content, CurrentUser.LanguageCode).SendTicketsPDF(order).Send();
                }
            }
            catch (OrderAlreadyProcessedException)
            {
                // Clear cart.
                currentCart.ClearCart();
            }

            return CurrentTemplate(model);
        }
    }
}