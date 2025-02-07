using Mirror;
using UnityEngine;

namespace Game
{
    public class BonusSpawner : NetworkBehaviour, IReset
    {
        [SerializeField]
        private Vector3 SpawnPosition = Vector3.zero;
        [SerializeField]
        private GameObject ToSpawn = null;
        [SerializeField]
        private LayerMask TriggerMask;

        private bool _activated = true;

        [SyncVar]
        private bool _bumped = false;

        public void Start()
        {
            SceneManager.Register<IReset>(this);
        }

        public void OnDestroy()
        {
            SceneManager.Remove<IReset>(this);
        }

        public void OnCollisionEnter2D(Collision2D collision)
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

        private void SpawnBonus()
        {
            _activated = false;
            _bumped = true;

            //Ideally should be pooled.
            GameObject spawnedObject = Instantiate(ToSpawn, transform.position + SpawnPosition, Quaternion.identity);
            
            //Server object need to have a owner to execute their behavior
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

        public void Reset()
        {
            if (isServer)
            {
                _bumped = false;
            }
           
            _activated = true;
            for (int n = 0; n < transform.childCount; ++n)
            {
                transform.GetChild(n).gameObject.SetActive(true);
            }            
        }
    }
}