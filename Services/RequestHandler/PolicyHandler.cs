using Castle.Core.Logging;
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
    public class PolicyHandler
    {
        private readonly string _className = "PolicyHandler";
        private readonly ILogger<PolicyHandler> _logger;
        public PolicyHandler(ILogger<PolicyHandler> logger)
        {
            _logger = logger;
        }
        public async Task<List<Policies>> GetPoliciesByNationalId(ClsInput clsInput)
        {
            List<Policies> _policies = null;
            HttpMessageHandler handler = new HttpClientHandler();

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(WebConstants.HostAddress + "api/GetAllPolicies"),
                Timeout = new TimeSpan(0, 2, 0)
            };

            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

            //Getting the input paramters as json 
            string content = GetJson(clsInput);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(WebConstants.HostAddress + "api/GetAllPolicies", httpContent);


            if (response.StatusCode == HttpStatusCode.OK)
            {
                var res = JsonConvert.DeserializeObject<PolicyResponse>(response.Content.ReadAsStringAsync().Result);
                _policies = res.responseData;
            }
            return _policies;

        }

        #region GetInputJson    
        public string GetJson(ClsInput clsInput)
        {
            string clientSecret = "{\r\n\"code\":\"CI\",\r\n\"nationalID\": \"" + clsInput.nationalID + "\",\r\n\"yearOfBirth\": \"" + clsInput.yearOfBirth + "\",\r\n\"insPolicyNo\": \"" + clsInput.insPolicyNo + "\"\r\n}\r\n";
            return clientSecret;
        }
        #endregion

        public List<Policies> GetPoliciesById(string nationalId, string yob)
        {
            return GetPoliciesByNationalId(nationalId, yob).Result;
        }
        public async Task<List<Policies>> GetPoliciesByNationalId(string nationalId, string yob)
        {           
            var result = new List<Policies>();
            try
            {
                if (nationalId != null && yob != null)
                {
                    var clsInput = new ClsInput();
                    clsInput.code = "CI";
                    clsInput.nationalID = nationalId;
                    DateTime date = DateTime.Parse(yob);
                    //DateTime date = Convert.ToDateTime(yob);
                    clsInput.yearOfBirth = date.Year.ToString();
                    clsInput.insPolicyNo = "";
                    result = await GetPoliciesByNationalId(clsInput);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_className + "::GetPoliciesByNationalId::" + ex.Message);
            }
            return result;
        }
    }
}
