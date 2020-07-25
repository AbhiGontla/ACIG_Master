﻿using Core;
using Core.Api;
using Core.Content;
using Core.Domain;
using Core.Domain.Customer;
using Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IUnitOfWorks
    {
        void Save();

        Task SaveAsync();

        void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity;
        IRepository<Customer> CustomerRepository { get; }
        IRepository<Registration> RegistrationRepository { get; }
        IRepository<Approvals> ApprovalsRepository { get; }
        IRepository<Policies> PoliciesRepository { get; }
        IRepository<CoverageBalance> CoverageBalanceRepository { get; }
        IRepository<Providers> ProvidersRepository { get; }
        IRepository<PaidClaims> PaidClaimsRepository { get; }
        IRepository<OSClaims> OSClaimsRepository { get; }
        IRepository<MRClient> ClientRepository { get; }
        IRepository<MRRequest> ReimbursmentRepository { get; }
        IRepository<Banners> BannersRepository { get; }
        IRepository<MRClaimType> ClaimTypeRepository { get; }
        IRepository<BankMaster> BankRepository { get; }
        IRepository<Articles> ArticlesRepository { get; }

        #region TOB

        IRepository<TOBlist> TOBListRepository { get; }
        IRepository<TOB> TOBRepository { get; }
        IRepository<Inpatient> InpatientRepository { get; }
        IRepository<Outpatient> OutpatientRepository { get; }
        IRepository<MaternityBenefit> MaternityBenefitRepository { get; }
        IRepository<DentalBenefit> DentalBenefitRepository { get; }
        IRepository<ReimbursementClaim> ReimbursementClaimRepository { get; }
        IRepository<AdditionalBenefit> AdditionalBenefitRepository { get; }

        #endregion
        IRepository<Relations> RelationsRepository { get; }
        IRepository<MRRequestStatusLog> RequestStatusLogRepository { get; }
        IRepository<MRRequestFile> RequestFileRepository { get; }
    }
}
