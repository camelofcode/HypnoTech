using System;
using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Common.Models
{
    public static class ModelRegistry
    {
        private static readonly object RegistrationLock = new object();

        public static List<ModelRegistryMap> Registrations = new List<ModelRegistryMap>();

        public static void Register(Type type, string dataName, string displayName)
        {
            lock (RegistrationLock)
            {
                if (Registrations.Any(registration =>
                        registration.Type == type &&
                        registration.DataName == dataName &&
                        registration.DisplayName == displayName))
                {
                    return;
                }

                Registrations.Add(new ModelRegistryMap(type, dataName, displayName));
            }
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

