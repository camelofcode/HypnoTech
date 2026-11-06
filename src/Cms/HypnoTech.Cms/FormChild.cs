using System;
using System.Reflection;

namespace HypnoTech.Cms
{
    public class FormChild
    {
        public MemberInfo TypeInfo { get; }

        public Type Type{ get; }

        public MemberInfo IdInfo { get; }

        public string DataStore { get; }

        public FormChild(MemberInfo typeInfo, MemberInfo idInfo, Type type, string dataStore)
        {
            TypeInfo = typeInfo;
            IdInfo = idInfo;
            DataStore = dataStore;
            Type = type;
        }
    }
}
