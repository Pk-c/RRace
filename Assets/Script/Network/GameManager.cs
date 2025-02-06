using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class GameManager : NetworkBehaviour
    {
        public enum GameState
        {
            Running,
            Reseting,
            Restart,
            CountDown
        }

        public BoxCollider2D FinishBox;
        public TextMeshProUGUI Information;

        private GameState State = GameState.Running;

        public void Update()
        {
            // Only server check game rules

            if (!isServer) return; 

            switch (State)
            {
                case GameState.Running:

                    foreach (NetworkConnectionToClient identity in NetworkServer.connections.Values)
                    {
                        if (identity.identity != null)
                        {
                            var player = identity.identity;

                            // If a player dies, they lose immediately
                            if (player.GetComponent<HealthAttribute>().IsDead)
                            {
                                RpcShowFinishText(identity, Color.red, "YOU ARE DEAD");
                                SetPlayerInput(identity, false);

                            }

                            // If a player finishes the race, they win and all others lose
                            if (FinishBox.bounds.Contains(player.transform.position))
                            {
                                RpcShowFinishText(identity, Color.green, "YOU WON"); // Winner
                                NotifyOthersOfLoss(identity); // Make others lose
                                State = GameState.Reseting;
                                StartCoroutine(WaitForReset());
                                return; // End the check after we have a winner
                            }
                        }
                    }
                    break;

                case GameState.Restart:

                    //Reset Game
                    List<IReset> resetable = SceneManager.GetObjects<IReset>();
                    for(int i = 0; i < resetable.Count; i++)
                    {
                        resetable[i].Reset();
                    }
        
                    //Respawn player
                    foreach (NetworkConnectionToClient client in NetworkServer.connections.Values)
                    {
                        if (client != null && client.identity != null)
                        {
                            //In a host setup (server & client on same instance): The server doesn't have a separate client connection for the local player
                            //So we have to do an exception for this case, this would need better encapsulation
                            if (client.identity.isLocalPlayer)
                            {
                                client.identity.transform.position = NetworkManager.singleton.GetStartPosition().position;
                            }
                            else
                            {
                                TargetRespawnPlayer(client, NetworkManager.singleton.GetStartPosition().position);
                            }

                            RpcShowFinishText(client, Color.white, "");
                        }
                    }

                    //Ready let's go again
                    State = GameState.CountDown;
                    StartCoroutine(CountDown());

                    break;
            }
        }


        private void NotifyOthersOfLoss(NetworkConnectionToClient winner)
        {
            
            foreach (NetworkConnectionToClient identity in NetworkServer.connections.Values)
            {
                if (identity != winner) // Make sure we don't notify the winner
                {
                    RpcShowFinishText( identity, Color.red, "YOU LOST");
                    SetPlayerInput(identity, false);
                }
            }
        }

        private IEnumerator CountDown()
        {
            //Insert a little delay to let everybody respawn properly and wait for their camera
            yield return new WaitForSeconds(1f);
            foreach (NetworkConnectionToClient identity in NetworkServer.connections.Values)
            {
                SetPlayerInput(identity, true);
            }
            State = GameState.Running;
        }

        private IEnumerator WaitForReset()
        {
            //Let winner savour victory for 3 seconds...
            yield return new WaitForSeconds(3f);
            State = GameState.Restart;
        }

        [Server]
        private void SetPlayerInput(NetworkConnectionToClient client, bool enabled)
        {
            if( client.identity.TryGetComponent<PlayerInputController>( out PlayerInputController controller))
            {
                controller.SetInputEnabled(enabled);
            }
        }

        //RPCS

        [TargetRpc]
        public void RpcShowFinishText(NetworkConnectionToClient player, Color col, string text)
        {
            Information.color = col;
            Information.text = text;
        }

        [TargetRpc]
        public void TargetRespawnPlayer(NetworkConnectionToClient player, Vector3 newPosition)
        {
            //Get local player and reset position
            NetworkClient.localPlayer.transform.position = newPosition;

            //Reset everything client side
            List<IReset> resetable = SceneManager.GetObjects<IReset>();
            for (int i = 0; i < resetable.Count; i++)
            {
                if(resetable[i] != null )
                    resetable[i].Reset();
            }
        }
    }
}
