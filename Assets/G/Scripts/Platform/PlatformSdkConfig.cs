using UnityEngine;

namespace G.Platform
{
    public enum PlatformSdkKind
    {
        EditorStub = 0,
        Mobile = 1,
        Web = 2,
    }

    [CreateAssetMenu(fileName = "PlatformSdkConfig", menuName = "Configs/Platform SDK Config")]
    public class PlatformSdkConfig : ScriptableObject
    {
        [SerializeField] private PlatformSdkKind _kind = PlatformSdkKind.EditorStub;
        [SerializeField] private float _initializeTimeoutSeconds = 10f;

        public PlatformSdkKind Kind => _kind;

        public float InitializeTimeoutSeconds => _initializeTimeoutSeconds;
    }
}
