using Core.Api;
using Core.Domain;
using Core.Domain.Customer;
using Core.Infrastructure;
using Data;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Services
{
    public class CustomerService : ICustomerService
    {
        private IUnitOfWorks _unitOfWorks;

        public CustomerService(IUnitOfWorks unitOfWorks)
        {
            _unitOfWorks = unitOfWorks;
        }

        public Registration GetCustomerById(string NId)
        {
            var _customers = _unitOfWorks.RegistrationRepository.GetDbSet();
            return _customers.Where(c => (c.Iqama_NationalID == NId)).FirstOrDefault();
        }

        public int Insert(MRClient client)
        {
            try
            {
                _unitOfWorks.ClientRepository.Insert(client);
                _unitOfWorks.Save();
                int id = client.Id;
                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void Insert(Registration _userregistration)
        {
            try
            {
                
                _unitOfWorks.RegistrationRepository.Insert(_userregistration);
                _unitOfWorks.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region OSClaims
        public void Insert(List<OSClaims> OSClaims)
        {
            for (int i = 0; i < OSClaims.Count; i++)
            {
                _unitOfWorks.OSClaimsRepository.Insert(OSClaims[i]);
                _unitOfWorks.Save();
            }
        }

        public List<OSClaims> GetOSClaimsByNationalId(string NId)
        {
            var res = _unitOfWorks.OSClaimsRepository.GetDbSet();
            return res.Where(c => (c.NationalID == NId)).ToList();
        }
        #endregion

        #region PaidClaims
        public void Insert(List<PaidClaims> PaidClaims)
        {
            for (int i = 0; i < PaidClaims.Count; i++)
            {
                _unitOfWorks.PaidClaimsRepository.Insert(PaidClaims[i]);
                _unitOfWorks.Save();
            }
        }

        public List<PaidClaims> GetPaidClaimsByNationalId(string NId)
        {
            var res = _unitOfWorks.PaidClaimsRepository.GetDbSet();
            return res.Where(c => (c.NationalID == NId)).ToList();
        }
        #endregion

        #region Providers

        #region InsertProviders
        public void Insert(List<Providers> Providers)
        {
            for (int i = 0; i < Providers.Count; i++)
            {
                _unitOfWorks.ProvidersRepository.Insert(Providers[i]);
                _unitOfWorks.Save();
            }
        }

        #region GetProviderseById
        public List<Providers> GetProvidersByNationalId(string NId)
        {
            var res = _unitOfWorks.ProvidersRepository.GetDbSet();
            return res.Where(c => (c.NationalID == NId)).ToList();
        }


        #endregion
        #endregion

        #endregion

        #region CoverageBalances

        #region InsertCoverageBalances
        public void Insert(List<CoverageBalance> coverageBalances)
        {
            for (int i = 0; i < coverageBalances.Count; i++)
            {
                _unitOfWorks.CoverageBalanceRepository.Insert(coverageBalances[i]);
                _unitOfWorks.Save();
            }
        }

        #region GetCoverageBalanceById
        public List<CoverageBalance> GetCovBalsByNationalId(string NId)
        {
            var res = _unitOfWorks.CoverageBalanceRepository.GetDbSet();
            return res.Where(c => (c.NationalID == NId)).ToList();
        }
        #endregion
        #endregion

        #endregion
        #region GetPoliciesByNationalId
        public List<Policies> GetPoiciesByNationalId(string nationalID)
        {
            Registration _userdetails = GetCustomerById(nationalID);
            var res = _unitOfWorks.PoliciesRepository.GetDbSet();
            return res.Where(c => (c.PolicyNumber == _userdetails.PolicyNo) && (c.TushfaMemberNo == _userdetails.TushfaMemberNo)).ToList();
        }
        #endregion
        #region Approvals

        #region InsertApprovals
        public void Insert(List<Approvals> approvals)
        {
            for (int i = 0; i < approvals.Count; i++)
            {
                _unitOfWorks.ApprovalsRepository.Insert(approvals[i]);
                _unitOfWorks.Save();
            }

        }
        #endregion

        #region GetApprovalsByNationalId
        public List<Approvals> GetApprovalsByNationalId(string NId)
        {
            var res = _unitOfWorks.ApprovalsRepository.GetDbSet();
            return res.Where(c => (c.NationalId == NId)).ToList();
        }
        #endregion

        #endregion
        public Registration ValidateCustomer(string NId, string Pin)
        {
            var _customers = _unitOfWorks.RegistrationRepository.GetDbSet();
            return _customers.Where(c => (c.Iqama_NationalID == NId) && (c.ConfirmPin == Pin)).FirstOrDefault();
        }
        public MRClient GetClientByNationalId(string NId)
        {
            var res = _unitOfWorks.ClientRepository.GetDbSet();
            return res.Where(c => (c.IDNumber == NId)).FirstOrDefault();
        }
        public int Insert(MRRequest _reclaims)
        {
            _unitOfWorks.ReimbursmentRepository.Insert(_reclaims);
            _unitOfWorks.Save();
            int id = _reclaims.Id;
            return id;
        }
        public IPagedList<Policies> GetPoicies(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.PoliciesRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.Iqama_NationalID.Contains(searchQuery)
                || q.MemberName.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(Policies).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var policyList = new PagedList<Policies>(query, pageIndex, pageSize);
            return policyList;
        }
        public IPagedList<Approvals> GetApprovals(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.ApprovalsRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.NationalId.Contains(searchQuery)
                || q.CL_NAME.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(Approvals).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var approvalList = new PagedList<Approvals>(query, pageIndex, pageSize);
            return approvalList;
        }
        public IPagedList<CoverageBalance> GetCoverageBalances(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.CoverageBalanceRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.NationalID.Contains(searchQuery)
                || q.InsPolicyNo.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(CoverageBalance).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var coverageBalanceList = new PagedList<CoverageBalance>(query, pageIndex, pageSize);
            return coverageBalanceList;
        }
        public IPagedList<PaidClaims> GetPaidClaims(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.PaidClaimsRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.NationalID.Contains(searchQuery)
                || q.InsPolicyNo.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(PaidClaims).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var paidClaimList = new PagedList<PaidClaims>(query, pageIndex, pageSize);
            return paidClaimList;
        }
        public IPagedList<OSClaims> GetOSClaims(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.OSClaimsRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.NationalID.Contains(searchQuery)
                || q.InsPolicyNo.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(OSClaims).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var osClaimList = new PagedList<OSClaims>(query, pageIndex, pageSize);
            return osClaimList;
        }
        public IPagedList<MRRequest> GetReimbursments(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.ReimbursmentRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.MemberName.Contains(searchQuery)
                || q.PolicyNumber.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(MRRequest).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var reClaimList = new PagedList<MRRequest>(query, pageIndex, pageSize);
            return reClaimList;
        }
        public IPagedList<Providers> GetProviders(string searchQuery, string sortField, int pageIndex = 0, int pageSize = int.MaxValue, bool orderByDec = false)
        {
            var query = _unitOfWorks.ProvidersRepository.GetDbSet();
            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(q => q.ProviderName.Contains(searchQuery)
                || q.ProviderNumber.Contains(searchQuery));
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                PropertyInfo prop = typeof(Providers).GetProperty(sortField.FirstCharToUpper());
                if (prop != null)
                    if (!orderByDec)
                        query = query.OrderBy(x => prop.GetValue(x, null));
                    else if (orderByDec)
                        query = query.OrderByDescending(x => prop.GetValue(x, null));
            }
            var providerlist = new PagedList<Providers>(query, pageIndex, pageSize);
            return providerlist;
        }
        public List<MRClaimType> GetClaimTypes()
        {
            var query = _unitOfWorks.ClaimTypeRepository.GetDbSet();
            return query.ToList();
        }
        public List<BankMaster> GetBankNames()
        {
            return _unitOfWorks.BankRepository.GetDbSet().ToList();
        }
        public List<Registration> GetAllCustomers()
        {
            var q = _unitOfWorks.RegistrationRepository.GetDbSet();
            if (q != null)
                return q.ToList();
            else return new List<Registration>();
        }
        #region TOB

        public void Insert(TOB tOB)
        {
            _unitOfWorks.TOBRepository.Insert(tOB);
            _unitOfWorks.Save();
        }

        public TOB GetTOB(string PolicyNumber, string classcode)
        {
            var tobdetails = _unitOfWorks.TOBRepository.GetDbSet();
            return tobdetails.Where(c => (c.PolicyNo.ToString() == PolicyNumber) && (c.ClassCode == classcode)).FirstOrDefault();
        }

        public void Insert(TOBlist tOBlist)
        {
            _unitOfWorks.TOBListRepository.Insert(tOBlist);
            _unitOfWorks.Save();
        }

        public List<TOBlist> GetTOBList(string classname)
        {
            var tobdetails = _unitOfWorks.TOBListRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }

        public void Insert(Inpatient inpatient)
        {
            _unitOfWorks.InpatientRepository.Insert(inpatient);
            _unitOfWorks.Save();
        }

        public List<Inpatient> GetInpatientList(string classname)
        {
            var tobdetails = _unitOfWorks.InpatientRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }

        public void Insert(Outpatient outpatient)
        {
            _unitOfWorks.OutpatientRepository.Insert(outpatient);
            _unitOfWorks.Save();
        }

        public List<Outpatient> GetOutpatientList(string classname)
        {
            var tobdetails = _unitOfWorks.OutpatientRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }

        public void Insert(MaternityBenefit maternityBenefit)
        {
            _unitOfWorks.MaternityBenefitRepository.Insert(maternityBenefit);
            _unitOfWorks.Save();
        }

        public List<MaternityBenefit> GetMaternityBenefitList(string classname)
        {
            var tobdetails = _unitOfWorks.MaternityBenefitRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }

        public void Insert(DentalBenefit dentalBenefit)
        {
            _unitOfWorks.DentalBenefitRepository.Insert(dentalBenefit);
            _unitOfWorks.Save();
        }

        public List<DentalBenefit> GetDentalBenefitList(string classname)
        {
            var tobdetails = _unitOfWorks.DentalBenefitRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }

        public void Insert(ReimbursementClaim reimbursementClaim)
        {
            _unitOfWorks.ReimbursementClaimRepository.Insert(reimbursementClaim);
            _unitOfWorks.Save();
        }

        public List<ReimbursementClaim> GetReimbursementClaimList(string classname)
        {
            var tobdetails = _unitOfWorks.ReimbursementClaimRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }

        public void Insert(AdditionalBenefit additionalBenefit)
        {
            _unitOfWorks.AdditionalBenefitRepository.Insert(additionalBenefit);
            _unitOfWorks.Save();
        }

        public List<AdditionalBenefit> GetAdditionalBenefitList(string classname)
        {
            var tobdetails = _unitOfWorks.AdditionalBenefitRepository.GetDbSet();
            return tobdetails.Where(c => (c.ClassName == classname)).ToList();
        }



        #endregion

        public void InsertRelations(Relations relation)
        {
            _unitOfWorks.RelationsRepository.Insert(relation);
            _unitOfWorks.Save();
        }
        public IEnumerable<Relations> GetRelationsByRegistrationId(int id)
        {
            var query = _unitOfWorks.RelationsRepository.GetAll();
            return query.Where(r => r.RegistrationId.Equals(id));
        }
        public void Insert(MRRequestStatusLog mRRequestStatusLog)
        {
            _unitOfWorks.RequestStatusLogRepository.Insert(mRRequestStatusLog);
            _unitOfWorks.Save();
        }

        public void Insert(MRRequestFile requestFile)
        {
            _unitOfWorks.RequestFileRepository.Insert(requestFile);
            _unitOfWorks.Save();
        }

        public void UpdateRequestNumber(MRRequest request)
        {
            _unitOfWorks.ReimbursmentRepository.Update(request);
            _unitOfWorks.Save();
        }

        public MRRequest GetReimbursmentClaimById(string ID)
        {
            int id = Convert.ToInt32(ID);
            return _unitOfWorks.ReimbursmentRepository.Get(c => c.Id == id);
        }
        public List<MRRequest> GetReimByClientId(string clientId)
        {
            if (clientId != null)
            {
                int clintId = Convert.ToInt32(clientId);
                var res = _unitOfWorks.ReimbursmentRepository.GetDbSet();
                return res.Where(c => (c.ClientId == clintId)).ToList();
            }
            else
            {
                return null;
            }
        }

        public void UpdateRequestStatus(MRRequest request)
        {
            _unitOfWorks.ReimbursmentRepository.Update(request);
            _unitOfWorks.Save();
        }

        #region InsertPolicies
        public void Insert(Policies policies)
        {
            _unitOfWorks.PoliciesRepository.Insert(policies);
            _unitOfWorks.Save();
        }
        #endregion
        public bool CheckIfUserExists(string nid, string dob = null)
        {
            bool status = true;
            try
            {
                var usersList = _unitOfWorks.RegistrationRepository.GetDbSet();
                var userdetails = usersList.Where(c => (c.Iqama_NationalID == nid));
                if (dob != null)
                {
                    userdetails = userdetails.Where(c => c.DOB.Equals(dob));
                }
                if (userdetails != null && userdetails.Count() > 0)
                {
                    return status;
                }
                else
                {
                    status = false;
                }
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
    }
}
