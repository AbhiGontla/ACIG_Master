﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Api
{
    public class RequestCreateDTO : BaseEntity
    {
        public int? ClientId { get; set; }
        public string RequestNumber { get; set; }
        public DateTime? RequestDate { get; set; }
        public int? RequestStatusId { get; set; }
        public string RequestStatusName { get; set; }
        public string PolicyNumber { get; set; }
        public string HolderName { get; set; }
        public string MemberID { get; set; }
        public string MemberName { get; set; }
        public string RelationName { get; set; }
        public string ClaimTypeName { get; set; }
        public string CardNumber { get; set; }
        public DateTime? CardExpireDate { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public decimal? ActualAmount { get; set; }
        public decimal? VATAmount { get; set; }
        public DateTime? TransferDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public string NationalId { get; set; }
        public string Comment { get; set; }

        public  ClientDTO ClientDTO { get; set; }
        public int RequestId { get; set; }
        public  List<RequestFileDTO> RequestFileList { get; set; }
        public List<RequestStatusLogList> RequestStatusLogList { get; set; }
    }

    public class ReimbursmentResponse
    {
        public RequestCreateDTO[] _reimbursmentClaims { get; set; }
    }
    [NotMapped]
    public class MRRequest : BaseEntity
    {
        public int? ClientId { get; set; }
        public string RequestNumber { get; set; }
        public DateTime? RequestDate { get; set; }
        public int? RequestStatusId { get; set; }

        public string PolicyNumber { get; set; }
        public string HolderName { get; set; }
        public string MemberID { get; set; }
        public string MemberName { get; set; }
        public string RelationName { get; set; }
        public string ClaimTypeName { get; set; }
        public string CardNumber { get; set; }
        public DateTime? CardExpireDate { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public decimal? ActualAmount { get; set; }
        public decimal? VATAmount { get; set; }
        public DateTime? TransferDate { get; set; }

        //public virtual ClientDTO ClientDTO { get; set; }

        //public virtual List<RequestFileDTO> RequestFileList { get; set; }

    }
}
