using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace Naviam.Code
{
    public class CustomDecimalModelBinder : DefaultModelBinder 
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(Nullable<decimal>))
            {
                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (valueProviderResult != null)
                {
                    decimal? result = null;
                    if (!String.IsNullOrEmpty(valueProviderResult.AttemptedValue))
                        result = Convert.ToDecimal(valueProviderResult.AttemptedValue);
                    return result;
                }
            }
            return base.BindModel(controllerContext, bindingContext);
        }
    }
    
    public class CustomDateTimeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(Nullable<DateTime>))
            {
                ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (valueProviderResult != null)
                {
                    DateTime? result = null;
                    if (!String.IsNullOrEmpty(valueProviderResult.AttemptedValue) && valueProviderResult.AttemptedValue.Contains("/Date("))
                    {
                        var st = valueProviderResult.AttemptedValue.Substring(6).Replace(")/", "");
                        result = new DateTime(Convert.ToInt64(st) * 10000 +  (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks).ToLocalTime();
                    }
                    return result;
                }
            }
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}