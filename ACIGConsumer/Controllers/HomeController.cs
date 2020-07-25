using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACIGConsumer.Models;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using Microsoft.AspNetCore.Razor.Language;
using Core.Domain;
using Microsoft.AspNetCore.Authorization;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Core;
using Services.RequestHandler;
using ACIGConsumer.Models.Home;
using System.IO;

namespace ACIGConsumer.Controllers
{

    public class HomeController : Controller
    {
        private CustomerHandler CustomerHandler;
        const string SessionMobilenumber = "_mobilenumber";
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileService _fileService;
        string langcode;
        private readonly GetLang getLang;
        private PolicyHandler policyHandler;
        private ApprovalsHandler ApprovalsHandler;
        public HomeController(IAuthService authService, IFileService fileService, ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor,
     GetLang _getLang, PolicyHandler policy, ApprovalsHandler _approvalsHandler)
        {
            _logger = logger;
            _authenticationService = authService;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            getLang = _getLang;
            langcode = getLang.GetLanguage();
            policyHandler = policy;
            ApprovalsHandler = _approvalsHandler;
        }

        public IActionResult Index(Registration _data)
        {
            try
            {

                string nationalId = TempData["NationalId"].ToString();
                string yob = TempData["YOB"].ToString();
                TempData.Keep("YOB");
                TempData.Keep("NationalId");
                if (!_authenticationService.IsAdmin())
                {
                    ViewBag.lang = langcode;
                    var policies = policyHandler.GetPoliciesById(nationalId, yob);
                    var approvals = ApprovalsHandler.GetApprovById(nationalId, yob);
                    if(policies != null && policies.Count > 0)
                    {
                        ViewBag.Policies = policies.OrderByDescending(x => x.PolicyToDate).FirstOrDefault();
                    }
                    if(approvals != null && approvals.Count > 0)
                    {
                        ViewBag.Approvals = approvals.OrderByDescending(x => x.CL_DATEOT).FirstOrDefault();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("HomeController::Index::" + ex.Message);
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AddBanner()
        {
            return View();
        }
        public async Task<IActionResult> Banner(BannerModel model)
        {
            if (model.Image == null || model.Image.Length == 0)
                return Content("file not selected");
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("ACIG_POlicy" + ":" + "ACIG@123");
            string val = System.Convert.ToBase64String(plainTextBytes);
            try
            {
                var fileName = model.Name + "_" + Guid.NewGuid();
                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), @"wwwroot\Banners\",
                             fileName);
                var extension = Path.GetExtension(model.Image.FileName);
                using (var stream = new FileStream(path + extension, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }
                _logger.LogInformation("HomeController::Banner::" + "UploadedFile::" + path);
                var bannerList = _fileService.GetBanners();
                Banners banner = new Banners();
                if (bannerList.Any(b => b.priority.Equals(model.priority)))
                {
                    banner = bannerList.FirstOrDefault(b => b.priority.Equals(model.priority));
                    banner.ImageUrl = @"\Banners\" + fileName + extension;
                    banner.Name = model.Name;
                    banner.priority = model.priority;
                    banner.Type = model.Type;
                    _fileService.UpdateBanner(banner);
                }
                else
                {
                    banner.ImageUrl = @"\Banners\" + fileName + extension;
                    banner.Name = model.Name;
                    banner.priority = model.priority; 
                    banner.Type = model.Type;
                    _fileService.InsertBanner(banner);
                }
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("HomeController::Banner::" + ex.Message);
            }
            return Json(new { Success = false });
        }
    }
}
