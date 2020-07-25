using ACIGConsumer.Models.Approvals;
using Core.Infrastructure;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Factories
{
    public class ApprovalFactory : IApprovalFactory
    {
        private readonly ICustomerService _customerService;
        public ApprovalFactory(ICustomerService customerService)
        {
            this._customerService = customerService;
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
    }
}
