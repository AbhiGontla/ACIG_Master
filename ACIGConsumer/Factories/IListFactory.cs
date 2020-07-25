using ACIGConsumer.Models.Approvals;
using ACIGConsumer.Models.Claims;
using ACIGConsumer.Models.CoverageLimits;
using ACIGConsumer.Models.Policy;
using ACIGConsumer.Models.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Factories
{
    public interface IListFactory
    {
        PolicyListModel PreparePolicyListModel(PolicySearchModel searchModel);
        ApprovalListModel PrepareApprovalListModel(ApprovalSearchModel searchModel);
        CoverageBalanceListModel PrepareCoverageBalanceListModel(CoverageBalanceSearchModel searchModel);
        ClaimListModel PrepareClaimListModel(ClaimSearchModel searchModel);
        ReimbursmentClaimListModel PrepareReimbursmentClaimListModel(ReimbursmentSearchModel searchModel);
        ProvidersListModel PrepareProvidersListModel(ProvidersSearchModel searchModel);
    }
}
