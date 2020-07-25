using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACIGConsumer.Factories;
using ACIGConsumer.Models.CoverageLimits;
using Core;
using Core.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class CoverageLimitsController : Controller
    {
        private string _className = "CoverageLimitsController";
        private readonly ILogger<CoverageLimitsController> _logger;
        private CoverageBalanceHandler _coverageHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IListFactory _listFactory;
        string langcode;
        private readonly GetLang getLang;
        public CoverageLimitsController(IListFactory listFactory,ILogger<CoverageLimitsController> logger, CoverageBalanceHandler coverageHandler, IHttpContextAccessor httpContextAccessor, GetLang _getLang)
        {
            _listFactory = listFactory;
            _logger = logger;
            _coverageHandler = coverageHandler;
            _httpContextAccessor = httpContextAccessor;
            getLang = _getLang;
            langcode = getLang.GetLanguage();
        }
        public IActionResult Index()
        {
            ViewBag.lang = langcode;
            var _coverages = GetCoveragesById();
            return View(_coverages);
        }

        public IActionResult Details(int id)
        {
            ViewBag.lang = langcode;
            var _coverages = GetCoveragesById();
            var _coverageDetails = _coverages.Where(c => (c.Id == id));
            return View(_coverageDetails);
        }

        public List<CoverageBalance> GetCoveragesById()
        {
            return GetCoveragesByNationalId().Result;
        }
        public async Task<List<CoverageBalance>> GetCoveragesByNationalId()
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<CoverageBalance>();
            try
            {
                if (nationalId != null && yob != null)
                {
                    var clsInput = new ClsInput();
                    clsInput.code = "";
                    clsInput.nationalID = nationalId;
                    DateTime date = Convert.ToDateTime(yob);
                    clsInput.yearOfBirth = date.Year.ToString();
                    clsInput.insPolicyNo = "";
                    result = await _coverageHandler.GetCoveragesByNationalId(clsInput);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetCoveragesByNationalId::" + ex.Message);
            }
            return result;
        }

        public IActionResult CovergeLimitList()
        {
            try
            {
                var formCollection = Request.Form;

                var searchModel = new CoverageBalanceSearchModel();

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

                if (!string.IsNullOrWhiteSpace(formCollection["query[coverageSearch]"]))
                    searchModel.Query = formCollection["query[coverageSearch]"];
                var model = _listFactory.PrepareCoverageBalanceListModel(searchModel);
                return Json(model);
            }
            catch (Exception ex)
            {

            }
            return Json(new CoverageBalanceListModel());
        }
        public IActionResult ListView()
        {
            return View();
        }
    }
}
