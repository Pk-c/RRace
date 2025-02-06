
using Mirror;
using UnityEngine;

namespace Game
{
    public class PickableLife : Pickable
    {
        public override void OnPicked(GameObject picker)
        {
            base.OnPicked(gameObject);

            if (isOwned)
            {
                //Add life
            }
        }
    }
}