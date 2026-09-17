using UnityEngine;

namespace G.Core.Pool
{
    public interface IPoolable
    {
        GameObject GameObject { get; }
        Transform Transform { get; }

        void OnSpawned();
        void OnDespawned();
    }
}
