using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACIGConsumer.Factories;
using ACIGConsumer.Models;
using ACIGConsumer.Models.Approvals;
using Core;
using Core.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class ApprovalsController : Controller
    {
        private string _className = "ApprovalsController";
        private readonly ILogger<ApprovalsController> _logger;
        private ApprovalsHandler _approvalsHandler;
        private readonly IOptions<ApplConfig> appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthService _authenticationService;
        private readonly IListFactory _listFactory;
        string langcode;
        private readonly GetLang getLang;
        public ApprovalsController(IListFactory listFactory, IAuthService authenticationService, ILogger<ApprovalsController> logger,
            IOptions<ApplConfig> _config, ApprovalsHandler approvalsHandler, IHttpContextAccessor httpContextAccessor, GetLang _getLang)
        {
            this._authenticationService = authenticationService;
            this._listFactory = listFactory;
            _logger = logger;
            appSettings = _config;
            _approvalsHandler = approvalsHandler;
            _httpContextAccessor = httpContextAccessor;
            getLang = _getLang;
            langcode = getLang.GetLanguage();
        }
        public IActionResult Index()
        {
            ViewBag.lang = langcode;
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var _approvals = _approvalsHandler.GetApprovById(nationalId, yob);
            if(_approvals != null && _approvals.Count > 0)
            {
               _approvals = _approvals.OrderByDescending(x => x.CL_DATEOT).ToList();
            }
            return View(_approvals);
        }
        public IActionResult test()
        {
            return View();
        }


        public List<Approvals> GetApprovById()
        {
            return GetApprovalsByNationalId().Result;
        }
        public async Task<List<Approvals>> GetApprovalsByNationalId()
        {
            var regUser = _authenticationService.GetAuthenticatedUser();
            string nationalId = regUser.Iqama_NationalID;
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<Approvals>();
            try
            {
                if (nationalId != null && yob != null)
                {
                    var clsInput = new ClsInput();
                    clsInput.code = "CI";
                    clsInput.nationalID = nationalId;
                    DateTime date = Convert.ToDateTime(yob);
                    clsInput.yearOfBirth = date.Year.ToString();
                    clsInput.insPolicyNo = "";
                    result = await _approvalsHandler.GetApprovalsByNationalId(clsInput);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(_className + "::GetApprovalsByNationalId::" + ex.Message);
            }
            return result;
        }
        public IActionResult ListView()
        {

            return View();
        }
        public IActionResult ApprovalList()
        {
            try
            {
                var formCollection = Request.Form;

                var searchModel = new ApprovalSearchModel();

                if (!string.IsNullOrWhiteSpace(formCollection["pagination[page]"]))
                    searchModel.Page = int.Parse(formCollection["pagination[page]"]);

                if (!string.IsNullOrWhiteSpace(formCollection["pagination[pages]"]))
                    searchModel.Pages = int.Parse(formCollection["pagination[pages]"]);

                if (!string.IsNullOrWhiteSpace(formCollection["pagination[perpage]"]))
                {
                    var pageSize = int.Parse(formCollection["pagination[perpage]"]);
                    searchModel.Perpage = pageSize > 0 ? pageSize : 10;
                }

                if (!string.IsNullOrWhiteSpace(formCollection["pagination[total]"]))
                    searchModel.Total = int.Parse(formCollection["pagination[total]"]);

                if (!string.IsNullOrWhiteSpace(formCollection["sort[field]"]))
                    searchModel.Field = formCollection["sort[field]"];

                if (!string.IsNullOrWhiteSpace(formCollection["sort[sort]"]))
                    searchModel.Sort = formCollection["sort[sort]"];

                if (!string.IsNullOrWhiteSpace(formCollection["query[approvalSearch]"]))
                    searchModel.Query = formCollection["query[approvalSearch]"];
                var model = _listFactory.PrepareApprovalListModel(searchModel);
                return Json(model);
            }
            catch (Exception ex)
            {

            }
            return Json(new ApprovalListModel());
        }

    }
}
