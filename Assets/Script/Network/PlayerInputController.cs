using Mirror;
using UnityEngine;

namespace Game
{
    public class PlayerInputController : NetworkBehaviour
    {
        //Simple input controller to lock / unlock player input

        [SyncVar]
        private bool _inputEnabled = true;
        public bool InputEnabled { get { return _inputEnabled; } }

        [Server]
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
        }
    }
}