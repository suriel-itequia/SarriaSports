﻿//Contributor: Noel Revuelta

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Sermepa.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Sermepa
{
    /// <summary>
    /// Sermepa payment processor
    /// </summary>
    public class SermepaPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly SermepaPaymentSettings _sermepaPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public SermepaPaymentProcessor(SermepaPaymentSettings sermepaPaymentSettings,
            ISettingService settingService, IWebHelper webHelper)
        {
            this._sermepaPaymentSettings = sermepaPaymentSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets Sermepa URL
        /// </summary>
        /// <returns></returns>
        private string GetSermepaUrl()
        {
            return _sermepaPaymentSettings.Pruebas ? "https://sis-t.sermepa.es:25443/sis/realizarPago" :
                "https://sis.sermepa.es/sis/realizarPago";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //Notificación On-Line
            string strDs_Merchant_MerchantURL = _webHelper.GetStoreLocation(false) + "Plugins/PaymentSermepa/Return";

            //URL OK
            string strDs_Merchant_UrlOK = _webHelper.GetStoreLocation(false) + "checkout/completed";

            //URL KO
            string strDs_Merchant_UrlKO = _webHelper.GetStoreLocation(false) + "Plugins/PaymentSermepa/Error";

            //Numero de pedido
            //You have to change the id of the orders table to begin with a number of at least 4 digits.
            string strDs_Merchant_Order = postProcessPaymentRequest.Order.Id.ToString("0000");

            //Nombre del comercio
            string strDs_Merchant_MerchantName = _sermepaPaymentSettings.NombreComercio;

            //Importe
            string amount = ((int)Convert.ToInt64(postProcessPaymentRequest.Order.OrderTotal * 100)).ToString();
            string strDs_Merchant_Amount = amount;

            //Código de comercio
            string strDs_Merchant_MerchantCode = _sermepaPaymentSettings.FUC;

            //Moneda
            string strDs_Merchant_Currency = _sermepaPaymentSettings.Moneda;

            //Terminal
            string strDs_Merchant_Terminal = _sermepaPaymentSettings.Terminal;

            //Tipo de transaccion (0 - Autorización)
            string strDs_Merchant_TransactionType = "0";

            //Clave
            string clave = "";
            if (_sermepaPaymentSettings.Pruebas)
            {
                clave = _sermepaPaymentSettings.ClavePruebas;
            }
            else
            {
                clave = _sermepaPaymentSettings.ClaveReal;
            }

            //Calculo de la firma
            string SHA = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                strDs_Merchant_Amount,
                strDs_Merchant_Order,
                strDs_Merchant_MerchantCode,
                strDs_Merchant_Currency,
                strDs_Merchant_TransactionType,
                strDs_Merchant_MerchantURL,
                clave);

            byte[] SHAresult;
            SHA1 shaM = new SHA1Managed();
            SHAresult = shaM.ComputeHash(Encoding.Default.GetBytes(SHA));
            string SHAresultStr = BitConverter.ToString(SHAresult).Replace("-", "");

            //Creamos el POST
            var remotePostHelper = new RemotePost();
            remotePostHelper.FormName = "form1";
            remotePostHelper.Url = GetSermepaUrl();

            remotePostHelper.Add("Ds_Merchant_Amount", strDs_Merchant_Amount);
            remotePostHelper.Add("Ds_Merchant_Currency", strDs_Merchant_Currency);
            remotePostHelper.Add("Ds_Merchant_Order", strDs_Merchant_Order);
            remotePostHelper.Add("Ds_Merchant_MerchantCode", strDs_Merchant_MerchantCode);
            remotePostHelper.Add("Ds_Merchant_TransactionType", strDs_Merchant_TransactionType);
            remotePostHelper.Add("Ds_Merchant_MerchantURL", strDs_Merchant_MerchantURL);
            remotePostHelper.Add("Ds_Merchant_MerchantSignature", SHAresultStr);
            remotePostHelper.Add("Ds_Merchant_Terminal", strDs_Merchant_Terminal);
            remotePostHelper.Add("Ds_Merchant_MerchantName", strDs_Merchant_MerchantName);
            remotePostHelper.Add("Ds_Merchant_UrlOK", strDs_Merchant_UrlOK);
            remotePostHelper.Add("Ds_Merchant_UrlKO", strDs_Merchant_UrlKO);

            remotePostHelper.Post();
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _sermepaPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentSermepa";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Sermepa.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentSermepa";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Sermepa.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentSermepaController);
        }

        public override void Install()
        {
            var settings = new SermepaPaymentSettings()
            {
                NombreComercio = "",
                Titular = "",
                Producto = "",
                FUC = "",
                Terminal = "",
                Moneda = "",
                ClaveReal = "",
                ClavePruebas = "",
                Pruebas = true,
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            base.Install();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        #endregion
    }
}
