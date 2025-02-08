
using Mirror;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof (Collider2D))]
    public class Pickable : NetworkBehaviour
    {
        public virtual void OnPicked(GameObject picker) 
        { 
            //Only owner of the bonus will destroy it ( in our case it's the server )
            if(isOwned)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}