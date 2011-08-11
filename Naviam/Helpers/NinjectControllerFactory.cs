using System;
using System.Web.Mvc;
using Naviam.Domain.Concrete;
using Ninject;
using Naviam.Domain.Abstract;

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
            _ninjectKernel.Bind<IUserAccountRepository>().To<UserAccountRepository>();
        }
    }
}