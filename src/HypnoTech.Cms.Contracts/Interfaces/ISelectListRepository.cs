using System;
using System.Collections.Generic;

namespace HypnoTech.Cms.Contracts.Interfaces
{
    public interface ISelectListRepository
    {
        List<object> GetSelectListItems(Type type);
    }
}
