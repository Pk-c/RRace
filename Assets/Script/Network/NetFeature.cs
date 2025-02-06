using Mirror;

namespace Game
{
    public class NetFeature : NetworkBehaviour
    {
        public void Update()
        {
            SharedUpdate();

            if (isLocalPlayer)
            {
                OwnerUpdate();
            }
            else
            {
                RemoteUpdate();
            }
        }

        public void FixedUpdate()
        {
            SharedFixedUpdate();

            if (isLocalPlayer)
            {
                OwnerFixedUpdate();
            }
            else
            {
                RemoteFixedUpdate();
            }
        }

        protected virtual void SharedUpdate() { }
        protected virtual void OwnerUpdate() { }
        protected virtual void RemoteUpdate() { }

        protected virtual void SharedFixedUpdate() { }
        protected virtual void OwnerFixedUpdate() { }
        protected virtual void RemoteFixedUpdate() { }
    }
}