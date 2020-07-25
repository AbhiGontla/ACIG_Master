using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ACIGConsumer.Models;
using Core;
using Core.Api;
using Core.Domain;
using Core.Sms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Services.Interfaces;


namespace ACIG_Services.Controllers
{
    [Route("api")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly string _className = "HomeController";
        private readonly ILogger<HomeController> _logger;
        private ICustomerService _customerService;
        private readonly IFileService _fileService;
        private readonly IOptions<ApplConfig> appSettings;
        private static string UploadedFilepath;
        public HomeController(IFileService fileService, ILogger<HomeController> logger, IOptions<ApplConfig> _config, ICustomerService customerService)
        {
            _logger = logger;
            _fileService = fileService;
            appSettings = _config;
            _customerService = customerService;
        }

        [Route("GetApprovals")]
        [HttpPost]
        public async Task<GetApprovalsResponse> GetApprovals(ClsInput clsInput)
        {
            List<Approvals> _approvals = null;
            GetApprovalsResponse res = null;
            if(clsInput.nationalID == "9294436588")
            {
                clsInput.nationalID = "2129246654";
                clsInput.yearOfBirth = "1961";
            }
            try
            {

                //check whether is user approvals in db or not
                _approvals = _customerService.GetApprovalsByNationalId(clsInput.nationalID);
                if (_approvals.Count > 0)
                {
                    res = new GetApprovalsResponse();
                    res.responseCode = "Success";
                    res.responseData = _approvals;
                    res.responseMessage = "User Approvals From Table";
                    return res;
                }
                else
                {
                    string url = appSettings.Value.Urls.GetApprovals;
                    string username = appSettings.Value.BasicAuth.Username;
                    string pass = appSettings.Value.BasicAuth.Password;

                    HttpMessageHandler handler = new HttpClientHandler();

                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(url),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                    //This is the key section you were missing    
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                    //Getting the input paramters as json 
                    string content = GetJson(clsInput.nationalID, clsInput.yearOfBirth, clsInput.insPolicyNo);
                    _logger.LogInformation(_className + "::GetApprovals::InputJson::" + content);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                    _logger.LogInformation(_className + "::GetApprovals::STATUS::" + response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        res = JsonConvert.DeserializeObject<GetApprovalsResponse>(response.Content.ReadAsStringAsync().Result);
                        try
                        {
                            if(res != null && res.responseData != null)
                            {
                                _approvals = res.responseData;
                                var result = GetApprovData(_approvals, clsInput);
                                _customerService.Insert(result);
                            }
                            else
                            {
                                _logger.LogInformation(_className + "::GetApprovals::Response::NULL::" + res.responseMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation(_className + "::GetApprovals::Response::" + ex.Message);
                        }

                    }
                    else
                    {
                        res.responseCode = response.StatusCode.ToString();
                        return res;
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new GetApprovalsResponse() { responseCode = "500", responseMessage = "Something went wrong",responseData = new List<Approvals>() };
                _logger.LogInformation(_className + "::GetApprovals::Exception" + ex.Message);
            }
            return res;
        }

        #region GetInputJson    
        public string GetJson(string nationalId, string YOB, string insPolicyno)
        {
            string clientSecret = "{\r\n\"code\":\"CI\",\r\n\"nationalID\": \"" + nationalId + "\",\r\n\"yearOfBirth\": \"" + YOB + "\",\r\n\"insPolicyNo\": \"" + insPolicyno + "\"\r\n}\r\n";
            return clientSecret;
        }
        #endregion


        [Route("GetCustomers")]
        [HttpGet]
        public Registration GetCustomers()
        {
            var cust = _customerService.GetCustomerById("2332978820");
            return cust;
        }
        [Route("GetCustomerById")]
        [HttpGet]
        public Registration GetCustomerById(string nationalId,string dob)
        {
            try
            {
                var customer = _customerService.GetRegistrationByNationalId(nationalId, dob);
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetCustomerById::" + ex.Message);
            }
            return null;
        }
        private static List<Approvals> GetApprovData(List<Approvals> approvals, ClsInput clsInput)
        {
            List<Approvals> _app;
            try
            {
                _app = approvals;
                foreach (var i in _app)
                {
                    i.Code = clsInput.code;
                    i.NationalId = clsInput.nationalID;
                    i.YearofBirth = clsInput.yearOfBirth;
                    i.InsPolicyNo = clsInput.insPolicyNo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _app;
        }

        [Route("GetPolicies")]
        [HttpPost]
        public PolicyResponse GetPoicies(ClsInput clsInput)
        {
            List<Policies> _policies = null;
            PolicyResponse res = null;
            if (clsInput.nationalID == "9294436588")
            {
                clsInput.nationalID = "2129246654";
                clsInput.yearOfBirth = "1961";
            }
            //check whether is user approvals in db or not
            try
            {

                _policies = _customerService.GetPoiciesByNationalId(clsInput.nationalID);
                if (_policies.Count > 0)
                {
                    res = new PolicyResponse();
                    res.responseCode = "Success";
                    res.responseData = _policies;
                    res.responseMessage = "User Policies From Table";
                    return res;
                }
                else
                {
                    res = new PolicyResponse();
                    res.responseCode = "Failes";
                    res.responseData = new List<Policies>();
                    res.responseMessage = "No Policies found.";
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new PolicyResponse() { responseCode = "500", responseMessage = "Something went wrong!", responseData = new List<Policies>() };
                _logger.LogInformation(_className + "::GetPolicies::" + ex.Message);
            }
            return res;
        }



        [Route("GetCoverageBalances")]
        [HttpPost]
        public async Task<CoverageBalanceResponse> GetCoverageBalances(ClsInput clsInput)
        {
            List<CoverageBalance> coverageBalances = null;
            CoverageBalanceResponse res = null;
            if (clsInput.nationalID == "9294436588")
            {
                clsInput.nationalID = "1047561319";
                clsInput.yearOfBirth = "1981";
            }
            try
            {
                //check whether is user covragebalances in db or not
                coverageBalances = _customerService.GetCovBalsByNationalId(clsInput.nationalID);
                if (coverageBalances.Count > 0)
                {
                    res = new CoverageBalanceResponse();
                    res.responseCode = "Success";
                    res.responseData = coverageBalances;
                    res.responseMessage = "User CoverageBalances From Table";
                    return res;
                }
                else
                {
                    string url = appSettings.Value.Urls.GetCoverageBalance;
                    string username = appSettings.Value.BasicAuth.Username;
                    string pass = appSettings.Value.BasicAuth.Password;

                    HttpMessageHandler handler = new HttpClientHandler();

                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(url),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                    //This is the key section you were missing    
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                    //Getting the input paramters as json 
                    string content = GetJson(clsInput.nationalID, clsInput.yearOfBirth, clsInput.insPolicyNo);
                    _logger.LogInformation(_className + "::GetCoverageBalances::Content::" + content);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                    _logger.LogInformation(_className + "::GetCoverageBalances::STATUS::" + response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        res = JsonConvert.DeserializeObject<CoverageBalanceResponse>(response.Content.ReadAsStringAsync().Result);
                      
                        try
                        {
                            if(res != null && res.responseData != null)
                            {
                                coverageBalances = res.responseData;
                                var result = GetCovData(coverageBalances, clsInput);
                                _customerService.Insert(result);
                            }
                            else
                            {
                                _logger.LogInformation(_className + "::GetCoverageBalances::ResponseData::NULL::REsponseMsg::" + res.responseMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation(_className + "::GetCoverageBalances::ResponseData::Exception" + ex.Message);
                        }

                    }
                    else
                    {
                        res.responseCode = response.StatusCode.ToString();
                        return res;
                    }
                    return res;
                }

            }
            catch (Exception ex)
            {
                res = new CoverageBalanceResponse() { responseCode = "500", responseMessage = "Something went wrong", responseData = new List<CoverageBalance>() };
                _logger.LogInformation(_className + "::GetCoverageBalances::" + ex.Message);
            }
            return res;
        }
        private static List<CoverageBalance> GetCovData(List<CoverageBalance> coverageBalances, ClsInput clsInput)
        {
            List<CoverageBalance> _cov;
            try
            {
                _cov = coverageBalances;
                foreach (var i in _cov)
                {
                    i.NationalID = clsInput.nationalID;
                    i.YearofBirth = clsInput.yearOfBirth;
                    i.InsPolicyNo = clsInput.insPolicyNo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return _cov;
        }


        [Route("GetProvidersList")]
        [HttpPost]
        public async Task<ProvidersResponse> GetProviders(ClsInput clsInput)
        {
            List<Providers> providers = null;
            ProvidersResponse res = null;
            if (clsInput.nationalID == "9294436588")
            {
                clsInput.nationalID = "2332978820";
                clsInput.yearOfBirth = "1988";
            }
            try
            {
                //check whether is user covragebalances in db or not
                providers = _customerService.GetProvidersByNationalId(clsInput.nationalID);
                if (providers.Count > 0)
                {
                    res = new ProvidersResponse();
                    res.responseCode = "Success";
                    res.responseData = providers;
                    res.responseMessage = "User Providers From Table";
                    return res;
                }
                else
                {
                    string url = appSettings.Value.Urls.GetProvidersList;
                    string username = appSettings.Value.BasicAuth.Username;
                    string pass = appSettings.Value.BasicAuth.Password;

                    HttpMessageHandler handler = new HttpClientHandler();

                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(url),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                    //This is the key section you were missing    
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                    //Getting the input paramters as json 
                    string content = GetJson(clsInput.nationalID, clsInput.yearOfBirth, clsInput.insPolicyNo);
                    _logger.LogInformation(_className + ":: GetProviders::Content" + content);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                    _logger.LogInformation(_className + ":: GetProviders::STAUS::" + response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        res = JsonConvert.DeserializeObject<ProvidersResponse>(response.Content.ReadAsStringAsync().Result);
                       
                        try
                        {
                            if(res != null && res.responseData != null)
                            {
                                providers = res.responseData;
                                var result = GetProvData(providers, clsInput);
                                _customerService.Insert(result);
                            }
                            else
                            {
                                _logger.LogInformation(_className + ":: GetProviders::ResponseData::NULL::" + res.responseMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation(_className + ":: GetProviders::ResponseData::Exception" +ex.Message);
                        }

                    }
                    else
                    {
                        res.responseCode = response.StatusCode.ToString();
                        return res;
                    }
                    return res;
                }

            }
            catch (Exception ex)
            {
                res = new ProvidersResponse() { responseCode = "500", responseMessage = "Something went wrong!", responseData = new List<Providers>() };
                _logger.LogInformation(_className + ":: GetProviders ::" + ex.Message);
            }
            return res;
        }
        private static List<Providers> GetProvData(List<Providers> Providers, ClsInput clsInput)
        {
            List<Providers> _prov;
            try
            {
                _prov = Providers;
                foreach (var i in _prov)
                {
                    i.Code = clsInput.code;
                    i.NationalID = clsInput.nationalID;
                    i.YearofBirth = clsInput.yearOfBirth;
                    i.InsPolicyNo = clsInput.insPolicyNo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return _prov;
        }

        [Route("GetPaidClaims")]
        [HttpPost]
        public async Task<PaidClaimsResponse> GetPaidClaims(ClsInput clsInput)
        {
            List<PaidClaims> paidClaims = null;
            PaidClaimsResponse res = null;
            if (clsInput.nationalID == "9294436588")
            {
                clsInput.nationalID = "2359237050";
                clsInput.yearOfBirth = "2013";
            }
            try
            {
                //check whether is user covragebalances in db or not
                paidClaims = _customerService.GetPaidClaimsByNationalId(clsInput.nationalID);
                if (paidClaims.Count > 0)
                {
                    res = new PaidClaimsResponse();
                    res.responseCode = "Success";
                    res.responseData = paidClaims;
                    res.responseMessage = "User paidClaims From Table";
                    return res;
                }
                else
                {
                    string url = appSettings.Value.Urls.GetPaidClaims;
                    string username = appSettings.Value.BasicAuth.Username;
                    string pass = appSettings.Value.BasicAuth.Password;

                    HttpMessageHandler handler = new HttpClientHandler();

                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(url),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                    //This is the key section you were missing    
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                    //Getting the input paramters as json 
                    string content = GetJson(clsInput.nationalID, clsInput.yearOfBirth, clsInput.insPolicyNo);
                    _logger.LogInformation(_className + "::GetPaidClaims::Content::" + content);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                    _logger.LogInformation(_className + "::GetPaidClaims::Response::STATUS" + response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        res = JsonConvert.DeserializeObject<PaidClaimsResponse>(response.Content.ReadAsStringAsync().Result);
                        try
                        {
                            if (res != null && res.responseData != null)
                            {
                                paidClaims = res.responseData;
                                var result = GetPaidClaimsData(paidClaims, clsInput);
                                _customerService.Insert(result);
                            }
                            else
                            {
                                _logger.LogInformation(_className + "::GetPaidClaims::ResponseData:message::" + res.responseMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation(_className + "::GetPaidClaims::ResponseData:Exception::" + ex.Message);
                        }

                    }
                    else
                    {
                        res.responseCode = response.StatusCode.ToString();
                        return res;
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new PaidClaimsResponse() { responseCode = "500", responseMessage = "Something went wrong!", responseData = new List<PaidClaims>() };
                _logger.LogInformation(_className + "::GetPaidClaims::" + ex.Message);
            }
            return res;
        }
        private static List<PaidClaims> GetPaidClaimsData(List<PaidClaims> paidclaims, ClsInput clsInput)
        {
            List<PaidClaims> _prov;
            try
            {
                _prov = paidclaims;
                foreach (var i in _prov)
                {
                    i.Code = clsInput.code;
                    i.NationalID = clsInput.nationalID;
                    i.YearofBirth = clsInput.yearOfBirth;
                    i.InsPolicyNo = clsInput.insPolicyNo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return _prov;
        }

        [Route("GetOSClaims")]
        [HttpPost]
        public async Task<OSClaimsResponse> GetOSClaims(ClsInput clsInput)
        {
            List<OSClaims> osClaims = null;
            OSClaimsResponse res = null;
            if (clsInput.nationalID == "9294436588")
            {
                clsInput.nationalID = "2326837727";
                clsInput.yearOfBirth = "2010";
            }
            try
            {
                //check whether is user covragebalances in db or not
                osClaims = _customerService.GetOSClaimsByNationalId(clsInput.nationalID);
                if (osClaims.Count > 0)
                {
                    res = new OSClaimsResponse();
                    res.responseCode = "Success";
                    res.responseData = osClaims;
                    res.responseMessage = "User osclaims From Table";
                    return res;
                }
                else
                {
                    string url = appSettings.Value.Urls.GetOSClaims;
                    string username = appSettings.Value.BasicAuth.Username;
                    string pass = appSettings.Value.BasicAuth.Password;

                    HttpMessageHandler handler = new HttpClientHandler();

                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(url),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                    //This is the key section you were missing    
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                    //Getting the input paramters as json 
                    string content = GetJson(clsInput.nationalID, clsInput.yearOfBirth, clsInput.insPolicyNo);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        res = JsonConvert.DeserializeObject<OSClaimsResponse>(response.Content.ReadAsStringAsync().Result);
                        if(res.responseData == null || res.responseData.Count ==0 )
                        {
                            res = new OSClaimsResponse() { responseCode = "Success", responseMessage = "No Os Claims found.", responseData = new List<OSClaims>() };
                        }
                        osClaims = res.responseData;
                        var result = GetOSClaimsData(osClaims, clsInput);
                        try
                        {
                            _customerService.Insert(result);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                    }
                    else
                    {
                        res.responseCode = response.StatusCode.ToString();
                        return res;
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new OSClaimsResponse() { responseCode = "500", responseMessage = "Somthing went wrong!", responseData = new List<OSClaims>() };
                _logger.LogInformation(_className + "::GetOsClaims::" + ex.Message);
            }
            return res;
            
        }
        private static List<OSClaims> GetOSClaimsData(List<OSClaims> oSClaims, ClsInput clsInput)
        {
            List<OSClaims> _prov;
            try
            {
                _prov = oSClaims;
                foreach (var i in _prov)
                {
                    i.Code = clsInput.code;
                    i.NationalID = clsInput.nationalID;
                    i.YearofBirth = clsInput.yearOfBirth;
                    i.InsPolicyNo = clsInput.insPolicyNo;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return _prov;
        }

        //[Route("GetReimbursmentClaims")]
        //[HttpPost]
        //public async Task<List<RequestCreateDTO>> GetReimbursmentClaims(ClsInput clsInput)
        //{
        //    List<RequestCreateDTO> requestCreateDTOs = null;
        //    ReimbursmentResponse res = null;           
        //    requestCreateDTOs = _customerService.GetreimClaimsByNationalId(clsInput.nationalID);
        //    if (requestCreateDTOs.Count > 0)
        //    {              

        //        return requestCreateDTOs;
        //    }
        //    else
        //    {
        //        HttpMessageHandler handler = new HttpClientHandler();
        //        string url = appSettings.Value.Urls.GetReimbursmentClaims;
        //        string cpath = url + clsInput.nationalID;
        //        var httpClient = new HttpClient(handler)
        //        {
        //            BaseAddress = new Uri(cpath),
        //            Timeout = new TimeSpan(0, 2, 0)
        //        };
        //        httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
        //        HttpResponseMessage response = await httpClient.GetAsync(cpath);
        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            requestCreateDTOs = JsonConvert.DeserializeObject<RequestCreateDTO[]>(response.Content.ReadAsStringAsync().Result).ToList();

        //            try
        //            {
        //                _customerService.Insert(requestCreateDTOs);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        else
        //        {
        //            requestCreateDTOs = null;
        //            return requestCreateDTOs;
        //        }
        //    }

        //    return requestCreateDTOs;
        //}
        //[Route("GetReimbursmentClaimsById/id/{id}")]
        //[HttpGet]
        //public async Task<RequestCreateDTO> GetReimbursmentClaimsById(string id)
        //{
        //    RequestCreateDTO requestCreateDTOs = null;
        //    HttpMessageHandler handler = new HttpClientHandler();
        //    string url = appSettings.Value.Urls.GetReimbursmentDetails;
        //    string cpath = url + id;
        //    var httpClient = new HttpClient(handler)
        //    {
        //        BaseAddress = new Uri(cpath),
        //        Timeout = new TimeSpan(0, 2, 0)
        //    };
        //    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
        //    HttpResponseMessage response = await httpClient.GetAsync(cpath);
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        requestCreateDTOs = JsonConvert.DeserializeObject<RequestCreateDTO>(response.Content.ReadAsStringAsync().Result);

        //    }
        //    return requestCreateDTOs;
        //}

        [Route("AddClaimRequest")]
        [HttpPost]
        public async Task<string> InsertClaimRequest(RequestCreateDTO _claimdetails)
        {
            string Status = "false";
            string result;
            try
            {
                string url = appSettings.Value.Urls.AddReimbursmentClaims;
                HttpMessageHandler handler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(url),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                //var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                //string val = System.Convert.ToBase64String(plainTextBytes);
                //httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                //Getting the input paramters as json 
                var content = JsonConvert.SerializeObject(_claimdetails);

                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    result = null;
                }

                if (result != null)
                {
                    try
                    {
                        MRClient _list = _customerService.GetClientByNationalId(_claimdetails.NationalId);

                        if (_list == null)
                        {
                            MRClient _clientdet = new MRClient();
                            _clientdet.IDNumber = _claimdetails.NationalId;
                            _clientdet.BankName = _claimdetails.ClientDTO.BankName;
                            _clientdet.ClientName = _claimdetails.ClientDTO.ClientName;
                            _clientdet.Email = _claimdetails.ClientDTO.Email;
                            if (_claimdetails.ClientDTO.GenderName == null)
                            {
                                _clientdet.GenderId = null;
                            }
                            else if (_claimdetails.ClientDTO.GenderName.ToUpper() == "MALE")
                            {
                                _clientdet.GenderId = 1;
                            }
                            else if (_claimdetails.ClientDTO.GenderName.ToUpper() == "FEMALE")
                            {
                                _clientdet.GenderId = 2;
                            }
                            _clientdet.IBANNumber = _claimdetails.ClientDTO.IBANNumber;
                            _clientdet.MobileNumber = _claimdetails.ClientDTO.MobileNumber;

                            _customerService.Insert(_clientdet);
                        }

                        var _climdet = new MRRequest();
                        _climdet.ActualAmount = _claimdetails.ActualAmount;
                        _climdet.CardExpireDate = _claimdetails.CardExpireDate;
                        _climdet.CardNumber = _claimdetails.CardNumber;
                        _climdet.ClaimTypeName = _claimdetails.ClaimTypeName;
                        _climdet.RequestNumber = result;
                        _climdet.ClientId = _list.Id;
                        _climdet.ExpectedAmount = _claimdetails.ExpectedAmount;
                        _climdet.HolderName = _claimdetails.HolderName;
                        _climdet.MemberID = _claimdetails.MemberID;
                        _climdet.MemberName = _claimdetails.MemberName;
                        _climdet.PolicyNumber = _claimdetails.PolicyNumber;
                        _climdet.RelationName = _claimdetails.RelationName;
                        _climdet.RequestDate = _claimdetails.RequestDate;
                        _climdet.RequestStatusId = 1;
                        _climdet.TransferDate = _claimdetails.TransferDate;
                        _climdet.VATAmount = _claimdetails.VATAmount;

                        _customerService.Insert(_climdet);
                        Status = "true";

                    }
                    catch (Exception ex)
                    {
                        Status = "false";
                        throw ex;
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::InsertClaimRequest::" + ex.Message);
            }
            return Status;
        }

        [Route("GetClaimsByClientId/ClientId/{id}")]
        [HttpGet]
        public async Task<List<ReImClaims>> GetClaimsByClientId(string id)
        {
            List<ReImClaims> reImClaims = null;
            try
            {

                HttpMessageHandler handler = new HttpClientHandler();
                string url = appSettings.Value.Urls.GetReimbursmentClaims;
                string cpath = url + id;
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(cpath),
                    Timeout = new TimeSpan(0, 2, 0)
                };
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
                HttpResponseMessage response = await httpClient.GetAsync(cpath);
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    var a = JsonConvert.DeserializeObject<List<ReImClaims>>(response.Content.ReadAsStringAsync().Result);
                    reImClaims = a;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetClaimsByClientId::" + ex.Message);
                reImClaims = new List<ReImClaims>();
            }
            return reImClaims;
        }


        [Route("GetClaimDetailsById/Id/{id}")]
        [HttpGet]
        public async Task<RequestCreateDTO> GetClaimDetailsById(string id)
        {
            RequestCreateDTO reImClaimdetails = null;
            try
            {

                HttpMessageHandler handler = new HttpClientHandler();
                string url = appSettings.Value.Urls.GetReimbursmentDetails;
                string cpath = url + id;
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(cpath),
                    Timeout = new TimeSpan(0, 2, 0)
                };
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
                HttpResponseMessage response = await httpClient.GetAsync(cpath);
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    var a = JsonConvert.DeserializeObject<RequestCreateDTO>(response.Content.ReadAsStringAsync().Result);
                    reImClaimdetails = a;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetClaimDetailsById::" + ex.Message);
            }
            return reImClaimdetails;
        }

        #region LoginAPI

        public IActionResult sendsms()
        {
            var otp = GenerateRandomNo();
            string url = appSettings.Value.SmsConfig.url;
            string uname = appSettings.Value.SmsConfig.userName;
            string pwd = appSettings.Value.SmsConfig.password;
            string sender = appSettings.Value.SmsConfig.senderName;
            string mobilenumber = "966508095931";
            var request = (HttpWebRequest)WebRequest.Create(url);
            var postData =
            "UserName=" + uname + "&Password=" + pwd + "&MessageType=text&Recipients=" + mobilenumber + "&SenderName=" + sender + "&MessageText = " + otp + "";
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (response.StatusCode.ToString() == "OK")
            {
                return new JsonResult(new { success = true, responseText = "Sending OTP Success.", sentotp = otp });

            }
            else
            {
                return new JsonResult(new { success = false, responseText = "Sending OTP Failed." });
            }
        }

        //Generate RandomNo
        public int GenerateRandomNo()
        {
            Random _rdm = new Random();
            int _min = 1000;
            int _max = 9999;
            return _rdm.Next(_min, _max);
        }
        [Route("Login/ValidateUser")]
        [HttpGet]
        public IActionResult ValidateUser(string nid, string pin)
        {
            try
            {
                Registration Item = _customerService.ValidateCustomer(nid, pin);

                //second request, get value marking it from deletion

                //later on decide to keep it


                if (Item == null)
                {
                    return new JsonResult(new { success = false, responseText = "Login Failed." });
                }
                else
                {
                    return new JsonResult(new { success = true, responseText = "Login Success.", response = Item });
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::ValidateUser::" + ex.Message);
                return new JsonResult(new { exception = ex.Message });
            }

        }
        [Route("Login/RegisterUser")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterModel model)
        {
            string status = "false";
            Registration _userdetails = new Registration();
            
            try
            {
                var isUserExists = _customerService.CheckIfUserExists(model.nid);
                if (!isUserExists)
                {
                    ClsInput input = new ClsInput() { nationalID = model.nid, yearOfBirth = model.dob };
                    var registrationResponse = await GetMemberByNationalId(input);
                    _logger.LogInformation(_className + "::RegisterUser::Response::" + registrationResponse.ResponseMessage);
                    if (registrationResponse != null && registrationResponse.Members != null && registrationResponse.Members.Count > 0)
                    {
                        _logger.LogInformation(_className + "::RegisterUser::Response::Members found::" + registrationResponse.Members.Count);
                        var EmpMember = registrationResponse.Members.Where(c => c.MemberType.ToLower() == "employee").FirstOrDefault();
                        EmpMember.Iqama_NationalID = model.nid;
                        EmpMember.DOB = model.dob;
                        EmpMember.CreatePin = model.enterpin;
                        EmpMember.ConfirmPin = model.enterpin;
                        _customerService.Insert(EmpMember);
                        _userdetails = EmpMember;
                        var members = registrationResponse.Members.FindAll(c => c.MemberType.ToLower() != "employee");
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
                                relation.Iqama_NationalID = model.nid;
                                relation.MemberMobileNumber = member.MemberMobileNo;
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
                                relation.Iqama_NationalID = model.nid;
                                relations.Add(relation);
                            }
                            foreach (var rel in relations)
                            {
                                _customerService.InsertRelations(rel);
                            }
                        }
                        status = "true";
                    }
                    else
                    {
                        return new JsonResult(new { success = false, responseText = "No members found." });
                    }
                }
                else
                {
                    return new JsonResult(new { success = false, responseText = "User Already Exists." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::RegisterUser::" + ex.Message);
                return new JsonResult(new { Exception = ex.Message });
            }
            if (status == "false")
            {
                return new JsonResult(new { success = false, responseText = "User Registration Failed." });
            }
            else
            {
                return new JsonResult(new { success = true, responseText = "User Registration Success.", response = _userdetails });
            }

        }

        #endregion
        [Route("GetBanners")]
        [HttpGet]
        public IActionResult GetBanners()
        {
            try
            {
                var banners = _fileService.GetBanners();
                return new JsonResult(new { Success = true, Data = banners.ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetBanners::" + ex.Message);
            }
            return new JsonResult(new { Success = false, Data = new object[0] });
        }


        [Route("GetClaimsTypes")]
        [HttpGet]
        public List<MRClaimType> GetClaimsTypes()
        {
            try
            {
                return _customerService.GetClaimTypes();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetClaimsTypes::" + ex.Message);
            }
            return new List<MRClaimType>();
        }

        [Route("GetBankNames")]
        [HttpGet]
        public List<BankMaster> GetBankNames()
        {
            try
            {
                return _customerService.GetBankNames();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetBankNames::" + ex.Message);
            }
            return new List<BankMaster>();
        }

        [Route("SendSms")]
        [HttpPost]
        public ResponseSMS SendSms(ClsSms clsSms)
        {

            int otp;
            string response = "";
            var responseSMS = new ResponseSMS();
            try
            {
                otp = GenerateRandomNo();
                string url = appSettings.Value.SmsConfig.url;
                string uname = appSettings.Value.SmsConfig.userName;
                string pwd = appSettings.Value.SmsConfig.password;
                string sender = appSettings.Value.SmsConfig.senderName;
                string mobilenumber = "966508095931";
                //string mobilenumber = clsSms.MobileNumber; ;
                string message = "Dear Customer,Your One Time Password(OTP):" + otp;
                SmsRequest request = new SmsRequest();
                response = request.SmsHandler(mobilenumber, message);

                if (response.ToString() == "Success")
                {
                    
                    responseSMS.RequestStatus = response;
                    responseSMS.ResponseText = "OTP Sent Successfully";
                    responseSMS.OTPSent = otp.ToString();
                    return responseSMS;
                }
                else
                {
                    responseSMS.RequestStatus = response;
                    responseSMS.ResponseText = "OTP Sent Failed";
                    responseSMS.OTPSent = null;
                    return responseSMS;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::sendsms::" + ex.Message);
            }
            responseSMS.RequestStatus = response;
            responseSMS.ResponseText = "OTP Sent Failed";
            responseSMS.OTPSent = null;
            return responseSMS;
        }

        [Route("GetAllUsers")]
        [HttpGet]
        public List<Registration> GetAllUsers()
        {
            try
            {
                return _customerService.GetAllCustomers();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetAllUsers::" + ex.Message);
            }

            return new List<Registration>();
        }


        [Route("UpdateClaim")]
        [HttpPost]
        public async Task<string> UpdateClaimRequest(UpdateClaimRequest updateClaim)
        {
            string status = "false";
            try
            {
                string url = appSettings.Value.Urls.UpdateClaimRequest;
                HttpMessageHandler handler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(url),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                //var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                //string val = System.Convert.ToBase64String(plainTextBytes);
                //httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                //Getting the input paramters as json 
                var content = JsonConvert.SerializeObject(updateClaim);

                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var st = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                    status = st;
                }
                else
                {
                    return status;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::UpdateClaimRequest::" + ex.Message);
            }
            return status;
        }
        [Route("GetCustomerById")]
        [HttpGet]
        public Registration GetCustomerById(string NationalId)
        {
            var cust = _customerService.GetCustomerById(NationalId);
            return cust;
        }
        #region TOB

        #region GetTOBs

        [Route("GetTOBs")]
        [HttpPost]
        public async Task<TOBResponse> GetTOBs(ClsInput clsInput)
        {

            TOB tOB = null;
            TOBResponse res = null;
            try
            {

                var customerDetails = GetCustomerById(clsInput.nationalID);
                tOB = _customerService.GetTOB(customerDetails.PolicyNo, customerDetails.ClassCode);
                if (tOB != null)
                {
                    _logger.LogInformation(_className + "::GetTOBs::tob Not NULL");
                    res = new TOBResponse();
                    var tobDetails = GetTOBData(tOB.ClassName);
                    tOB.TOBlist = tobDetails.TOBlist;
                    tOB.TOBsublist = tobDetails.TOBsublist;
                    res.responseCode = "Success";
                    res.responseData = tOB;
                    res.responseMessage = "User Policies From Table";
                    return res;
                }
                else
                {
                    res = await GetTOBResponse(customerDetails.PolicyNo, customerDetails.PolicyFromDate.ToString(), customerDetails.TushfaMemberNo);
                    if (res != null)
                    {
                        _logger.LogInformation(_className + "::GetTOBs::TOBResponse Not NULL");
                        InsertTOB(res);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_className + "::GetTOBs::" + ex.Message);
            }
            return res = new TOBResponse() { responseCode="Failed",responseMessage= "Something went wrong.", responseData = new TOB() };
        }
        #endregion

        #region InsertTOBinDB
        private bool InsertTOB(TOBResponse res)
        {
            bool status = false;
            try
            {
                if (res.responseData != null)
                {
                    TOB tob = new TOB();
                    tob.PolicyNo = res.responseData.PolicyNo;
                    tob.PolicyFromDate = res.responseData.PolicyFromDate;
                    tob.PolicyToDate = res.responseData.PolicyToDate;
                    tob.ClassCode = res.responseData.ClassCode;
                    tob.ClassName = res.responseData.ClassName;
                    tob.Network = res.responseData.Network;
                    _customerService.Insert(tob);

                    if (res.responseData.TOBlist.Count > 0)
                    {
                        for (int i = 0; i < res.responseData.TOBlist.Count; i++)
                        {
                            TOBlist toblist = new TOBlist();
                            toblist = res.responseData.TOBlist[i];
                            toblist.ClassName = res.responseData.ClassName;
                            _customerService.Insert(toblist);
                        }
                    }
                    if (res.responseData.TOBsublist.Count > 0)
                    {
                        for (int i = 0; i < res.responseData.TOBsublist.Count; i++)
                        {
                            if (res.responseData.TOBsublist[i].Inpatient.Count > 0)
                            {
                                for (int j = 0; j < res.responseData.TOBsublist[i].Inpatient.Count; j++)
                                {
                                    Inpatient inpatient = new Inpatient();
                                    inpatient = res.responseData.TOBsublist[j].Inpatient[j];
                                    inpatient.ClassName = res.responseData.ClassName;
                                    _customerService.Insert(inpatient);
                                }
                            }


                            if (res.responseData.TOBsublist[i].Outpatient.Count > 0)
                            {
                                for (int k = 0; k < res.responseData.TOBsublist[i].Outpatient.Count; k++)
                                {
                                    Outpatient outpatient = new Outpatient();
                                    outpatient = res.responseData.TOBsublist[k].Outpatient[k];
                                    outpatient.ClassName = res.responseData.ClassName;
                                    _customerService.Insert(outpatient);
                                }
                            }


                            if (res.responseData.TOBsublist[i].MaternityBenefits.Count > 0)
                            {
                                for (int j = 0; j < res.responseData.TOBsublist[i].MaternityBenefits.Count; j++)
                                {
                                    MaternityBenefit MaternityBenefit = new MaternityBenefit();
                                    MaternityBenefit = res.responseData.TOBsublist[j].MaternityBenefits[j];
                                    MaternityBenefit.ClassName = res.responseData.ClassName;
                                    _customerService.Insert(MaternityBenefit);
                                }
                            }


                            if (res.responseData.TOBsublist[i].Inpatient.Count > 0)
                            {
                                for (int j = 0; j < res.responseData.TOBsublist[i].DentalBenefits.Count; j++)
                                {
                                    DentalBenefit DentalBenefit = new DentalBenefit();
                                    DentalBenefit = res.responseData.TOBsublist[j].DentalBenefits[j];
                                    DentalBenefit.ClassName = res.responseData.ClassName;
                                    _customerService.Insert(DentalBenefit);
                                }
                            }


                            if (res.responseData.TOBsublist[i].ReimbursementClaims.Count > 0)
                            {
                                for (int j = 0; j < res.responseData.TOBsublist[i].ReimbursementClaims.Count; j++)
                                {
                                    ReimbursementClaim ReimbursementClaim = new ReimbursementClaim();
                                    ReimbursementClaim = res.responseData.TOBsublist[j].ReimbursementClaims[j];
                                    ReimbursementClaim.ClassName = res.responseData.ClassName;
                                    _customerService.Insert(ReimbursementClaim);
                                }
                            }


                            if (res.responseData.TOBsublist[i].AdditionalBenefits.Count > 0)
                            {
                                for (int j = 0; j < res.responseData.TOBsublist[i].AdditionalBenefits.Count; j++)
                                {
                                    AdditionalBenefit AdditionalBenefit = new AdditionalBenefit();
                                    AdditionalBenefit = res.responseData.TOBsublist[j].AdditionalBenefits[j];
                                    AdditionalBenefit.ClassName = res.responseData.ClassName;
                                    _customerService.Insert(AdditionalBenefit);
                                }
                            }

                        }
                    }

                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        #endregion

        #region GetTOBDetailsFromDB
        private TOB GetTOBData(string className)
        {
            TOB tOB = null;
            List<TOBlist> TOBlist = null;
            List<TOBsublist> TOBsub = null;
            if (className != null)
            {

                List<Inpatient> Inpatientlist;
                List<Outpatient> Outpatientlist;
                List<MaternityBenefit> MaternityBenefitslist;
                List<DentalBenefit> DentalBenefitslist;
                List<ReimbursementClaim> ReimbursementClaimslist;
                List<AdditionalBenefit> AdditionalBenefitslist;
                //assign values to TOB object
                tOB = new TOB();



                //gets the toblist data
                TOBlist = new List<TOBlist>();
                TOBlist = _customerService.GetTOBList(className);
                tOB.TOBlist = TOBlist;

                //gets the tobsublist data
                TOBsub = new List<TOBsublist>();
                TOBsublist sublist = new TOBsublist();
                AdditionalBenefitslist = _customerService.GetAdditionalBenefitList(className);
                sublist.AdditionalBenefits = AdditionalBenefitslist;
                DentalBenefitslist = _customerService.GetDentalBenefitList(className);
                sublist.DentalBenefits = DentalBenefitslist;
                Inpatientlist = _customerService.GetInpatientList(className);
                sublist.Inpatient = Inpatientlist;
                MaternityBenefitslist = _customerService.GetMaternityBenefitList(className);
                sublist.MaternityBenefits = MaternityBenefitslist;
                Outpatientlist = _customerService.GetOutpatientList(className);
                sublist.Outpatient = Outpatientlist;
                ReimbursementClaimslist = _customerService.GetReimbursementClaimList(className);
                sublist.ReimbursementClaims = ReimbursementClaimslist;
                TOBsub.Add(sublist);
                tOB.TOBsublist = TOBsub;
            }
            return tOB;
        }
        #endregion

        #region GetPolicyJson    
        private string GetTOBJson(string PolicyNumber, string PolicyFromDate, string ClassCode)
        {
            string clientSecret = "{\"PolicyNumber\":\"" + PolicyNumber + "\",\"PolicyFromDate\":\"" + PolicyFromDate + "\",\"ClassCode\":\"" + ClassCode + "\"}";
            return clientSecret;
        }
        #endregion

        #region GetTOBDetailsFromTPA
        private async Task<TOBResponse> GetTOBResponse(string PolicyNumber, string PolicyFromDate, string ClassCode)
        {
            TOBResponse res = null;
            try
            {
                string url = appSettings.Value.Urls.TOBRequest;
                string username = appSettings.Value.BasicAuth.T_Username;
                string pass = appSettings.Value.BasicAuth.T_Password;
                HttpMessageHandler handler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(url),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                string val = System.Convert.ToBase64String(plainTextBytes);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                //Getting the input paramters as json 
                var content = GetTOBJson(PolicyNumber, PolicyFromDate, ClassCode);
                _logger.LogInformation(_className + "::GetTOBResponse::Content::" + content);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                _logger.LogInformation(_className + "::GetTOBResponse::Response::" + response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var st = JsonConvert.DeserializeObject<TOB>(response.Content.ReadAsStringAsync().Result);
                    if (st != null)
                    {
                        res = new TOBResponse();
                        res.responseCode = "Success";
                        res.responseData = st;
                        res.responseMessage = "User Policies From TPA Server";
                        // _customerService.Insert(st);
                        _logger.LogInformation(_className + "::GetTOBResponse::ResponseData::");
                    }
                    else
                    {
                        res = new TOBResponse();
                        res.responseCode = "Success";
                        res.responseData = null;
                        res.responseMessage = "User Policies Not Found";
                        _logger.LogInformation(_className + "::GetTOBResponse::ResponseData::NULL");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetTOBResponse::Exception" + ex.Message);
                res = new TOBResponse() { responseCode = "Failed", responseMessage = "Something went wrong." };
            }
            return res;
        }
        #endregion

        #endregion
        #region AddClaimRequest_New
        [Route("AddClaimRequest_New")]
        [HttpPost]
        public async Task<string> InsertClaimRequestNew(RequestCreateDTO _claimdetails)
        {
            string Status = "false";
            string result;
            string clientId = null;
            string requestId = null;
            try
            {
                MRClient _list = _customerService.GetClientByNationalId(_claimdetails.NationalId);
                if (_list != null)
                {
                    clientId = _list.Id.ToString();
                }

                if (_list == null)
                {
                    MRClient _clientdet = new MRClient();
                    _clientdet.IDNumber = _claimdetails.NationalId;
                    _clientdet.BankName = _claimdetails.ClientDTO.BankName;
                    _clientdet.ClientName = _claimdetails.ClientDTO.ClientName;
                    _clientdet.Email = _claimdetails.ClientDTO.Email;
                    if (_claimdetails.ClientDTO.GenderName == null)
                    {
                        _clientdet.GenderId = null;
                    }
                    else if (_claimdetails.ClientDTO.GenderName.ToUpper() == "MALE")
                    {
                        _clientdet.GenderId = 1;
                    }
                    else if (_claimdetails.ClientDTO.GenderName.ToUpper() == "FEMALE")
                    {
                        _clientdet.GenderId = 2;
                    }
                    _clientdet.IBANNumber = _claimdetails.ClientDTO.IBANNumber;
                    _clientdet.MobileNumber = _claimdetails.ClientDTO.MobileNumber;

                    clientId = _customerService.Insert(_clientdet).ToString();
                    _logger.LogInformation(_className + "::InsertClaimRequestNew::MRClientInserted");
                }

                var _climdet = new MRRequest();
                _climdet.ActualAmount = _claimdetails.ActualAmount;
                _climdet.CardExpireDate = _claimdetails.CardExpireDate;
                _climdet.CardNumber = _claimdetails.CardNumber;
                _climdet.ClaimTypeName = _claimdetails.ClaimTypeName;
                _climdet.ClientId = Convert.ToInt32(clientId);
                _climdet.ExpectedAmount = _claimdetails.ExpectedAmount;
                _climdet.HolderName = _claimdetails.HolderName;
                _climdet.MemberID = _claimdetails.MemberID;
                _climdet.MemberName = _claimdetails.MemberName;
                _climdet.PolicyNumber = _claimdetails.PolicyNumber;
                _climdet.RelationName = _claimdetails.RelationName;
                _climdet.RequestDate = _claimdetails.RequestDate;
                _climdet.RequestStatusId = 1;
                _climdet.TransferDate = _claimdetails.TransferDate;
                _climdet.VATAmount = _claimdetails.VATAmount;

                requestId = _customerService.Insert(_climdet).ToString();
                _logger.LogInformation(_className + "::InsertClaimRequestNew::MRRequestInserted");
                InsertCommentsFiles(clientId, requestId, _claimdetails);
                Status = "true";

            }
            catch (Exception ex)
            {
                Status = "false";
                _logger.LogInformation(_className + "::InsertClaimRequest::Status:FALSE::" + ex.Message  + "::InnerException::" + ex.InnerException.Message);
            }


            if (Status == "true")
            {
                try
                {
                    string url = appSettings.Value.Urls.AddReimbursmentClaims;
                    HttpMessageHandler handler = new HttpClientHandler();

                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(url),
                        Timeout = new TimeSpan(0, 2, 0)
                    };
                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
                    //Getting the input paramters as json 
                    var content = JsonConvert.SerializeObject(_claimdetails);
                    _logger.LogInformation(_className + "::InsertClaimRequest::CONTENT::");
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                    _logger.LogInformation(_className + "::InsertClaimRequest::Status::" + response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                        if (result != null)
                        {
                            var request = _customerService.GetReimbursmentClaimById(requestId);
                            request.RequestNumber = result;
                            _customerService.UpdateRequestNumber(request);
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(_className + "::InsertClaimRequest::Exception::" + ex.Message + "::InnerException::" + ex.InnerException.Message);
                }
            }

            return Status;
        }

        private void InsertCommentsFiles(string clientId, string requestId, RequestCreateDTO claimdetails)
        {
            if (claimdetails != null && clientId != null && requestId != null)
            {
                try
                {
                    MRRequestStatusLog rRequestStatusLog = new MRRequestStatusLog();
                    rRequestStatusLog.ClientId = Convert.ToInt32(clientId);
                    rRequestStatusLog.RequestId = Convert.ToInt32(requestId);
                    rRequestStatusLog.RequestStatusId = 1;
                    rRequestStatusLog.Comment = claimdetails.Comment;
                    rRequestStatusLog.EntryDate = DateTime.Now;
                    rRequestStatusLog.EntryEmpId = 0;
                    _customerService.Insert(rRequestStatusLog);
                    _logger.LogInformation(_className + "::InsertCommentsFiles::MRRequestStatusLog");
                    foreach (var item in claimdetails.RequestFileList)
                    {
                        MRRequestFile requestFile = new MRRequestFile();
                        requestFile.RequestId = Convert.ToInt32(requestId);
                        requestFile.FileDesc = item.FileDesc;
                        SaveFile(item);
                        requestFile.FilePath = UploadedFilepath;
                        requestFile.ClientId = Convert.ToInt32(clientId);
                        requestFile.EntryDate = DateTime.Now;
                        requestFile.IsClientVisible = true;
                        requestFile.EntryEmpId = 0;
                        requestFile.IsActive = true;
                        _customerService.Insert(requestFile);
                    }
                    _logger.LogInformation(_className + "::InsertCommentsFiles::MRRequestFile");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(_className + "::InsertCommentsFiles::Exception::" + ex.Message + "::InnerException::" + ex.InnerException.Message);
                }
            }
        }

        private async void SaveFile(RequestFileDTO item)
        {
            string uploadPath = appSettings.Value.FileUploadPath.Path;
            var fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(item.FileDesc);
            //var uploadPathWithfileName = Path.Combine(uploadPath, fileName);
            var uploadPathWithfileName = Path.Combine(
                            Directory.GetCurrentDirectory(), @"wwwroot\Uploads\",
                             fileName);
            try
            {

                using (var fileStream = new FileStream(uploadPathWithfileName, FileMode.Create))
                {
                    byte[] filest = item.MyFile;
                    var stream = new MemoryStream(filest);
                    IFormFile file = new FormFile(stream, 0, filest.Length, "name", item.FileDesc);
                    await file.CopyToAsync(fileStream);
                    UploadedFilepath = uploadPathWithfileName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::SaveFile::" + ex.Message);
            }
        }
        #endregion
        [Route("GetRelations")]
        [HttpGet]
        public IActionResult GetRelations(string nationalId)
        {
            List<Relations> relations = new List<Relations>();
            try
            {
                Registration user = _customerService.GetCustomerById(nationalId);
                
                if(user != null)
                {
                    var query = _customerService.GetRelationsByRegistrationId(user.Id);
                    if(query.Count() > 0)
                    {
                        relations = query.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetRelations::" + ex.Message);
            }
            return new JsonResult(new { responseCode = "Success", responseData = relations });
        }

        #region GetReimbursmentClaimsBy_NationalId
        [Route("GetClaimsByClientId_New/ClientId/{id}")]
        [HttpGet]
        public List<ReImClaims> GetClaimsByClientId_New(string id)
        {
            List<ReImClaims> reImClaims = null;
            try
            {
                var clientDTO = GetClientByNationalId(id);
                var _clmRequests = GetClmRequestByNationalId(clientDTO.ClientId, clientDTO);

                if (_clmRequests != null)
                {
                    reImClaims = _clmRequests;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_className + "::GetClaimsByClientId_New::Exception::" + ex.Message);
            }
            return reImClaims;
        }

        private List<ReImClaims> GetClmRequestByNationalId(string id, ClientDTO clientDTO)
        {
            List<ReImClaims> reImClaims = null;
            ReImClaims claims = null;
            var claimrequests = _customerService.GetReimByClientId(id);
            if (claimrequests != null)
            {
                reImClaims = new List<ReImClaims>();
                foreach (var request in claimrequests)
                {
                    claims = new ReImClaims();
                    claims.RequestId = request.Id;
                    claims.RequestNumber = request.RequestNumber;
                    if (request.RequestDate != null)
                    {
                        claims.RequestDate = (DateTime)request.RequestDate;
                    }
                    claims.PolicyNumber = request.PolicyNumber;
                    claims.ClaimTypeName = request.ClaimTypeName;
                    claims.ExpectedAmount = (double)request.ExpectedAmount;
                    claims.ActualAmount = request.ActualAmount;
                    claims.VATAmount = (double)request.VATAmount;
                    claims.CreateDate = null;
                    if (request.RequestStatusId == 1)
                    {
                        claims.RequestStatusName = "Pending";
                    }
                    if (request.RequestStatusId == 2)
                    {
                        claims.RequestStatusName = "Under Process";
                    }
                    if (request.RequestStatusId == 3)
                    {
                        claims.RequestStatusName = "Missing / Additional information";
                    }
                    if (request.RequestStatusId == 4)
                    {
                        claims.RequestStatusName = "Approved with Partial payment";
                    }
                    if (request.RequestStatusId == 5)
                    {
                        claims.RequestStatusName = "Approved with Full payment";
                    }
                    if (request.RequestStatusId == 6)
                    {
                        claims.RequestStatusName = "Rejected";
                    }
                    if (request.RequestStatusId == 7)
                    {
                        claims.RequestStatusName = "Amount Transferred";
                    }
                    if (request.RequestStatusId == 8)
                    {
                        claims.RequestStatusName = "Accepted";
                    }
                    if (request.RequestStatusId == 1)
                    {
                        claims.RequestStatusName = "Pending";
                    }
                    var cldto = new ClientDTO();
                    cldto.ClientId = clientDTO.Id.ToString();
                    cldto.ClientName = clientDTO.ClientName;
                    cldto.IDNumber = clientDTO.IDNumber;
                    cldto.MobileNumber = clientDTO.MobileNumber;
                    cldto.Email = clientDTO.Email;
                    cldto.GenderName = clientDTO.GenderName;
                    cldto.IBANNumber = clientDTO.IBANNumber;
                    cldto.BankName = clientDTO.BankName;
                    cldto.Id = Convert.ToInt32(clientDTO.Id);
                    claims.ClientDTO = cldto;
                    reImClaims.Add(claims);
                }
            }
            return reImClaims;
        }

        private ClientDTO GetClientByNationalId(string id)
        {
            ClientDTO clientDTO = null;
            try
            {
                var clientdetails = _customerService.GetClientByNationalId(id);
                if (clientdetails != null)
                {
                    clientDTO = new ClientDTO();
                    clientDTO.ClientId = clientdetails.Id.ToString();
                    clientDTO.ClientName = clientdetails.ClientName;
                    clientDTO.IDNumber = clientdetails.IDNumber;
                    clientDTO.MobileNumber = clientdetails.MobileNumber;
                    clientDTO.Email = clientdetails.Email;
                    clientDTO.GenderName = clientdetails.GenderId.ToString();
                    clientDTO.IBANNumber = clientdetails.IBANNumber;
                    clientDTO.BankName = clientdetails.BankName;
                    clientDTO.Id = Convert.ToInt32(clientdetails.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetClientByNationalId::" + ex.Message);
            }

            return clientDTO;
        }
        #endregion

        #region GetReimbursmentClaim_Details_By_Id
        [Route("GetClaimDetailsById_New/Id/{id}")]
        [HttpGet]
        public async Task<RequestCreateDTO> GetClaimDetailsById_New(string id)
        {
            RequestCreateDTO reImClaimdetails = null;
            try
            {
                var request = _customerService.GetReimbursmentClaimById(id);

                HttpMessageHandler handler = new HttpClientHandler();
                string url = appSettings.Value.Urls.GetReimbursmentDetails;
                string cpath = url + request.RequestNumber;
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(cpath),
                    Timeout = new TimeSpan(0, 2, 0)
                };
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
                HttpResponseMessage response = await httpClient.GetAsync(cpath);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var a = JsonConvert.DeserializeObject<RequestCreateDTO>(response.Content.ReadAsStringAsync().Result);
                    if ( a != null && a.RequestStatusName != null && a.RequestStatusName.ToUpper() != "PENDING")
                    {
                        bool status = UpdateRequesetStatus(a.RequestStatusName, a.RequestId, id, a.ClientDTO.ClientId, request);
                    }
                    else
                    {
                        _logger.LogInformation(_className + "::GetClaimDetailsById_New::ResponseNull");
                    }
                    reImClaimdetails = a;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetClaimDetailsById_New::Exception::" + ex.Message);
            }
            return reImClaimdetails;
        }

        private bool UpdateRequesetStatus(string requestStatusName, int reqid, string recordid, string clientid, MRRequest request)
        {
            bool status = false;
            try
            {

                if (requestStatusName == "Under Process")
                {
                    request.RequestStatusId = 2;
                }
                if (requestStatusName == "Missing / Additional information")
                {
                    request.RequestStatusId = 3;
                }
                if (requestStatusName == "Approved with Partial payment")
                {
                    request.RequestStatusId = 4;
                }
                if (requestStatusName == "Approved with Full payment")
                {
                    request.RequestStatusId = 5;
                }
                if (requestStatusName == "Rejected")
                {
                    request.RequestStatusId = 6;
                }
                if (requestStatusName == "Amount Transferred")
                {
                    request.RequestStatusId = 7;
                }
                if (requestStatusName == "Accepted")
                {
                    request.RequestStatusId = 8;
                }
                _customerService.UpdateRequestStatus(request);


                MRRequestStatusLog rRequestStatusLog = new MRRequestStatusLog();
                rRequestStatusLog.ClientId = Convert.ToInt32(clientid);
                rRequestStatusLog.RequestId = Convert.ToInt32(recordid);
                rRequestStatusLog.RequestStatusId = (int)request.RequestStatusId;
                rRequestStatusLog.Comment = "";
                rRequestStatusLog.EntryDate = DateTime.Now;
                rRequestStatusLog.EntryEmpId = 0;
                _customerService.Insert(rRequestStatusLog);

                status = true;
            }
            catch (Exception ex)
            {
                return status;
            }


            return status;
        }
        #endregion
        [Route("RegistrationRequest")]
        [HttpPost]
        public async Task<Registration> RegistrationRequest(Registration registration)
        {
            //sample input-{"Iqama_NationalID":"1039640063","DOB":"01-05-1987"}
            string status = "false";
            Registration res = null;
            try
            {
                string url = appSettings.Value.Urls.RegistrationRequest;
                string username = appSettings.Value.BasicAuth.T_Username;
                string pass = appSettings.Value.BasicAuth.T_Password;
                HttpMessageHandler handler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(url),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                string val = System.Convert.ToBase64String(plainTextBytes);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                //Getting the input paramters as json 
                var content = GetRegJson(registration.Iqama_NationalID, registration.DOB);

                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var st = JsonConvert.DeserializeObject<Registration>(response.Content.ReadAsStringAsync().Result);
                    status = "true";
                    if (st != null)
                    {
                        res = st;
                    }
                    else
                    {
                        res = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }

        #region GetRegJson    
        private string GetRegJson(string nationalId, string YOB)
        {
            string clientSecret = "{\"Id\":\"" + nationalId + "\",\"DOB\":\"" + YOB + "\"}";
            return clientSecret;
        }
        #endregion

        #region Policies

        #region GetAllPolicies
        [Route("GetAllPolicies")]
        [HttpPost]
        public async Task<PolicyResponse> GetAllPolicies(ClsInput clsInput)
        {
            List<Policies> _policies = null;
            PolicyResponse res = null;
            //check whether is user policies in db or not
            _policies = _customerService.GetPoiciesByNationalId(clsInput.nationalID);
            if (_policies.Count > 0)
            {
                res = new PolicyResponse();
                res.responseCode = "Success";
                res.responseData = _policies;
                res.responseMessage = "User Policies From Table";
                return res;
            }
            else
            {
                //var customerDetails = GetAllUsers().Where(c => c.Iqama_NationalID == clsInput.nationalID).FirstOrDefault();
                var customerDetails = _customerService.GetCustomerById(clsInput.nationalID);
                res = await GetPolicyResponse(customerDetails.PolicyNo, customerDetails.TushfaMemberNo);
            }
            return res;
        }
        #endregion

        #region GetPolicyJson    
        private string GetPolicyJson(string PolicyNumber, string TushfaMemberNumber)
        {
            string clientSecret = "{\"PolicyNumber\":\"" + PolicyNumber + "\",\"TushfaMemberNumber\":\"" + TushfaMemberNumber + "\"}";
            return clientSecret;
        }
        #endregion

        #region GetPoliciesFromTPA
        private async Task<PolicyResponse> GetPolicyResponse(string policyno, string tushfamemno)
        {
            List<Policies> _policies = null;
            PolicyResponse res = null;
            _logger.LogInformation(_className + "::GetPolicyResponse::START");
            try
            {
                string url = appSettings.Value.Urls.PoliciesRequest;
                string username = appSettings.Value.BasicAuth.T_Username;
                string pass = appSettings.Value.BasicAuth.T_Password;
                HttpMessageHandler handler = new HttpClientHandler();

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(url),
                    Timeout = new TimeSpan(0, 2, 0)
                };

                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                //This is the key section you were missing    
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                string val = System.Convert.ToBase64String(plainTextBytes);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                //Getting the input paramters as json 
                var content = GetPolicyJson(policyno, tushfamemno);
                _logger.LogInformation(_className + "::GetPolicyResponse::" + content);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                _logger.LogInformation(_className + "::GetPolicyResponse::COntent" + httpContent);
                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                _logger.LogInformation(_className + "::GetPolicyResponse::STATUS::" + response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var st = JsonConvert.DeserializeObject<Policies>(response.Content.ReadAsStringAsync().Result);
                    if (st != null)
                    {
                        res = new PolicyResponse();
                        res.responseCode = "Success";
                        res.responseData = _policies;
                        res.responseMessage = "User Policies From Table";
                        _customerService.Insert(st);
                    }
                    else
                    {
                        res = new PolicyResponse();
                        res.responseCode = "Success";
                        res.responseData = null;
                        res.responseMessage = "User Policies Not Found";

                    }
                }
                _logger.LogInformation(_className + "::GetPolicyResponse::response.StatusCode::"+ response);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetPolicyResponse::Exception::" + ex.Message + "::innerException::" + ex.InnerException.Message);
                res = new PolicyResponse() { responseCode = "Failed", responseMessage = "Something went wrong.", responseData = new List<Policies>() };
            }
            return res;
        }
        #endregion

        #endregion
        [Route("CheckMemberByNationalId")]
        [HttpPost]
        public async Task<RegistrationResponse> GetMemberByNationalId(ClsInput clsInput)
        {
            RegistrationResponse _memberdetails = new RegistrationResponse();
            try
            {
                _logger.LogInformation(_className + "::GetMemberByNationalId::YearOfBirth::BeforeCheck" + clsInput.yearOfBirth);
                var userexist = _customerService.CheckIfUserExists(clsInput.nationalID, clsInput.yearOfBirth);
                _logger.LogInformation(_className + "::GetMemberByNationalId::YearOfBirth::AfterCheck" + clsInput.yearOfBirth);
                _logger.LogInformation(_className + "::GetMemberByNationalId::START");
                if (userexist != true)
                {
                    HttpMessageHandler handler = new HttpClientHandler();
                    string url = appSettings.Value.Urls.RegistrationRequest;
                    string username = appSettings.Value.BasicAuth.T_Username;
                    string pass = appSettings.Value.BasicAuth.T_Password;
                    var httpClient = new HttpClient(handler)
                    {
                        BaseAddress = new Uri("http://130.90.4.130/PolicyEnquiryAPIUAT/api/Policy/RegistrationReq"),
                        Timeout = new TimeSpan(0, 2, 0)
                    };

                    httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
                    //This is the key section you were missing    
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(username + ":" + pass);
                    string val = System.Convert.ToBase64String(plainTextBytes);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
                    //Getting the input paramters as json 
                    string content = GetRegJson(clsInput.nationalID, clsInput.yearOfBirth);
                    _logger.LogInformation(_className + "::GetMemberByNAtionalId::RequestContent::" + content);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync("http://130.90.4.130/PolicyEnquiryAPIUAT/api/Policy/RegistrationReq", httpContent);

                    _logger.LogInformation(_className + "::GetMemberByNationalId::Response::" + response.StatusCode);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation(_className + "::GetMemberByNAtionalId::Response::OK::" + response.Content.ReadAsStringAsync().Result);
                        var res = JsonConvert.DeserializeObject<RegistrationResponse>(response.Content.ReadAsStringAsync().Result);
                        
                        if (res != null)
                        {
                            _memberdetails = res;
                        }
                        else
                        {
                            _memberdetails.Errors = res.Errors;
                        }

                    }
                }
                else
                {
                    _memberdetails.ResponseMessage = "User Already Exists";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(_className + "::GetMemberByNationalId::" + ex.Message);
                _memberdetails = new RegistrationResponse() { ResponseMessage = "Something went wrong" };
            }
            return _memberdetails;
        }
    }
}
