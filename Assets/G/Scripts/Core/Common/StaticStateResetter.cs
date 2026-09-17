using System;
using System.Reflection;
using UnityEngine;

namespace G.Core.Common
{
    public static class StaticStateResetter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAll()
        {
            Assembly assembly = typeof(StaticStateResetter).Assembly;

            foreach (Type type in assembly.GetTypes())
            {
                MethodInfo[] methods = type.GetMethods(
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach (MethodInfo method in methods)
                {
                    if (method.IsDefined(typeof(ResetOnPlayAttribute), inherit: false) == false)
                        continue;

                    if (method.GetParameters().Length > 0)
                    {
                        GameDebug.LogError($"[StaticStateResetter] {type.Name}.{method.Name} помечен ResetOnPlay, но имеет параметры.");
                        continue;
                    }

                    method.Invoke(null, null);
                }
            }
        }
    }
}
