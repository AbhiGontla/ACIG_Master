using Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Components.QuickActionsPanel
{
    public class QuickActionsPanelViewComponent : ViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IAuthService _authenticationService;
        public QuickActionsPanelViewComponent(ICustomerService customerService, IAuthService authService)
        {
            _authenticationService = authService;
            _customerService = customerService;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.requestPath = HttpContext.Request.Path.Value;
            var user = _authenticationService.GetAuthenticatedUser();
            if(user.Role != (int)RegistrationRole.Admin)
            {
                var relations = _customerService.GetRelationsByRegistrationId(user.Id);
                ViewBag.Relations = relations;
            }
            return View();
        }
    }
}
