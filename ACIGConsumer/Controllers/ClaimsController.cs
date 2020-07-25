using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ACIGConsumer.Factories;
using ACIGConsumer.Models.Claims;
using Core;
using Core.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Services.RequestHandler;

namespace ACIGConsumer.Controllers
{
    public class ClaimsController : Controller
    {
        private string _className = "Claims";
        private ClaimsHandler _claimsHandler;
        private ICustomerService _customerService;
        private readonly IAuthService _authenticationService;
        private readonly ILogger<ClaimsController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IListFactory _listFactory;
        string langcode;
        private readonly GetLang getLang;
        private CustomerHandler CustomerHandler;
        const string SessionShowDialog = "_showDialog";

        public ClaimsController(IAuthService authService, ICustomerService customerService, IListFactory listFactory, ILogger<ClaimsController> logger, IHttpContextAccessor httpContextAccessor, GetLang _getLang,
            ClaimsHandler claimsHandler, CustomerHandler customerHandler)
        {
            _authenticationService = authService;
            this._listFactory = listFactory;
            this._customerService = customerService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            getLang = _getLang;
            langcode = getLang.GetLanguage();
            _claimsHandler = claimsHandler;
            CustomerHandler = customerHandler;
        }
        public IActionResult Index()
        {
            ViewBag.lang = langcode;
            var _paidclaims = GetPaidClaimsById();

            //var _osClaims = GetOSClaimsById();
            var claimsResponse = new ClaimsResponse();
            //if(_osClaims != null)
            //    claimsResponse.OSClaims = _osClaims;
            if(_paidclaims != null)
                claimsResponse.PaidClaims = _paidclaims.OrderByDescending(c => c.POLICY_INC).ToList();
            return View(claimsResponse);
        }

        public IActionResult ProviderPaidDetails(int id)
        {
            string Nid = TempData["NationalId"].ToString();
            TempData.Keep("NationalId");
            ViewBag.lang = langcode;
            var _paidclaims = GetPaidClaimsById();
            var _providersDetail = _paidclaims.Find(c => c.Id == id);
            var details = new ProviderClaimsDetails();
            details.paidClaims = _providersDetail;
            details.registration = _customerService.GetCustomerById(Nid);
            return View(details);
        }
        public IActionResult ProviderOSDetails(int id)
        {
            string Nid = TempData["NationalId"].ToString();
            TempData.Keep("NationalId");
            ViewBag.lang = langcode;
            var _osclaims = GetOSClaimsById();
            var _osclaimDetail = _osclaims.Find(c => c.Id == id);
            var details = new ProviderOSClaimsDetails();
            details.OSClaims = _osclaimDetail;
            details.registration = CustomerHandler.GetCustomerById(Nid);
            return View(details);
        }
        public IActionResult ReimbursmentClaims()
        {
            ViewBag.lang = langcode;
            List<ReImClaims> _reimclaims = new List<ReImClaims>();
            try
            {
                var query = GetReimClaimsByClientId();
                if (query != null && query.Count > 0)
                {
                    _reimclaims = query.OrderByDescending(c => c.RequestDate).ToList();
                }
                string showdialog = HttpContext.Session.GetString(SessionShowDialog);
                string status = "";
                if (showdialog != null)
                {
                    status = HttpContext.Session.GetString(SessionShowDialog);
                }
                ViewBag.showdialog = status;
            }
            catch (Exception ex)
            {
                _logger.LogError(_className + "::ReimbursmentClaims::" + ex);
            }
            return View(_reimclaims);
        }
        public IActionResult ReImClaimDetais(string id)
        {
            string Nid = TempData["NationalId"].ToString();
            TempData.Keep("NationalId");
            ViewBag.lang = langcode;
            var clamDetails = new ReClaimsDetails();
            try
            {
                clamDetails.RequestCreateDTO = GetReimClaimDetailsById(id);
                clamDetails.registration = _customerService.GetCustomerById(Nid);
                //clamDetails._claimstypes = GetClaimTypesViewModel();
                //clamDetails._bankNames = GetBankViewModel();
                if(clamDetails.RequestCreateDTO != null)
                    ViewBag.RequestStatus = clamDetails.RequestCreateDTO.RequestStatusName;
                //ViewBag.RequestStatus = GetRequestStatus(clamDetails.RequestCreateDTO.RequestStatusLogList);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::ReImClaimDetails::Exception::" + ex.Message);
            }
            return View(clamDetails);
        }

