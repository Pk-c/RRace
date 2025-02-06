using Mirror;
using UnityEngine;

namespace Game
{
    public class BonusSpawner : NetworkBehaviour
    {
        public Vector3 SpawnPosition = Vector3.zero;
        public GameObject ToSpawn = null;
        public LayerMask TriggerMask;

        private bool _activated = true;

        [SyncVar]
        private bool _bumped = false;

        public void OnCollisionEnter2D(UnityEngine.Collision2D collision)
        {
            if (_activated && ToSpawn != null && collision.gameObject != null)
            {
                if (LayerUtils.IsLayerInMask(TriggerMask, collision.gameObject.layer))
                {
                    ContactPoint2D contact = collision.GetContact(0);
                    if (contact.normal.y > 0.5f)
                    {
                        Deactivate();

                        if (isServer)
                        {
                            SpawnBonus();
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (_activated && _bumped)
            {
                Deactivate();
            }
        }


        private void Reset()
        {
            _activated = true;
            _bumped = false;
        }


        private void SpawnBonus()
        {
            _activated = false;
            _bumped = true;
            GameObject spawnedObject = Instantiate(ToSpawn, transform.position + SpawnPosition, Quaternion.identity);
            //I use the loopback connection because mirror need a owner for object living on server
            NetworkServer.Spawn(spawnedObject, NetworkServer.localConnection);
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + SpawnPosition,0.5f);
        }
#endif

        public void Deactivate()
        {
            _activated = false;
            for (int n = 0; n < transform.childCount; ++n)
            {
                transform.GetChild(n).gameObject.SetActive(false);
            }
        }
    }
}