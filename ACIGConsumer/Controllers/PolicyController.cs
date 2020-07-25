using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACIGConsumer.Factories;
using ACIGConsumer.Models.Policy;
using Core;
using Core.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class PolicyController : Controller
    {

        private readonly ILogger<PolicyController> _logger;
        private PolicyHandler _policyHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        string langcode;
        private readonly GetLang getLang;
        private readonly IListFactory _listFactory;
        public PolicyController(ILogger<PolicyController> logger,
            PolicyHandler policyHandler, IHttpContextAccessor httpContextAccessor, IListFactory listFactory, GetLang _getLang)
        {
            _logger = logger;
            _policyHandler = policyHandler;
            _httpContextAccessor = httpContextAccessor;
            getLang = _getLang;
            this._listFactory = listFactory;
            langcode = getLang.GetLanguage();
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.lang = langcode;
            var _policies = GetPoliciesById().OrderByDescending(x => x.PolicyToDate);
            return View(_policies);
        }

        [HttpPost]
        public IActionResult Index(int? id)
        {
            return PartialView("_policyDetails");
        }

        public IActionResult Details(string id)
        {
            ViewBag.lang = langcode;
            var _policies = GetPoliciesById();
            var _policydetails = _policies.Where(c => (c.PolicyNumber == id)).FirstOrDefault();
            return View(_policydetails);
        }

        public IActionResult medicaladvice()
        {
            ViewBag.lang = langcode;
            return View();
        }
        public IActionResult medicaldetails()
        {
            ViewBag.lang = langcode;
            return View();
        }

        public List<Policies> GetPoliciesById()
        {
            return GetPoliciesByNationalId().Result;
        }
        public async Task<List<Policies>> GetPoliciesByNationalId()
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<Policies>();
            try
            {
                if (nationalId != null && yob != null)
                {
                    var clsInput = new ClsInput();
                    clsInput.code = "CI";
                    clsInput.nationalID = nationalId;
                    //DateTime date = DateTime.Parse(yob);
                    //DateTime date = Convert.ToDateTime(yob);
                    DateTime dt = Convert.ToDateTime(yob);
                    int year = dt.Year;
                    clsInput.yearOfBirth = year.ToString();
                    clsInput.insPolicyNo = "";
                    result = await _policyHandler.GetPoliciesByNationalId(clsInput);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("PolicyController::GetPoliciesByNationalId::" + ex.Message);
            }
            return result;
        }

        public IActionResult some()
        {
            return null;
        }
        public IActionResult ListView()
        {
            return View();
        }
        public IActionResult PolicyList()
        {
            try
            {
                var formCollection = Request.Form;

                var searchModel = new PolicySearchModel();

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

                if (!string.IsNullOrWhiteSpace(formCollection["query[policySearch]"]))
                    searchModel.Query = formCollection["query[policySearch]"];
                var model = _listFactory.PreparePolicyListModel(searchModel);
                return Json(model);
            }
            catch (Exception ex)
            {

            }
            return Json(new PolicyListModel());
        }

    }
}
