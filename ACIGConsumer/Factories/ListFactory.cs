using ACIGConsumer.Models.Approvals;
using ACIGConsumer.Models.Claims;
using ACIGConsumer.Models.CoverageLimits;
using ACIGConsumer.Models.Policy;
using ACIGConsumer.Models.Providers;
using Core.Infrastructure;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Factories
{
    public class ListFactory : IListFactory
    {
        private readonly ICustomerService _customerService;
        public ListFactory(ICustomerService customerService)
        {
            this._customerService = customerService;
        }
        public PolicyListModel PreparePolicyListModel(PolicySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var direction = searchModel.Sort.Equals("asc") ? false : true;
            var policies = _customerService.GetPoicies(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            var model = new PolicyListModel()
            {
                Data = policies.Select(p =>
                {
                    var policyModel = new PolicyModel()
                    {
                        PolicyNumber = p.PolicyNumber,
                        PolicyFromDate = p.PolicyFromDate,
                        PolicyToDate = p.PolicyToDate,
                        PolicyType = p.PolicyType,
                        ClientType = p.ClientType,
                        ClassCode = p.ClassCode,
                        ClassName = p.ClassName,
                        NetworkCode = p.NetworkCode,
                        NetworkName = p.NetworkName,
                        TPAID = p.TPAID,
                        TPAName = p.TPAName,
                        ClientName = p.ClientName,
                        PolicySponsorName = p.PolicySponsorName,
                        BrokerName = p.BrokerName,
                        SponsorID = p.SponsorID,
                        Iqama_NationalID = p.Iqama_NationalID,
                        MemberName = p.MemberName,
                        MobileNumber = p.MobileNumber,
                        Gender = p.Gender,
                        MaritalStatus = p.MaritalStatus,
                        DOBGreg = p.DOBGreg,
                        DOBHijri = p.DOBHijri,
                        IDExpiryDate = p.IDExpiryDate,
                        OccupationCode = p.OccupationCode,
                        OccupationDesc = p.OccupationDesc,
                        NationalityCode = p.NationalityCode,
                        NationalityDesc = p.NationalityDesc,
                        MemberTypeCode = p.MemberTypeCode,
                        MemberTypeDesc = p.MemberTypeDesc,
                        RelationCode = p.RelationCode,
                        RelationDesc = p.RelationDesc,
                        EmployeeNo = p.EmployeeNo,
                        MemberStatus = p.MemberStatus,
                        TushfaMemberNo = p.TushfaMemberNo,
                        CardNo = p.CardNo,
                        EnrollmentDate = p.EnrollmentDate,
                        MigrationDate = p.MigrationDate,
                        DeletionDate = p.DeletionDate,
                        AdditionPremium = p.AdditionPremium,
                        MigrationPremium = p.MigrationPremium,
                        DeletionPremium = p.DeletionPremium,
                        CCHIStatus = p.CCHIStatus,
                        CCHIUploadDate = p.CCHIUploadDate,
                        CCHIErrorMessage = p.CCHIErrorMessage,
                        DeletionReason = p.DeletionReason,
                        CityCode = p.CityCode,
                        CityDesc = p.CityDesc,
                        RegionCode = p.RegionCode,
                        RegionDesc = p.RegionDesc,
                        TransDate = p.TransDate
                    };
                    return policyModel;
                }),
            };
            model.Meta = new PageListMetadata
            {
                Page = policies.PageIndex + 1,
                Perpage = policies.PageSize,
                Pages = policies.TotalPages,
                Sort = searchModel.Sort,
                Field = searchModel.Field,
                Total = policies.TotalCount
            };
            return model;
        }

        public ApprovalListModel PrepareApprovalListModel(ApprovalSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var direction = searchModel.Sort.Equals("asc") ? false : true;
            var approvals = _customerService.GetApprovals(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            var model = new ApprovalListModel()
            {
                Data = approvals.Select(p => p)
            };
            model.Meta = new PageListMetadata
            {
                Page = approvals.PageIndex + 1,
                Perpage = approvals.PageSize,
                Pages = approvals.TotalPages,
                Sort = searchModel.Sort,
                Field = searchModel.Field,
                Total = approvals.TotalCount
            };
            return model;

        }
        public CoverageBalanceListModel PrepareCoverageBalanceListModel(CoverageBalanceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var direction = searchModel.Sort.Equals("asc") ? false : true;
            var coverageBalances = _customerService.GetCoverageBalances(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            var model = new CoverageBalanceListModel()
            {
                Data = coverageBalances.Select(p => p)
            };
            model.Meta = new PageListMetadata
            {
                Page = coverageBalances.PageIndex + 1,
                Perpage = coverageBalances.PageSize,
                Pages = coverageBalances.TotalPages,
                Sort = searchModel.Sort,
                Field = searchModel.Field,
                Total = coverageBalances.TotalCount
            };
            return model;
        }
        public ClaimListModel PrepareClaimListModel(ClaimSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            List<ClaimModel> claimModels = new List<ClaimModel>();
            var direction = searchModel.Sort.Equals("asc") ? false : true;
            var paidClaims = _customerService.GetPaidClaims(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            var osClaims = _customerService.GetOSClaims(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            foreach (var claim in osClaims)
            {
                claimModels.Add(new ClaimModel{Code = claim.Code,NationalID = claim.NationalID,YearofBirth = claim.YearofBirth,InsPolicyNo = claim.InsPolicyNo,CL_STATUS = claim.CL_STATUS,CL_COUNTRY = claim.CL_COUNTRY,CL_SEQID = claim.CL_SEQID,CL_VISA_CODE = claim.CL_VISA_CODE,CL_VISA = claim.CL_VISA,
                    CL_VISA_STS = claim.CL_VISA_STS,CL_ADMDATE = claim.CL_ADMDATE,CL_DCLDTE = claim.CL_DCLDTE,POLICY_SEQ = claim.POLICY_SEQ,CL_PP_NO = claim.CL_PP_NO,CL_ClASS = claim.CL_CLASS,CL_RISK = claim.CL_RISK,CL_SUBRISK = claim.CL_SUBRISK,CL_SUBADM = claim.CL_SUBADM,CL_DISCHARGE = claim.CL_DISCHARGE,
                    CL_PROVIDERTYPE = claim.CL_PROVIDER_TYPE,CL_PROVIDERNO = claim.CL_PROVIDER_NO,CL_CURR = claim.CL_CURR,CL_INV_DATE = claim.CL_INV_DATE,CL_BATCH = claim.CL_BATCH,CL_DIAG = claim.CL_DIAG,CL_DIAG_DESC = claim.CL_DIAG_DESC,CL_INV_NO = claim.CL_INV_NO,CL_ACCIDATE = claim.CL_ACCIDT,CL_CLMAMT_OR = claim.CL_CLMAMT_OR,
                    CL_CLMAMT_LL = claim.CL_CLMAMT_LL,CL_STSER = claim.CL_STSER,CL_PROD = claim.CL_PROD,CL_STLDATE = claim.CL_STLDATE,CL_INV_RDATE = claim.CL_INV_RDATE,CL_PAIDAMT_OR = claim.CL_PAIDAMT_OR,CL_PAIDAMT_LL = claim.CL_PAIDAMT_LL,CL_HOSPAMT_OR = claim.CL_HOSPAMT_OR,CL_HOSPAMT_LL = claim.CL_HOSPAMT_LL,CL_CALCAMT_OR = claim.CL_CALCAMT_OR,CL_CALCAMT_LL = claim.CL_CALCAMT_LL,CL_PAYABLE_OR = claim.CL_PAYABLE_OR,CL_PAYABLE_LL = claim.CL_PAYABLE_LL,
                    CL_DEDCTN_OR = claim.CL_DEDCTN_OR,CL_DEDCTN_LL = claim.CL_DEDCTN_LL,CL_DEDCTBL_OR = claim.CL_DEDCTBL_OR,CL_DEDCTBL_LL = claim.CL_DEDCTBL_LL,CL_FSTVSA = claim.CL_FSTVSA,CL_CCHINO = claim.CL_CCHINO,CL_CLMTYPE = claim.CL_CLMTYPE,
                    CL_SRVC = claim.CL_SRVC,CL_DEDREASON = claim.CL_DEDREASON,CL_INSPOLNO = claim.CL_INSPOLNO,CL_INSINSURD = claim.CL_INSINSURD,CL_PROVNAME = claim.CL_PROVNAME,CL_BATCH_STS = claim.CL_BATCH_STS,CL_TRFTUI = claim.CL_TRFTUI,CL_SUBOFF = claim.CL_SUBOFF,
                    CL_FILENO = claim.CL_FILENO,CL_DEDMED = claim.CL_DEDMED,CL_DEDPROV = claim.CL_DEDPROV,CL_FTYPE = claim.CL_FTYPE,CL_VATAMT = claim.CL_VATAMT,CL_VATNET = claim.CL_VATNET,SRV_DESC = claim.SRV_DESC,SERIAL = claim.SERIAL,GROSS_OR = claim.GROSS_OR,
                    GROSS_LL = claim.GROSS_LL,NETPAYABLE_OR = claim.NETPAYABLE_OR,NETPAYABLE_LL = claim.NETPAYABLE_LL,ADM_TYPE = claim.ADM_TYPE,POLICY_INC = claim.POLICY_INC,PROVIDER_CITY = claim.PROVIDER_CITY,EMERGENCY_CHK = claim.EMERGENCY_CHK,CONGINATAL_CHK = claim.CONGINATAL_CHK,PRE_DISEASE_CHK = claim.PRE_DISEASE_CHK,
                    LOGINDATE = claim.LOGIN_DATE,FROMDATE = claim.FROM_DATE,TODATE = claim.TO_DATE,STATUS = claim.STATUS,});
            }
            foreach (var claim in paidClaims)
            {
                claimModels.Add(new ClaimModel
                {
                    Code = claim.Code,NationalID = claim.NationalID,YearofBirth = claim.YearofBirth,InsPolicyNo = claim.InsPolicyNo,CL_STATUS = claim.CL_STATUS,CL_COUNTRY = claim.CL_COUNTRY,CL_SEQID = claim.CL_SEQID,CL_VISA_CODE = claim.CL_VISA_CODE,CL_VISA = claim.CL_VISA,
                    CL_VISA_STS = claim.CL_VISA_STS,CL_ADMDATE = claim.CL_ADMDATE,CL_DCLDTE = claim.CL_DCLDTE,POLICY_SEQ = claim.POLICY_SEQ,CL_PP_NO = claim.CL_PP_NO,CL_ClASS = claim.CL_ClASS,CL_RISK = claim.CL_RISK,CL_SUBRISK = claim.CL_SUBRISK,CL_SUBADM = claim.CL_SUBADM,
                    CL_DISCHARGE = claim.CL_DISCHARGE,CL_PROVIDERTYPE = claim.CL_PROVIDERTYPE,CL_PROVIDERNO = claim.CL_PROVIDERNO,CL_CURR = claim.CL_CURR,CL_INV_DATE = claim.CL_INV_DATE,CL_BATCH = claim.CL_BATCH,CL_DIAG = claim.CL_DIAG,CL_DIAG_DESC = claim.CL_DIAG_DESC,
                    CL_INV_NO = claim.CL_INV_NO,CL_ACCIDATE = claim.CL_ACCIDATE,CL_CLMAMT_OR = claim.CL_CLMAMT_OR,CL_CLMAMT_LL = claim.CL_CLMAMT_LL,CL_STSER = claim.CL_STSER,CL_PROD = claim.CL_PROD,CL_STLDATE = claim.CL_STLDATE,CL_INV_RDATE = claim.CL_INV_RDATE,CL_PAIDAMT_OR = claim.CL_PAIDAMT_OR,
                    CL_PAIDAMT_LL = claim.CL_PAIDAMT_LL,CL_HOSPAMT_OR = claim.CL_HOSPAMT_OR,CL_HOSPAMT_LL = claim.CL_HOSPAMT_LL,CL_CALCAMT_OR = claim.CL_CALCAMT_OR,CL_CALCAMT_LL = claim.CL_CALCAMT_LL,CL_PAYABLE_OR = claim.CL_PAYABLE_OR,CL_PAYABLE_LL = claim.CL_PAYABLE_LL,
                    CL_DEDCTN_OR = claim.CL_DEDCTN_OR,CL_DEDCTN_LL = claim.CL_DEDCTN_LL,CL_DEDCTBL_OR = claim.CL_DEDCTBL_OR,CL_DEDCTBL_LL = claim.CL_DEDCTBL_LL,CL_FSTVSA = claim.CL_FSTVSA,CL_CCHINO = claim.CL_CCHINO,CL_CLMTYPE = claim.CL_CLMTYPE,CL_SRVC = claim.CL_SRVC,
                    CL_DEDREASON = claim.CL_DEDREASON,CL_INSPOLNO = claim.CL_INSPOLNO,CL_INSINSURD = claim.CL_INSINSURD,CL_PROVNAME = claim.CL_PROVNAME,CL_BATCH_STS = claim.CL_BATCH_STS,CL_TRFTUI = claim.CL_TRFTUI,CL_SUBOFF = claim.CL_SUBOFF,CL_FILENO = claim.CL_FILENO,CL_DEDMED = claim.CL_DEDMED,
                    CL_DEDPROV = claim.CL_DEDPROV,CL_FTYPE = claim.CL_FTYPE,CL_VATAMT = claim.CL_VATAMT,CL_VATNET = claim.CL_VATNET,SRV_DESC = claim.SRV_DESC,SERIAL = claim.SERIAL,GROSS_OR = claim.GROSS_OR,GROSS_LL = claim.GROSS_LL,NETPAYABLE_OR = claim.NETPAYABLE_OR,NETPAYABLE_LL = claim.NETPAYABLE_LL,
                    ADM_TYPE = claim.ADM_TYPE,POLICY_INC = claim.POLICY_INC,PROVIDER_CITY = claim.PROVIDER_CITY,EMERGENCY_CHK = claim.EMERGENCY_CHK,CONGINATAL_CHK = claim.CONGINATAL_CHK,PRE_DISEASE_CHK = claim.PRE_DISEASE_CHK,LOGINDATE = claim.LOGINDATE,
                    FROMDATE = claim.FROMDATE,TODATE = claim.TODATE,STATUS = claim.STATUS,});
            }
            var IpagedClaims = new PagedList<ClaimModel>(claimModels.AsQueryable(), searchModel.Page - 1, searchModel.Perpage);
            var model = new ClaimListModel()
            {
                Data = IpagedClaims.AsEnumerable()
            };
            model.Meta = new PageListMetadata
            {
                Page = IpagedClaims.PageIndex + 1,
                Perpage = IpagedClaims.PageSize,
                Pages = IpagedClaims.TotalPages,
                Sort = searchModel.Sort,
                Field = searchModel.Field,
                Total = IpagedClaims.TotalCount
            };
            return model;
        }

        public ReimbursmentClaimListModel PrepareReimbursmentClaimListModel(ReimbursmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var direction = searchModel.Sort.Equals("asc") ? false : true;
            var reimbursments = _customerService.GetReimbursments(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            var model = new ReimbursmentClaimListModel()
            {
                Data = reimbursments.Select(p => p)
            };
            model.Meta = new PageListMetadata
            {
                Page = reimbursments.PageIndex + 1,
                Perpage = reimbursments.PageSize,
                Pages = reimbursments.TotalPages,
                Sort = searchModel.Sort,
                Field = searchModel.Field,
                Total = reimbursments.TotalCount
            };
            return model;
        }
        public ProvidersListModel PrepareProvidersListModel(ProvidersSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var direction = searchModel.Sort.Equals("asc") ? false : true;
            var providers = _customerService.GetProviders(searchQuery: searchModel.Query, sortField: searchModel.Field, pageIndex: searchModel.Page - 1, pageSize: searchModel.Perpage, orderByDec: direction);
            var model = new ProvidersListModel()
            {
                Data = providers.Select(p => p)
            };
            model.Meta = new PageListMetadata
            {
                Page = providers.PageIndex + 1,
                Perpage = providers.PageSize,
                Pages = providers.TotalPages,
                Sort = searchModel.Sort,
                Field = searchModel.Field,
                Total = providers.TotalCount
            };
            return model;
        }
    }
}
