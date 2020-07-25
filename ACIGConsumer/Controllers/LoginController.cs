using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ACIGConsumer.Models;
using Core;
using Core.Domain;
using Core.Sms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class LoginController : Controller
    {
        private CustomerHandler CustomerHandler;
        const string SessionMobilenumber = "_mobilenumber";
        private  ICustomerService _customerService;
        private readonly IOptions<ApplConfig> appSettings;
        private readonly IAuthService _authenticationService;
        string langcode;
        private readonly GetLang getLang;
        private readonly ILogger<LoginController> _logger;
        public LoginController(CustomerHandler customerHandler, ILogger<LoginController> logger, IAuthService authenticationService, ICustomerService customerService, IOptions<ApplConfig> _config, GetLang _getLang)
        {
            CustomerHandler = customerHandler;
            _customerService = customerService;
            _logger = logger;
            appSettings = _config;
            getLang = _getLang;
            langcode = getLang.GetLanguage();
            _authenticationService = authenticationService;
        }
        public IActionResult Index()
        {
            //string link = "https://localhost:44310/Test/Login?lang=en";
            return RedirectToAction("Login", "Accounts", new { lang = "en" });
        }
        public IActionResult sendsms()
        {

            string mobno = HttpContext.Session.GetString(SessionMobilenumber);
            var response = CustomerHandler.SendSms(mobno);

            if (response.RequestStatus.ToString() == "Success")
            {
                TempData["SentOTP"] = response.OTPSent;
                TempData.Keep("SentOTP");
                return Json(new { success = true, responseText = "Sending OTP Success.", sentotp = response.OTPSent });
            }
            else
            {
                return Json(new { success = false, responseText = "Sending OTP Failed." });
            }

        }
        //public IActionResult sendsms()
        //{
        //    var otp = GenerateRandomNo();
        //    string url = appSettings.Value.SmsConfig.url;
        //    string uname = appSettings.Value.SmsConfig.userName;
        //    string pwd = appSettings.Value.SmsConfig.password;
        //    string sender = appSettings.Value.SmsConfig.senderName;
        //    string mobilenumber = "966508095931";
        //    string message = "Dear Customer,Your One Time Password(OTP):" + otp;
        //    SmsRequest request = new SmsRequest();
        //    var response=request.SmsHandler(mobilenumber, message);

        //    if (response.ToString() == "Success")
        //    {
        //        TempData["SentOTP"] = otp;
        //        TempData.Keep("SentOTP");
        //        return Json(new { success = true, responseText = "Sending OTP Success.", sentotp = otp });
                
        //    }
        //    else
        //    {
        //        return Json(new { success = false, responseText = "Sending OTP Failed." });
        //    }           

        //}
     
        //Generate RandomNo
        public int GenerateRandomNo()
        {
            Random _rdm = new Random();
            int _min = 1000;
            int _max = 9999;
            return _rdm.Next(_min, _max);
        }

        [HttpGet]
        public async Task<IActionResult> ValidateUser(string nid, string pin)
        {
            try
            {
                Registration Item = _customerService.ValidateCustomer(nid, pin);
                if (Item == null)
                {
                    return Json(new { success = false, responseText = "Login Failed." });
                }
                else
                {
                    await _authenticationService.SignIn(Item, false);
                    //second request, get value marking it from deletion
                    TempData["NationalId"] = Item.Iqama_NationalID;
                    TempData["YOB"] = Item.DOB;
                    //later on decide to keep it
                    TempData.Keep("YOB");
                    TempData.Keep("NationalId");
                    return Json(new { success = true, responseText = "Login Success.", isAdmin = _authenticationService.IsAdmin() });
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("LoginController::ValidateUser::" + ex.Message);

            }
            return Json(new { success = false, responseText = "Something went wrong." });

        }
       
        public IActionResult RegisterUser(string enterpin, string confirmpin)
        {
            string status = "false";
            try
            {
                Registration _userdetails = new Registration();
                _userdetails.Iqama_NationalID = TempData["nId"].ToString();                
                _userdetails.DOB = TempData["dob"].ToString();
                _userdetails.CreatePin = enterpin;
                _userdetails.ConfirmPin = confirmpin;
                _customerService.Insert(_userdetails);
                TempData["NationalId"] = _userdetails.Iqama_NationalID;
                DateTime dt = Convert.ToDateTime(TempData["dob"].ToString());

                //Know the year

                int year = dt.Year;
                TempData["YOB"] = year;
                TempData.Keep("YOB");
                TempData.Keep("NationalId");
                status = "true";
            }
            catch (Exception ex)
            {
                _logger.LogInformation("LoginController::RegisterUser::" + ex.Message); 
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



        public async Task<IActionResult> Logout()
        {
            await _authenticationService.SignOut();
            //string link = "https://localhost:44310/Test/Login?lang=en";
            return RedirectToAction("Login", "Accounts", new { lang="en" });
        }

        public IActionResult validateOTP(string otp)
        {
            TempData["SentOTP"] = otp;
            TempData.Keep("SentOTP");
            if (TempData["SentOTP"].ToString() == otp)
            {
                return Json(new { success = true, responseText = "OTP Verified." });
             
            }
            else
            {
                return Json(new { success = false, responseText = "OTP Failed." });
            }
        }
    }
}
