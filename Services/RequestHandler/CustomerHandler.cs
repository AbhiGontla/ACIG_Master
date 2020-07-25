using Castle.Core.Logging;
using Core;
using Core.Api;
using Core.Domain;
using Core.Sms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.RequestHandler
{
    public class CustomerHandler
    {
        public List<Registration> GetUsers()
        {
            return GetAllUsers().Result;
        }
        public async Task<List<Registration>> GetAllUsers()
        {
            List<Registration> users = null;
            try
            {

                HttpMessageHandler handler = new HttpClientHandler();
                string url = WebConstants.HostAddress + "api/GetAllUsers";
                string cpath = url;
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(cpath),
                    Timeout = new TimeSpan(0, 2, 0)
                };
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");


                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var a = JsonConvert.DeserializeObject<List<Registration>>(response.Content.ReadAsStringAsync().Result);
                    users = a;
                }
                else
                {
                    users = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return users;
        }
        public Registration GetCustomerById(string Nid)
        {
            var _allcustomers = GetAllUsers();
            if (_allcustomers != null)
            {
                return _allcustomers.Result.Find(c => c.Iqama_NationalID == Nid);
            }
            else
            {
                return null;
            }
        }
        #region GetInputJson    
        public string GetJson(ClsInput clsInput)
        {
            string clientSecret = "{\r\n\"code\":\"CI\",\r\n\"nationalID\": \"" + clsInput.nationalID + "\",\r\n\"yearOfBirth\": \"" + clsInput.yearOfBirth + "\",\r\n\"insPolicyNo\": \"" + clsInput.insPolicyNo + "\"\r\n}\r\n";
            return clientSecret;
        }
        #endregion
        public async Task<RegistrationResponse> GetMemberByNationalId(ClsInput clsInput)
        {
            RegistrationResponse users = null;
            try
            {

                HttpMessageHandler handler = new HttpClientHandler();
                string url = WebConstants.HostAddress + "api/CheckMemberByNationalId";
                string cpath = url;
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(cpath),
                    Timeout = new TimeSpan(0, 2, 0)
                };
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

                string content = GetJson(clsInput);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(WebConstants.HostAddress + "api/CheckMemberByNationalId", httpContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var a = JsonConvert.DeserializeObject<RegistrationResponse>(response.Content.ReadAsStringAsync().Result);
                    users = a;
                    if (users == null)
                    {
                        users = new RegistrationResponse();
                        users.Errors = new List<RequestError>() { new RequestError() { Message = "Something went wrong" } };
                    }
                }
                else
                {
                    users = new RegistrationResponse();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return users;
        }
        #region SMS
        public ResponseSMS SendSms(string mobilenumber)
        {
            return SendSmsuser(mobilenumber).Result;
        }
        public async Task<ResponseSMS> SendSmsuser(string mobilenumber)
        {
            string status = null;
            HttpMessageHandler handler = new HttpClientHandler();
            ResponseSMS smsResponse = new ResponseSMS();
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(WebConstants.HostAddress + "api/SendSms"),
                Timeout = new TimeSpan(0, 2, 0)
            };

            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

            //Getting the input paramters as json 
            var content = GetSMSJson(mobilenumber);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(WebConstants.HostAddress + "api/SendSms", httpContent);


            if (response.StatusCode == HttpStatusCode.OK)
            {
                var res = JsonConvert.DeserializeObject<ResponseSMS>(response.Content.ReadAsStringAsync().Result);
                if (res != null)
                {
                    smsResponse = res;
                }
            }
            return smsResponse;
        }
        public string GetSMSJson(string mobilenumber)
        {
            string clientSecret = "{\r\n\"MobileNumber\":\"" + mobilenumber + "\"\r\n}";
            return clientSecret;
        }
        #endregion
    }
}
