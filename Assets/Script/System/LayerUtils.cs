using UnityEngine;

namespace Game
{
    public class LayerUtils
    {
        public static bool IsLayerInMask(LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
