
using Mirror;
using UnityEngine;

namespace Game
{
    public class PickableLife : Pickable, IReset
    {
        public void Start()
        {
            SceneManager.Register<IReset>(this);
        }

        public void OnDestroy()
        {
            SceneManager.Remove<IReset>(this);
        }

        public override void OnPicked(GameObject picker)
        {
            base.OnPicked(gameObject);

            if (isServer)
            {
                //Add life
                if(picker.TryGetComponent<HealthAttribute>( out HealthAttribute health))
                {
                    health.SetHealth(1);
                }
            }
        }

        public void Reset()
        {
            if (isServer)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}