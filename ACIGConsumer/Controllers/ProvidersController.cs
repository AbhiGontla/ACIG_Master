using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACIGConsumer.Factories;
using ACIGConsumer.Models.Providers;
using Core;
using Core.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class ProvidersController : Controller
    {
        private string _className = "Providers";
        private readonly ILogger<ProvidersController> _logger;
        private ProvidersHandler _providersHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IListFactory _listFactory;
        string langcode;
        private readonly GetLang getLang;
        public ProvidersController(IListFactory listFactory, ILogger<ProvidersController> logger, ProvidersHandler providersHandler, IHttpContextAccessor httpContextAccessor, GetLang _getLang)
        {
            _logger = logger;
            this._listFactory = listFactory;
            _providersHandler = providersHandler;
            _httpContextAccessor = httpContextAccessor;
            getLang = _getLang;
            langcode = getLang.GetLanguage();
        }
        public IActionResult Index()
        {
            var _providers = GetProvidersById();
            ViewBag.lang = langcode;
            return View(_providers);
        }
        public IActionResult Details(string id)
        {
            var _providers = GetProvidersById();
            var _provdetails = _providers.Where(c => c.ProviderNumber == id);
            ViewBag.lang = langcode;
            return View(_provdetails);
        }

        public List<Providers> GetProvidersById()
        {
            return GetProvidersByNationalId().Result;
        }
        public async Task<List<Providers>> GetProvidersByNationalId()
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<Providers>();
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
                    result = await _providersHandler.GetProvidersByNationalId(clsInput);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetReImDetailsById:: " + ex.Message);
            }
            return result;
        }
        public IActionResult ListView()
        {
            return View();
        }
        public IActionResult ProvidersList()
        {
            try
            {
                var formCollection = Request.Form;

                var searchModel = new ProvidersSearchModel();

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

                if (!string.IsNullOrWhiteSpace(formCollection["query[providerSearch]"]))
                    searchModel.Query = formCollection["query[providerSearch]"];
                var model = _listFactory.PrepareProvidersListModel(searchModel);
                return Json(model);
            }
            catch (Exception ex)
            {

            }
            return Json(new ProvidersListModel());
        }
    }
}
