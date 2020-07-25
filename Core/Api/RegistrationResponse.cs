using Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Api
{
    public class RegistrationResponse
    {
        public List<Registration> Members { get; set; }
        public List<RequestError> Errors { get; set; }
        public string ResponseMessage { get; set; }
    }
    public class RequestError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Field { get; set; }

    }
}