        private string GetRequestStatus(List<RequestStatusLogList> requestStatusLogList)
        {
            string status = "";
            var requestlist = requestStatusLogList.OrderByDescending(c => c.CreateDate).FirstOrDefault();
            status = requestlist.RequestStatusName.ToString();
            return status;
        }

        public IActionResult AddClaim()
        {
            string Nid = TempData["NationalId"].ToString();
            TempData.Keep("NationalId");
            ViewBag.lang = langcode;
            var userdetails = _customerService.GetCustomerById(Nid);
            //var userdetails = CustomerHandler.GetCustomerById(Nid);
            ViewBag.MemberName = userdetails.MemberName;
            var _viewm = new AddClaimViewModel();
            //_viewm._claimstypes = GetClaimTypesViewModel();
            _viewm._bankNames = GetBankViewModel();
            return View(_viewm);
        }
        [HttpPost]
        public IActionResult AddClaim(AddClaimViewModel createDTO)
        {
            List<IFormFile> uploadedFiles = createDTO.FilesUploaded;
            if (!ModelState.IsValid)
            {
                ViewBag.lang = langcode;
                createDTO._claimstypes = GetClaimTypesViewModel();
                createDTO._bankNames = GetBankViewModel();
                createDTO.FilesUploaded = uploadedFiles;
                return View(createDTO);

            }
            else
            {
                ViewBag.lang = langcode;
                var res = AddClaimRequest(createDTO);
                if (res == "true")
                {
                    HttpContext.Session.SetString(SessionShowDialog, "true");
                }
                else
                {
                    HttpContext.Session.SetString(SessionShowDialog, "false");
                }

                return RedirectToAction("ReimbursmentClaims", "Claims", new { lang = "en" });
            }


        }

