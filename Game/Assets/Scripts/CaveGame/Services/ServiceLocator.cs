using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaveGame.Services
{
    public static class ServiceLocator
    {
        public static Dictionary<Type, IService> Services { get; private set; } = new();

        public static void Register<T>(IService service) where T : IService
        {
            if (Services.ContainsKey(typeof(T)))
            {
                throw new Exception($"Type {typeof(T).Name} already registered");
            }

            Services.Add(typeof(T), service);
        }

        public static void Unregister<T>() where T : IService
        {
            if (!Services.ContainsKey(typeof(T)))
            {
                throw new Exception($"Type {typeof(T).Name} is not registered");
            }

            Services.Remove(typeof(T));
        }

        public static T Get<T>() where T : IService
        {
            if (!Services.ContainsKey(typeof(T)))
            {
                throw new Exception($"Type {typeof(T).Name} is not registered");
            }

            return (T)Services[typeof(T)];
        }
    }
}