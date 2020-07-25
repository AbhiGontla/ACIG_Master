using Core.Domain;
using Data;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWorks _unitOfWorks;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUnitOfWorks unitOfWorks, IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._unitOfWorks = unitOfWorks;
        }
        private Registration _cachedUser;
        public async Task SignIn(Registration user, bool isPersistent = false)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(user.MemberName))
                claims.Add(new Claim(ClaimTypes.Name, user.MemberName, ClaimValueTypes.String, "ACIG"));


            if (!string.IsNullOrEmpty(user.MemberMobileNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.MemberMobileNumber, ClaimValueTypes.String, "ACIG"));
            if (!string.IsNullOrEmpty(user.Iqama_NationalID))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Iqama_NationalID, ClaimValueTypes.String, "ACIG"));


            foreach (var roles in Enum.GetNames(typeof(RegistrationRole)))
                claims.Add(new Claim(ClaimTypes.Role, roles));

            var CustomerIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var CustomerPrincipal = new ClaimsPrincipal(CustomerIdentity);

            //set value indicating whether session is persisted and the time at which the authentication was issued
            var authenticationProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = isPersistent,
                IssuedUtc = DateTime.UtcNow
            };

            //sign in
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CustomerPrincipal, authenticationProperties);

            //cache authenticated customer
            _cachedUser = user;
        }

        public Registration ValidateUser(string nationalId, string pin)
        {
            var _customers = _unitOfWorks.RegistrationRepository.GetDbSet();
            return _customers.Where(c => (c.Iqama_NationalID == nationalId) && (c.ConfirmPin == pin)).FirstOrDefault();
        }
        public Registration GetAuthenticatedUser()
        {
            //whether there is a cached customer
            if (_cachedUser != null)
                return _cachedUser;

            //try to get authenticated Customer identity
            var authenticateResult = _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            if (!authenticateResult.Succeeded)
                return null;

            Registration user = null;

            //try to get customer by email
            var nationalId = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier
                && claim.Issuer.Equals("ACIG", StringComparison.InvariantCultureIgnoreCase));
            if (nationalId != null)
                user =  _unitOfWorks.RegistrationRepository.GetDbSet().Where(r => r.Iqama_NationalID.Equals(nationalId.Value)).FirstOrDefault();

            if (user == null)
                return null;

            //cache authenticated customer
            _cachedUser = user;

            return _cachedUser;
        }
        public virtual async Task SignOut()
        {
            //reset cached customer
            _cachedUser = null;

            //and sign out from the current authentication scheme
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        public bool IsAdmin()
        {
            var regUser = GetAuthenticatedUser();
            if (regUser.Role == (int)RegistrationRole.Admin)
                return true;
            else
                return false;
        }
    }
}
