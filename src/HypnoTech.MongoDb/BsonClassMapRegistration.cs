using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;

namespace HypnoTech.MongoDb
{
    public static class BsonClassMapRegistration
    {
        private static readonly object RegistrationLock = new object();
        private static readonly HashSet<Type> RegisteredTypes = new HashSet<Type>();

        public static void RegisterIfNeeded<TClass>(Action<BsonClassMap<TClass>> initializer)
        {
            lock (RegistrationLock)
            {
                if (RegisteredTypes.Contains(typeof(TClass)))
                {
                    return;
                }

                BsonClassMap.RegisterClassMap(initializer);
                RegisteredTypes.Add(typeof(TClass));
            }
        }
    }
}
