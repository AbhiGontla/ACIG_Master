using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Api
{
    class AddClaimsRequest
    {
    }

    public class UpdateClaimRequest
    {
        public int RequestId { get; set; }
        public string Comment { get; set; }
        public List<RequestFileDTO> RequestFileList { get; set; }
    }
    public class MRRequestStatusLog
    {
        public int StatusLogId { get; set; }
        public int RequestId { get; set; }
        public int RequestStatusId { get; set; }
        public string Comment { get; set; }
        public int ClientId { get; set; }
        public int EntryEmpId { get; set; }
        public DateTime EntryDate { get; set; }
    }
    public class MRRequestFile
    {
        public int FileId { get; set; }
        public int RequestId { get; set; }
        public string FileDesc { get; set; }
        public string FilePath { get; set; }
        public int ClientId { get; set; }
        public int EntryEmpId { get; set; }
        public DateTime EntryDate { get; set; }
        public bool IsClientVisible { get; set; }
        public bool IsActive { get; set; }
        public bool IsBordereau { get; set; }
    }
}
