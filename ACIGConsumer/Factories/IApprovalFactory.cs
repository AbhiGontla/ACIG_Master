using ACIGConsumer.Models.Approvals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Factories
{
    public interface IApprovalFactory
    {
        ApprovalListModel PrepareApprovalListModel(ApprovalSearchModel searchModel);
    }
}
