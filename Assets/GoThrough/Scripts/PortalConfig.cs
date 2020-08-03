using GoThrough.Utility;
using UnityEngine;

namespace GoThrough
{
    [CreateAssetMenu]
    public class PortalConfig : ScriptableObjectSingleton<PortalConfig>
    {
        public int recursionMaxDepth = 5;
        public float nearClipOffset = 0.05f;
        public float nearClipLimit = 0.2f;
        public int maxRenderTextureAllocations = 100;
    }
}
