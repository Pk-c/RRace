using Game;
using Mirror;
using TMPro;
using UnityEngine;

namespace GameManager
{
    public class GameManager : NetworkBehaviour
    {
        public BoxCollider2D _FinishBox;
        public TextMeshProUGUI _Information;

        public void Start()
        {
            _Information.gameObject.SetActive(false);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.OnConnectedEvent += OnPlayerConnected;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            NetworkServer.OnConnectedEvent -= OnPlayerConnected;
        }

        public void OnPlayerConnected(NetworkConnectionToClient client)
        {
            if (client.identity != null)
            {
                SetPlayerInput(client, true);
            }
        }

        [Server]
        private void SetPlayerInput(NetworkConnectionToClient client, bool enabled)
        {
            var inputController = client.identity.GetComponent<PlayerInputController>();
            if (inputController != null)
            {
                inputController.SetInputEnabled(enabled);
            }
        }

        public void Update()
        {
            if (!isServer) return; // Only run on the server

            foreach (NetworkConnectionToClient identity in NetworkServer.connections.Values)
            {
                if (identity.identity != null)
                {
                    var player = identity.identity;

                    // If a player dies, they lose immediately
                    if (player.GetComponent<HealthAttribute>().IsDead)
                    {
                        RpcShowFinishText(identity, false);
                        SetPlayerInput(identity, false);

                    }

                    // If a player finishes the race, they win and all others lose
                    if (_FinishBox.bounds.Contains(player.transform.position))
                    {
                        RpcShowFinishText(identity, true); // Winner
                        NotifyOthersOfLoss(identity); // Make others lose
                        return; // End the check after we have a winner
                    }
                }
            }
        }


        [TargetRpc]
        public void RpcShowFinishText(NetworkConnectionToClient target, bool win)
        {
            if (win)
            {
                _Information.text = "YOU WIN";
                _Information.color = Color.green;
            }
            else
            {
                _Information.text = "YOU LOSE";
                _Information.color = Color.red;
            }

            _Information.gameObject.SetActive(true);
        }

        private void NotifyOthersOfLoss(NetworkConnectionToClient winner)
        {
            foreach (NetworkConnectionToClient identity in NetworkServer.connections.Values)
            {
                if (identity != winner) // Make sure we don't notify the winner
                {
                    RpcShowFinishText(identity, false);
                    SetPlayerInput(identity, false);
                }
            }
        }
    }
}