        public IActionResult GetOSClaims()
        {
            ViewBag.lang = langcode;
            var _osClaims = GetOSClaimsById();
            var claimsResponse = new ClaimsResponse();
            if(_osClaims != null)
            claimsResponse.OSClaims = _osClaims.OrderByDescending(c => c.POLICY_INC).ToList();
            return View(claimsResponse);
        }
        public List<PaidClaims> GetPaidClaimsById()
        {
            return GetPaidClaimsByNationalId().Result;
        }
        public async Task<List<PaidClaims>> GetPaidClaimsByNationalId()
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<PaidClaims>();
            try
            {
                if (nationalId != null && yob != null)
                {
                    var clsInput = new ClsInput();
                    clsInput.code = "";
                    clsInput.nationalID = nationalId;
                    DateTime date = DateTime.ParseExact(yob, "dd-MM-yyyy", null);
                    clsInput.yearOfBirth = date.Year.ToString();
                    clsInput.insPolicyNo = "";
                    result = await _claimsHandler.GetPaidClaimsByNationalId(clsInput);

                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetPaidClaimsByNationalId:: " + ex.Message);
            }
            return result;
        }
        public List<OSClaims> GetOSClaimsById()
        {
            return GetOSClaimsByNationalId().Result;
        }
        public async Task<List<OSClaims>> GetOSClaimsByNationalId()
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<OSClaims>();
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
                    result = await _claimsHandler.GetOSClaimsByNationalId(clsInput);

                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetOSClaimsByNationalId:: " + ex.Message);
            }
            return result;
        }

        public List<ReImClaims> GetReimClaimsByClientId()
        {
            return GetReClaimsByClientId().Result;
        }
        public async Task<List<ReImClaims>> GetReClaimsByClientId()
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new List<ReImClaims>();
            try
            {
                if (nationalId != null)
                {
                    result = await _claimsHandler.GetReImClaimsByClientId(nationalId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetReClaimsByClientId:: " + ex.Message);
            }
            return result;
        }
        public RequestCreateDTO GetReimClaimDetailsById(string id)
        {
            return GetReImDetailsById(id).Result;
        }
        public async Task<RequestCreateDTO> GetReImDetailsById(string id)
        {
            string nationalId = TempData["NationalId"].ToString();
            string yob = TempData["YOB"].ToString();
            TempData.Keep("YOB");
            TempData.Keep("NationalId");
            var result = new RequestCreateDTO();
            try
            {

                if (nationalId != null)
                {
                    result = await _claimsHandler.GetReImClaimDetailsById(id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetReImDetailsById:: " + ex.Message);
            }
            return result;
        }

        public List<MRClaimType> GetClaimTypes()
        {
            return GetAllClaimTypes().Result;
        }
        public async Task<List<MRClaimType>> GetAllClaimTypes()
        {

            return await _claimsHandler.GetClaimsTypes();

        }
        public List<BankMaster> GetBankMasters()
        {
            return GetAllBanks().Result;
        }
        public async Task<List<BankMaster>> GetAllBanks()
        {

            return await _claimsHandler.GetBankNames();

        }

        private List<SelectListItem> GetClaimTypesViewModel()
        {
            List<SelectListItem> claimtypes = null;
            var clmtypes = _customerService.GetClaimTypes();
            claimtypes = clmtypes.ConvertAll(a =>
            {
                return new SelectListItem()
                {
                    Text = a.ClaimTypeName.ToString(),
                    Value = a.ClaimTypeName.ToString(),
                    Selected = false
                };
            });

            return claimtypes;
        }

        private List<SelectListItem> GetBankViewModel()
        {
            List<SelectListItem> banks = null;

            banks = GetBankMasters().ConvertAll(a =>
            {
                return new SelectListItem()
                {
                    Text = a.BankNameEnglish.ToString(),
                    Value = a.BankNameEnglish.ToString(),
                    Selected = false
                };
            });

            return banks;
        }


        public string AddClaimRequest(AddClaimViewModel _clm)
        {
            return AddClaimReq(_clm).Result;
        }
        public async Task<string> AddClaimReq(AddClaimViewModel _clm)
        {
            try
            {
                var loggedInUser = _authenticationService.GetAuthenticatedUser();
                //string nationalId = TempData["NationalId"].ToString();
                _clm.ClientDTO.IDNumber = loggedInUser.Iqama_NationalID;
                _clm.NationalId = loggedInUser.Iqama_NationalID;
                return await _claimsHandler.AddClaimRequest(_clm);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::AddClaimReq::Exception::" + ex.Message);
            }
            return "false";

        }
        [HttpPost]
        public IActionResult UpdateClaim(ReClaimsDetails _claimdetails)
        {
            var res = UpdateClaimRequest(_claimdetails);
            if (res.ToUpper() == "TRUE")
            {
                if (res == "true")
                {
                    HttpContext.Session.SetString(SessionShowDialog, "updatesuccess");
                }
                else
                {
                    HttpContext.Session.SetString(SessionShowDialog, "updatefailed");
                }
                return RedirectToAction("ReimbursmentClaims", "Claims", new { lang = "en" });
            }
            else
            {
                return View();
            }

        }

        public string UpdateClaimRequest(ReClaimsDetails _clm)
        {
            return UpdateClaimReq(_clm).Result;
        }
        public async Task<string> UpdateClaimReq(ReClaimsDetails _clm)
        {
            return await _claimsHandler.UpdateClaimRequest(_clm);
        }

        public IActionResult ListView()
        {
            return View();
        }
        public IActionResult ClaimList()
        {
            try
            {
                var formCollection = Request.Form;

                var searchModel = new ClaimSearchModel();

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
                var model = _listFactory.PrepareClaimListModel(searchModel);
                return Json(model);
            }
            catch (Exception ex)
            {

            }
            return Json(new ClaimListModel());
        }
        public IActionResult ReimbursmentListView()
        {
            return View();
        }
        public IActionResult ReimbursmentList()
        {
            try
            {
                var formCollection = Request.Form;

                var searchModel = new ReimbursmentSearchModel();

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
                var model = _listFactory.PrepareReimbursmentClaimListModel(searchModel);
                return Json(model);
            }
            catch (Exception ex)
            {

            }
            return Json(new ReimbursmentClaimListModel());
        }
    }
}
