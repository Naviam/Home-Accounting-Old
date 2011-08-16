using System;
using System.Linq;
using System.Web.Mvc;

namespace Naviam.WebUI.Helpers
{
    public static class ControllerExtensions
    {
        //Enforeces model validation during testing. This is required because model binding doesn't occur in
        //during testing which means that ModelState.IsValid is always true
        //Taken from
        //http://blog.overridethis.com/blog/post/2010/07/08/MVC2-Validation-and-Testing-e28093-Refactored.aspx
        public static ActionResult CallWithModelValidation<C, R, T>(this C controller
            , Func<C, R> action
            , T model)
            where C : Controller
            where R : ActionResult
            where T : class
        {
            var provider = new DataAnnotationsModelValidatorProvider();

            var metadata = ModelMetadataProviders
                .Current
                .GetMetadataForProperties(model, typeof(T));
            foreach (var modelMetadata in metadata)
            {
                var validators = provider
                    .GetValidators(modelMetadata, new ControllerContext());

                foreach (var result in validators.Select(validator => validator.Validate(model)).SelectMany(results => results))
                {
                    controller.ModelState.AddModelError(modelMetadata.PropertyName, result.Message);
                }
            }
            return action(controller);
        }
    }
}