using System;
using System.Web.Mvc;
using Naviam.WebUI.Controllers;
using Ninject;

namespace Naviam.WebUI.Helpers
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private readonly IKernel _ninjectKernel;

        public NinjectControllerFactory()
        {
            _ninjectKernel = new StandardKernel();
            AddBindings();
        }

        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            return controllerType == null ? null : (IController)_ninjectKernel.Get(controllerType);
        }

        private void AddBindings()
        {
            // put bindings here
            _ninjectKernel.Bind<IFormsAuthentication>().To<FormsAuthenticationService>();
            _ninjectKernel.Bind<ICacheWrapper>().To<CacheWrapper>();
        }
    }
}