﻿using Autofac;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.PayPalExpressCheckout.Services;

namespace Nop.Plugin.Payments.PayPalExpressCheckout
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<PayPalCartItemService>().As<IPayPalCartItemService>();
            builder.RegisterType<PayPalCurrencyCodeParser>().As<IPayPalCurrencyCodeParser>();
            builder.RegisterType<PayPalInterfaceService>().As<IPayPalInterfaceService>();
            builder.RegisterType<PayPalOrderService>().As<IPayPalOrderService>();
            builder.RegisterType<PayPalRequestService>().As<IPayPalRequestService>();
            builder.RegisterType<PayPalSecurityService>().As<IPayPalSecurityService>();
            builder.RegisterType<PayPalShippingService>().As<IPayPalShippingService>();
            builder.RegisterType<PayPalUrlService>().As<IPayPalUrlService>();
            builder.RegisterType<PayPalCheckoutDetailsService>().As<IPayPalCheckoutDetailsService>();
            builder.RegisterType<PayPalRecurringPaymentsService>().As<IPayPalRecurringPaymentsService>();
            builder.RegisterType<PayPalExpressCheckoutConfirmOrderService>()
                   .As<IPayPalExpressCheckoutConfirmOrderService>();
            builder.RegisterType<PayPalExpressCheckoutPlaceOrderService>()
                   .As<IPayPalExpressCheckoutPlaceOrderService>();
            builder.RegisterType<PayPalExpressCheckoutService>()
                   .As<IPayPalExpressCheckoutService>();
            builder.RegisterType<PayPalExpressCheckoutShippingMethodService>()
                   .As<IPayPalExpressCheckoutShippingMethodService>();
            builder.RegisterType<PayPalExpressCheckoutShippingAddressService>()
                   .As<IPayPalExpressCheckoutShippingAddressService>();
            builder.RegisterType<PayPalRecurringPaymentsService>().As<IPayPalRecurringPaymentsService>();
            builder.RegisterType<PayPalRedirectionService>().As<IPayPalRedirectionService>();
            builder.RegisterType<PayPalIPNService>().As<IPayPalIPNService>();
        }

        public int Order { get { return 99; } }
    }
}