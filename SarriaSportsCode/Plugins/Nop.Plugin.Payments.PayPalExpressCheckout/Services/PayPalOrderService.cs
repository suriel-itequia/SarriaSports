using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.PayPalExpressCheckout.PayPalAPI;
using System.Linq;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Plugin.Payments.PayPalExpressCheckout.Helpers;

namespace Nop.Plugin.Payments.PayPalExpressCheckout.Services
{
    public class PayPalOrderService : IPayPalOrderService
    {
        private readonly IWorkContext _workContext;
        private readonly PayPalExpressCheckoutPaymentSettings _payPalExpressCheckoutPaymentSettings;
        private readonly IPayPalCurrencyCodeParser _payPalCurrencyCodeParser;
        private readonly IPayPalCartItemService _payPalCartItemService;
        private readonly IShippingService _shippingService;

        public PayPalOrderService(IWorkContext workContext,
                                  PayPalExpressCheckoutPaymentSettings payPalExpressCheckoutPaymentSettings,
                                  IPayPalCurrencyCodeParser payPalCurrencyCodeParser,
                                  IPayPalCartItemService payPalCartItemService,
                                  IShippingService shippingService)
        {
            _workContext = workContext;
            _payPalExpressCheckoutPaymentSettings = payPalExpressCheckoutPaymentSettings;
            _payPalCurrencyCodeParser = payPalCurrencyCodeParser;
            _payPalCartItemService = payPalCartItemService;
            _shippingService = shippingService;
        }

        public PaymentDetailsType[] GetPaymentDetails(IList<ShoppingCartItem> cart)
        {
            var currencyCode = _payPalCurrencyCodeParser.GetCurrencyCodeType(_workContext.WorkingCurrency);

            decimal orderTotalDiscountAmount;
            Discount appliedDiscount;
            int redeemedRewardPoints;
            decimal redeemedRewardPointsAmount;
            List<AppliedGiftCard> appliedGiftCards;
            var orderTotalWithDiscount = _payPalCartItemService.GetCartTotal(cart, out orderTotalDiscountAmount,
                out appliedDiscount,
                out redeemedRewardPoints,
                out redeemedRewardPointsAmount,
                out appliedGiftCards);

            decimal subTotalWithDiscount;
            decimal subTotalWithoutDiscount;
            Discount subTotalAppliedDiscount;
            decimal subTotalDiscountAmount;
            var itemTotalWithDiscount = _payPalCartItemService.GetCartItemTotal(cart,
                out subTotalDiscountAmount,
                out subTotalAppliedDiscount,
                out subTotalWithoutDiscount,
                out subTotalWithDiscount);

            var giftCardsAmount = appliedGiftCards.Sum(x => x.AmountCanBeUsed);

            itemTotalWithDiscount = itemTotalWithDiscount - orderTotalDiscountAmount - giftCardsAmount;

            var taxTotal = _payPalCartItemService.GetTax(cart);
            var shippingTotal = _payPalCartItemService.GetShippingTotal(cart);
            var items = GetPaymentDetailsItems(cart);

            if (orderTotalDiscountAmount > 0 || subTotalDiscountAmount > 0)
            {
                var discountItem = new PaymentDetailsItemType
                                       {
                                           Name = "Discount",
                                           Amount =
                                               (-orderTotalDiscountAmount + -subTotalDiscountAmount).GetBasicAmountType(
                                                   currencyCode),
                                           Quantity = "1"
                                       };

                items.Add(discountItem);
            }

            foreach (var appliedGiftCard in appliedGiftCards)
            {
                var giftCardItem = new PaymentDetailsItemType
                                       {
                                           Name = string.Format("Gift Card ({0})", appliedGiftCard.GiftCard.GiftCardCouponCode),
                                           Amount = (-appliedGiftCard.AmountCanBeUsed).GetBasicAmountType(currencyCode),
                                           Quantity = "1"
                                       };

                items.Add(giftCardItem);

            }

            return new[]
            {
                new PaymentDetailsType
                    {
                        OrderTotal = orderTotalWithDiscount.GetBasicAmountType(currencyCode),
                        ItemTotal = itemTotalWithDiscount.GetBasicAmountType(currencyCode),
                        TaxTotal = taxTotal.GetBasicAmountType(currencyCode),
                        ShippingTotal = shippingTotal.GetBasicAmountType(currencyCode),
                        PaymentDetailsItem = items.ToArray(),
                        PaymentAction = _payPalExpressCheckoutPaymentSettings.PaymentAction,
                        PaymentActionSpecified = true,
                        ButtonSource = "nopCommerce_Cart_EC"
                    }
            };
        }

        public BasicAmountType GetMaxAmount(IList<ShoppingCartItem> cart)
        {
            var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress);
            decimal toAdd = 0;
            if (getShippingOptionResponse.ShippingOptions != null && getShippingOptionResponse.ShippingOptions.Any())
            {
                toAdd = getShippingOptionResponse.ShippingOptions.Max(option => option.Rate);
            }
            var currencyCode = _payPalCurrencyCodeParser.GetCurrencyCodeType(_workContext.WorkingCurrency);
            var cartTotal = _payPalCartItemService.GetCartItemTotal(cart);
            return (cartTotal + toAdd).GetBasicAmountType(currencyCode);
        }

        private IList<PaymentDetailsItemType> GetPaymentDetailsItems(IList<ShoppingCartItem> cart)
        {
            return cart.Select(item => _payPalCartItemService.CreatePaymentItem(item)).ToList();
        }

        public string GetBuyerEmail()
        {
            return _workContext.CurrentCustomer != null
                       ? _workContext.CurrentCustomer.Email
                       : null;
        }
    }
}