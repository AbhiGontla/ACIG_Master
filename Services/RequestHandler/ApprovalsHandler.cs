﻿using Castle.Core.Logging;
using Core;
using Core.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestHandler
{
    public class ApprovalsHandler
    {
        private readonly ILogger<ApprovalsHandler> _logger;
        private readonly string _className = "ApprovalsHandler";
        public ApprovalsHandler(ILogger<ApprovalsHandler> logger)
        {
            _logger = logger;
        }
        public async Task<List<Approvals>> GetApprovalsByNationalId(ClsInput clsInput)
        {
            List<Approvals> _approvals = null;
            HttpMessageHandler handler = new HttpClientHandler();

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(WebConstants.HostAddress + "api/GetApprovals"),
                Timeout = new TimeSpan(0, 2, 0)
            };

            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

            //Getting the input paramters as json 
            string content = GetJson(clsInput);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(WebConstants.HostAddress + "api/GetApprovals", httpContent);
            
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var res = JsonConvert.DeserializeObject<GetApprovalsResponse>(response.Content.ReadAsStringAsync().Result);
                _approvals = res.responseData;
            }
            return _approvals;

        }

        #region GetInputJson    
        public string GetJson(ClsInput clsInput)
        {
            string clientSecret = "{\r\n\"code\":\"CI\",\r\n\"nationalID\": \"" + clsInput.nationalID + "\",\r\n\"yearOfBirth\": \"" + clsInput.yearOfBirth + "\",\r\n\"insPolicyNo\": \"" + clsInput.insPolicyNo + "\"\r\n}\r\n";
            return clientSecret;
        }
        #endregion

        public List<Approvals> GetApprovById(string nationalId, string yob)
        {
            return GetApprovalsByNationalId(nationalId, yob).Result;
        }
        public async Task<List<Approvals>> GetApprovalsByNationalId(string nationalId, string yob)
        {      
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
                    result = await GetApprovalsByNationalId(clsInput);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_className + "::GetApprovalsByNationalId::" + ex.Message);
            }
            return result;
        }
    }
}
