using Dazzling.Studio.Utils;
using Uvendia.Domain;
using Uvendia.Domain.Cart;
using Uvendia.Domain.Enums;
using Uvendia.Domain.Repositories;
using Microsoft.Owin.Security.Cookies;
using Snuffo.Web;
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
    public class CheckoutController : BaseSurfaceController
    {
        private readonly Auth0Helper _auth0Helper;

        public CheckoutController()
        {
            _auth0Helper = new Auth0Helper();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderCheckoutAddress(AccountAddressModel model)
        {
            model.ContactAddress.AddressTypeId = AddressType.Default.Id;
            model.ContactAddress.FirstName = model.FirstName;
            model.ContactAddress.LastName = model.LastName;
            model.ContactAddress.Email = model.EmailAddress;
            model.ContactAddress.Phone = model.Phone;

            base.SuppressValidation(model.GetPropertyName(x => x.ContactAddress.Phone));
            base.SuppressValidation(model.GetPropertyName(x => x.ContactAddress.AddressTypeId));            
            base.SuppressValidation(model.GetPropertyName(x => x.ShippingAddress.AddressTypeId));
            base.SuppressValidation(model.GetPropertyName(x => x.ShippingAddress.Phone));

            if (model.ShippingAddress != null) model.ShippingAddress.AddressTypeId = AddressType.ShipAddress.Id;

            if (model.HasSameAddress)
            {
                base.SuppressValidation(model.GetPropertyName(x => x.ShippingAddress));
                model.ShippingAddress = null;
            }

            if (!CurrentUser.IsAuthenticated)
            {
                base.SuppressValidation(model.GetPropertyName(x => x.UserId));
            }
            else
            {
                if (model.HasSameAddress)
                {
                    UvendiaContext.Addresses.DeleteByUseryIdAndAddressType(CurrentUser.UserId, AddressType.ShipAddress);
                }
            }

            if (ModelState.IsValid)
            {
                // Save user data
                if (CurrentUser.IsAuthenticated)
                {
                    _auth0Helper.UpdateAuth0User(model);
                }

                model.ContactAddress.CreatedBy = CurrentUser.UserId;
                var addresses = new List<Address>() { model.ContactAddress };
                if (model.ShippingAddress != null)
                {
                    model.ShippingAddress.CreatedBy = CurrentUser.UserId;
                    model.ShippingAddress.Phone = model.ContactAddress.Phone;
                    addresses.Add(model.ShippingAddress);
                }

                UvendiaContext.Addresses.Save(addresses);

                var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
                var order = currentCart.GetOrder();

                // Attach address to order
                var invoiceAddress = addresses.Single(a => a.AddressTypeId == AddressType.Default.Id);
                order.InvoiceAddressId = invoiceAddress.Id;
                order.ShipAddressId = (model.ShippingAddress == null) ? (long?)null : addresses.Single(a => a.AddressTypeId != AddressType.Default.Id).Id;
                                
                // Save the order
                currentCart.SaveOrder(order);

                if (order.HasOrderProductDetails())
                {
                    return Redirect($"/{CurrentUser.LanguageCode}/cart/checkout-shipping");
                }
                else
                {
                    return Redirect($"/{CurrentUser.LanguageCode}/cart/checkout-payment");
                }
            }

            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderCheckoutShipping(CheckoutShippingModel model)
        {
            if (ModelState.IsValid)
            {
                var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
                var order = currentCart.GetOrder();

                order.ShippingMethodId = model.SelectedShippingMethodId.Value;
                
                currentCart.SaveOrder(order);

                return Redirect($"/{CurrentUser.LanguageCode}/cart/checkout-payment");
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderCheckoutPayment(CheckoutPaymentModel model)
        {
            PaymentMethod paymentMethod = null;
            if (model.SelectedPaymentMethodId.HasValue)
            {
                paymentMethod = UvendiaContext.PaymentMethods.Single(model.SelectedPaymentMethodId.Value);
                if (model.SelectedPaymentMethodId.HasValue)
                {
                    if (string.Equals(paymentMethod.Name, "ideal", StringComparison.InvariantCultureIgnoreCase)
                        && model.iDealIssuerId.IsNullOrEmpty())
                    {
                        ModelState.AddModelError("", "Please select your bank");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var currentCart = CurrentCart.Create(SnuffoSettings.STORE_NAME);
                var order = currentCart.GetOrder();

                order.PaymentMethod = paymentMethod;
                order.PaymentMethodId = model.SelectedPaymentMethodId.Value;
                order.MetaData = model.iDealIssuerId;

                if (currentCart.SaveOrder(order, true, out List<string> errors))
                {
                    return Redirect($"/{CurrentUser.LanguageCode}/cart/checkout-review");
                }
                else
                {
                    errors.ForEach(err => ModelState.AddModelError("", err));
                }
            }
            return CurrentUmbracoPage();
        }
    }
}