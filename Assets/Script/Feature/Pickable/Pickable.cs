
using Mirror;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof (Collider2D))]
    public class Pickable : NetworkBehaviour
    {
        public virtual void OnPicked(GameObject picker) 
        { 
            //Only server own bonus, so only server will destroy them
            if(isOwned)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}