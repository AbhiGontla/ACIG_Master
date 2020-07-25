using ACIGConsumer.Models.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACIGConsumer.Factories
{
    public interface IPolicyFactory
    {
        PolicyListModel PreparePolicyListModel(PolicySearchModel searchModel);
        
    }
}
