using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace Naviam.WebUI.Helpers
{
    public class MyFormValueProviderFactory : ValueProviderFactory
    {

        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            return new MyFormValueProvider(controllerContext);
        }

    }

    public class MyFormValueProvider : IValueProvider
    {
        private ControllerContext _context;

        // Ctor

        public MyFormValueProvider(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this._context = context;
        }

        // methods

        protected virtual bool ContainsPrefix(string prefix)
        {
            string val = this._context.HttpContext.Request.Form[prefix.Replace('.', '[') + ']'] as string;
            return val != null;
        }

        protected virtual ValueProviderResult GetValue(string key)
        {
            ValueProviderResult res = null;
            string val = this._context.HttpContext.Request.Form[key.Replace('.', '[')+']'] as string;
            if (val != null)
                res = new ValueProviderResult(val, val, CultureInfo.CurrentCulture);
            return res; 
        }

        // IValueProvider members

        bool IValueProvider.ContainsPrefix(string prefix)
        {
            return this.ContainsPrefix(prefix);
        }

        ValueProviderResult IValueProvider.GetValue(string key)
        {
            return this.GetValue(key);
        }
    }
}