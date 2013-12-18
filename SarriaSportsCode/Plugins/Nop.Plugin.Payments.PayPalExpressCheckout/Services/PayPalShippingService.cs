using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Orders;
using System.Linq;

namespace Nop.Plugin.Payments.PayPalExpressCheckout.Services
{
    public class PayPalShippingService : IPayPalShippingService
    {
        private readonly PayPalExpressCheckoutPaymentSettings _payPalExpressCheckoutPaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IPayPalCurrencyCodeParser _payPalCurrencyCodeParser;

        public PayPalShippingService(PayPalExpressCheckoutPaymentSettings payPalExpressCheckoutPaymentSettings, IOrderTotalCalculationService orderTotalCalculationService, ICurrencyService currencyService, IWorkContext workContext, IPayPalCurrencyCodeParser payPalCurrencyCodeParser)
        {
            _payPalExpressCheckoutPaymentSettings = payPalExpressCheckoutPaymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _workContext = workContext;
            _payPalCurrencyCodeParser = payPalCurrencyCodeParser;
        }

        public string GetRequireConfirmedShippingAddress(IEnumerable<ShoppingCartItem> cart)
        {
            return cart.All(item => item.Product.IsDownload)
                       ? "0"
                       : _payPalExpressCheckoutPaymentSettings.RequireConfirmedShippingAddress
                             ? "1"
                             : "0";
        }

        public string GetNoShipping(IEnumerable<ShoppingCartItem> cart)
        {
            return cart.Any(item => !item.Product.IsDownload)
                       ? "2"
                       : "1";
        }
    }
}