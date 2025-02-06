
using Mirror;
using UnityEngine;

namespace Game
{
    public class PickableLife : Pickable
    {
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
    }
}