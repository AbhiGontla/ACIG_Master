using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Core;
using Core.Api;
using Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class AccountsController : Controller
    {
        string langcode;
        private readonly GetLang getLang;
        private readonly IAuthService _authService;
        private ICustomerService _customerService;
        private CustomerHandler CustomerHandler;
        private ILogger<AccountsController> _logger;
        const string SessionNId = "_Name";
        const string SessionDOB = "_Age";
        const string SessionMobilenumber = "_mobilenumber";
        const string _className = "AccountsController";
        public AccountsController(IAuthService authService, ILogger<AccountsController> logger, GetLang _getLang, ICustomerService customerService, CustomerHandler customerHandler)
        {
            getLang = _getLang;
            _authService = authService;
            langcode = getLang.GetLanguage();
            _customerService = customerService;
            CustomerHandler = customerHandler;
            _logger = logger;
        }
        public IActionResult Register()
        {
            ViewBag.lang = langcode;
            return View();
        }

        public async Task<IActionResult> VerifyDetails(string nid, string dob)
        {
            ViewBag.lang = langcode;
            if (!string.IsNullOrWhiteSpace(nid) && !string.IsNullOrWhiteSpace(dob))
            {
                try
                {

                    var result = ValidateIDByNIC(nid, "");
                    if (result == true)
                    {
                        //var dateTime = Convert.ToDateTime(dob);
                        //var date = dateTime.ToString("dd-MM-yyyy");
                        ClsInput clsInput = new ClsInput() { nationalID = nid, yearOfBirth = dob };
                        _logger.LogInformation(_className + "::VerifyDetails::GetMemberByNationalId::START::" + clsInput.yearOfBirth);

                        RegistrationResponse res = await CustomerHandler.GetMemberByNationalId(clsInput);
                        _logger.LogInformation(_className + "::VerifyDetails::GetMemberByNationalId::Response::" + res.ResponseMessage );
                        if (res.Members != null && res.Members.Count > 0)
                        {
                            Registration _userdetails = new Registration();
                            _userdetails = res.Members.Where(c => c.MemberType == "EMPLOYEE").FirstOrDefault();
                            _logger.LogInformation(_className + "::VerifyDetails::GetMemberByNationalId::_userdetails::" + _userdetails.MemberName);
                            setsession(nid, dob, _userdetails.MemberMobileNo);
                            return Json(new { success = true });
                        }
                        else
                        {
                            var responseMessage = string.Empty;
                            if(res.Errors != null && res.Errors.Count > 0)
                            {
                                responseMessage = "Invalid National ID or Date of Birth.Please check again.";
                            }
                            else
                            {
                                responseMessage = res.ResponseMessage;
                            }
                            return Json(new { success = false, responseText = responseMessage });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Invalid Credentials." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(_className + "::VerifyDetails::" + ex.Message  + "::InnerException::" + ex.InnerException?.Message);
                }
                return View();

            }
            else
            {
                _logger.LogInformation(_className + "::VerifyDetails::nationalId is NULL");
                return View();
            }
        }
        public void setsession(string nid, string dob, string mobilenumber)
        {
            //string dt = dob.ToString("yyyy-MM-dd");
            HttpContext.Session.SetString(SessionNId, nid);
            HttpContext.Session.SetString(SessionDOB, dob);
            HttpContext.Session.SetString(SessionMobilenumber, mobilenumber);
        }
        public void setsession(string nid, DateTime dob)
        {
            string dt = dob.ToString("yyyy-MM-dd");
            HttpContext.Session.SetString(SessionNId, nid);
            HttpContext.Session.SetString(SessionDOB, dt);
        }
        public IActionResult VerifyOTP()
        {
            ViewBag.lang = langcode;
            return View();
        }
        public IActionResult CreatePin()
        {
            ViewBag.lang = langcode;
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.lang = langcode;
            return View();
        }
        public async Task<IActionResult> RegisterUser(string enterpin, string confirmpin)
        {
            string status = "false";
            try
            {
                var nationalId = HttpContext.Session.GetString(SessionNId);
                var dateofbirth = HttpContext.Session.GetString(SessionDOB);
                ClsInput input = new ClsInput();
                input.nationalID = nationalId;
                input.yearOfBirth = dateofbirth;
                _logger.LogInformation(_className + "::RegisterUser::START");
                var response = await CustomerHandler.GetMemberByNationalId(input);
                _logger.LogInformation(_className + "::RegisterUser::response::MemberCount" + response.ResponseMessage);
                if (response.Members != null && response.Members.Count > 0)
                {
                    _logger.LogInformation(_className + "::RegisterUser::response::MemberCount" + response.Members.Count);
                    var EmpMember = response.Members.Where(c => c.MemberType == "EMPLOYEE").FirstOrDefault();
                    EmpMember.Iqama_NationalID = nationalId;
                    EmpMember.DOB = dateofbirth;
                    EmpMember.CreatePin = enterpin;
                    EmpMember.ConfirmPin = confirmpin;
                    EmpMember.MemberMobileNumber = EmpMember.MemberMobileNo;
                    EmpMember.Role = (int)RegistrationRole.Customer;
                    _customerService.Insert(EmpMember);
                    var members = response.Members.FindAll(c => c.MemberType != "EMPLOYEE");
                    if (members != null)
                    {
                        List<Relations> relations = new List<Relations>();
                        foreach (var member in members)
                        {
                            var relation = new Relations();
                            relation.RegistrationId = EmpMember.Id;
                            relation.CardNo = member.CardNo;
                            relation.ClassCode = member.ClassCode;
                            relation.ClassName = member.ClassName;
                            relation.DOB = member.DOB;
                            relation.Iqama_NationalID = nationalId;
                            relation.MemberName = member.MemberName;
                            relation.MemberType = member.MemberType;
                            relation.TPAID = member.TPAID;
                            relation.TPAName = member.TPAName;
                            relation.TushfaMemberNo = member.TushfaMemberNo;
                            relation.NetworkName = member.NetworkName;
                            relation.NetworkCode = member.NetworkCode;
                            relation.PolicyToDate = member.PolicyToDate;
                            relation.PolicyFromDate = member.PolicyFromDate;
                            relation.MemberStatus = member.MemberStatus;
                            relation.Iqama_NationalID = nationalId;
                            relation.DOB = dateofbirth;
                            relation.MemberMobileNumber = member.MemberMobileNo;
                            relations.Add(relation);
                        }
                        foreach (var rel in relations)
                        {
                            _customerService.InsertRelations(rel);
                        }
                    }
                    await _authService.SignIn(EmpMember);
                    TempData["NationalId"] = nationalId;
                    TempData["YOB"] = dateofbirth;
                    TempData.Keep("YOB");
                    TempData.Keep("NationalId");
                    status = "true";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::RegisterUser::Exception" + ex.Message);
            }
            if (status == "false")
            {
                return Json(new { success = false, responseText = "User Registration Failed." });
            }
            else
            {
                return Json(new { success = true, responseText = "User Registration Success." });
            }

        }


        public static bool ValidateIDByNIC(string IDNumber, string idType)
        {
            var _output = true;
            try
            {
                if (IDNumber.Length != 10)
                {
                    return false;
                }

                int nSum = 0;
                string lastDigit = "";
                int nDigit = 0;

                for (int i = 0; i < 9; i++)
                {
                    nDigit = Convert.ToByte(IDNumber[i]) - 48;
                    if (i % 2 == 0) // If Odd position digit double it
                    {
                        if ((nDigit * 2) > 9)
                            nSum += Convert.ToByte(((nDigit * 2).ToString().Substring(0, 1))) + Convert.ToByte(((nDigit * 2).ToString().Substring(1, 1)));
                        else
                            nSum += nDigit * 2;
                    }
                    else
                        nSum += nDigit;
                }

                lastDigit = (nSum % 10).ToString();
                if (lastDigit != "0")
                {
                    lastDigit = ((10 - (Convert.ToByte(lastDigit)))).ToString();
                }

                if (lastDigit == IDNumber.Substring(9, 1))
                    _output = true;
                else
                    _output = false;
            }
            catch (Exception ex)
            {
                _output = false;
            }
            return _output;
        }

        public bool ValidateUser(string nid, DateTime dob)
        {
            bool status = true;
            try
            {
                string dt = dob.ToString("yyyy-MM-dd");
                var usersList = CustomerHandler.GetAllUsers();
                var userdetails = usersList.Result.Where(c => (c.Iqama_NationalID == nid) && (c.DOB == dt)).FirstOrDefault();
                if (userdetails != null)
                {
                    return status;
                }
                else
                {
                    status = false;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return status;
        }

    }
}
