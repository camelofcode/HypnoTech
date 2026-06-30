using System.Collections.Generic;

namespace HypnoTech.Cms.FormMemberImplementations
{
    public interface IProvidesState
    {
        List<string> HiddenFields { get; }
    }
}
