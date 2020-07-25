using Core.Api;
using Core.Domain;
using Core.Domain.Customer;
using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interfaces
{
    public interface ICustomerService
    {
        public int Insert(MRClient customer);

        public Registration ValidateCustomer(string NId,string Pin);
        public Registration GetCustomerById(string NId);

        public void Insert(Registration _userregistration);
        int Insert(MRRequest _reclaims);
        MRClient GetClientByNationalId(string NId);
        void Insert(List<OSClaims> OSClaims);
        List<OSClaims> GetOSClaimsByNationalId(string NId);
        void Insert(List<PaidClaims> PaidClaims);
        List<PaidClaims> GetPaidClaimsByNationalId(string NId);
        void Insert(List<Providers> Providers);
        List<Providers> GetProvidersByNationalId(string NId);
        void Insert(List<CoverageBalance> coverageBalances);
        List<CoverageBalance> GetCovBalsByNationalId(string NId);
        List<Policies> GetPoiciesByNationalId(string nationalID);
        void Insert(List<Approvals> approvals);
        List<Approvals> GetApprovalsByNationalId(string NId);
        IPagedList<Policies> GetPoicies(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        IPagedList<Approvals> GetApprovals(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        IPagedList<CoverageBalance> GetCoverageBalances(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        IPagedList<OSClaims> GetOSClaims(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        IPagedList<PaidClaims> GetPaidClaims(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        IPagedList<MRRequest> GetReimbursments(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        IPagedList<Providers> GetProviders(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false);
        List<MRClaimType> GetClaimTypes();
        List<BankMaster> GetBankNames();
        List<Registration> GetAllCustomers();
        #region TOB
        public void Insert(TOB tOB);

        public TOB GetTOB(string PolicyNumber, string classcode);
        public void Insert(TOBlist tOBlist);
        public List<TOBlist> GetTOBList(string classname);
        public void Insert(Inpatient inpatient);
        public List<Inpatient> GetInpatientList(string classname);
        public void Insert(Outpatient outpatient);
        public List<Outpatient> GetOutpatientList(string classname);
        public void Insert(MaternityBenefit maternityBenefit);
        public List<MaternityBenefit> GetMaternityBenefitList(string classname);
        public void Insert(DentalBenefit dentalBenefit);
        public List<DentalBenefit> GetDentalBenefitList(string classname);
        public void Insert(ReimbursementClaim reimbursementClaim);
        public List<ReimbursementClaim> GetReimbursementClaimList(string classname);
        public void Insert(AdditionalBenefit additionalBenefit);
        public List<AdditionalBenefit> GetAdditionalBenefitList(string classname);

        #endregion
        IEnumerable<Relations> GetRelationsByRegistrationId(int id);
        public void Insert(MRRequestStatusLog mRRequestStatusLog);
        public void Insert(MRRequestFile requestFile);
        public MRRequest GetReimbursmentClaimById(string id);
        public void UpdateRequestNumber(MRRequest request);
        public List<MRRequest> GetReimByClientId(string id);
        public void UpdateRequestStatus(MRRequest request);
        public void Insert(Policies policies);
        bool CheckIfUserExists(string nid, string dob = null);
        void InsertRelations(Relations relation);
        Registration GetRegistrationByNationalId(string nationalId, string dob = null);
    }
}
