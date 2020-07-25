using Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Registration ValidateUser(string nationalId, string pin);
        Task SignIn(Registration customer, bool isPersistent = false);
        Registration GetAuthenticatedUser();
        Task SignOut();
        bool IsAdmin();
    }
}
