using System;
using System.Collections.Generic;

namespace HypnoTech.Common.Models
{
    public static class ModelRegistry
    {
        public static List<ModelRegistryMap> Registrations = new List<ModelRegistryMap>();

        public static void Register(Type type, string dataName, string displayName)
        {
            Registrations.Add(new ModelRegistryMap(type, dataName, displayName));
        }
    }

    public class ModelRegistryMap
    {
        public Type Type { get; }

        public string DataName { get; }

        public string DisplayName { get; }

        public ModelRegistryMap(Type type, string dataName, string displayName)
        {
            Type = type;
            DataName = dataName;
            DisplayName = displayName;
        }
    }
}

