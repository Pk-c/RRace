using Mirror;
using UnityEngine;

namespace Game
{
    public class CameraFeature : NetworkBehaviour
    {
        [SerializeField]
        private float Speed = 5.0f;

        public override void OnStartAuthority()
        {
            CameraFollow follow = Camera.main.gameObject.AddComponent<CameraFollow>();
            follow.Init(transform, Speed);
            base.OnStartAuthority();
        }
    }
}