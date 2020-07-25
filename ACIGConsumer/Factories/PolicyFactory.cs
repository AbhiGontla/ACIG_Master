using ACIGConsumer.Models.Policy;
using Core.Api;
using Core.Infrastructure;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Factories
{
    public class PolicyFactory : IPolicyFactory
    {
        private readonly ICustomerService _customerService;
        public PolicyFactory(ICustomerService customerService)
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
						PolicyNumber = p.PolicyNumber,PolicyFromDate = p.PolicyFromDate,PolicyToDate = p.PolicyToDate,PolicyType = p.PolicyType,ClientType = p.ClientType,ClassCode = p.ClassCode,
						ClassName = p.ClassName,NetworkCode = p.NetworkCode,NetworkName = p.NetworkName,TPAID = p.TPAID,TPAName = p.TPAName,ClientName = p.ClientName,PolicySponsorName = p.PolicySponsorName,
						BrokerName = p.BrokerName,SponsorID = p.SponsorID,Iqama_NationalID = p.Iqama_NationalID,MemberName = p.MemberName,MobileNumber = p.MobileNumber,Gender = p.Gender,MaritalStatus = p.MaritalStatus,
						DOBGreg = p.DOBGreg,DOBHijri = p.DOBHijri,IDExpiryDate = p.IDExpiryDate,OccupationCode = p.OccupationCode,OccupationDesc = p.OccupationDesc,NationalityCode = p.NationalityCode,NationalityDesc = p.NationalityDesc,
						MemberTypeCode = p.MemberTypeCode,MemberTypeDesc = p.MemberTypeDesc,RelationCode = p.RelationCode,RelationDesc = p.RelationDesc,EmployeeNo = p.EmployeeNo,MemberStatus = p.MemberStatus,TushfaMemberNo = p.TushfaMemberNo,
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
    }
}
