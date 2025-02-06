using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace Game
{
    public class PredictedRigidbody2D : NetworkBehaviour
    {
        [Header("Network Settings")]
        public Rigidbody2D Body;
        public float CorrectionScale = 10.0f;
        public float CorrectionThreshold = 0.1f;
        public float SendRate = 0.05f;

        private readonly struct State
        {
            public readonly Vector2 position;
            public readonly Vector2 velocity;
            public readonly double timestamp;

            public State(Vector2 pos, Vector2 vel, double time)
            {
                position = pos;
                velocity = vel;
                timestamp = time;
            }
        }

        private List<State> _stateHistory = new List<State>();
        private float _lastSentTime;
        private State? _targetState = null;

        void Update()
        {
            if (isOwned)
            {
                // Periodically send state to the server
                if (Time.time - _lastSentTime > SendRate)
                {
                    SendStateToServer();
                    _lastSentTime = Time.time;
                }
            }
        }

        void FixedUpdate()
        {
            if (!isOwned && _targetState.HasValue)
            {
                //We have correction to apply
                Interpolate();
            }
        }

        void SendStateToServer()
        {
            State currentState = new(Body.position, Body.velocity, NetworkTime.time);

            if (!isServer)
            {
                // Store the state in the history buffer and send it
                _stateHistory.Add(currentState);
                CmdSendStateToServer(currentState);
            }
            else
            {
                // Server simply replicate his state to all clients
                RpcSyncStateToClients(currentState);
            }
        }

        [Command]
        void CmdSendStateToServer(State clientState)
        {
            Body.position = clientState.position;
            Body.velocity = clientState.velocity;
            RpcSyncStateToClients(clientState);
        }

        [ClientRpc]
        void RpcSyncStateToClients(State serverState)
        {
            // If the host is also the local player, avoid redundant correction
            if (isServer && isOwned) return;

            ApplyStateCorrection(serverState);
        }

        private void ApplyStateCorrection(State serverState)
        {
            // we check our history state against the latest server state, and apply correction if needed
            if (isOwned)
            {
                _targetState = null;

                //We find the closest match to our last record history
                int index = BinarySearch(serverState.timestamp);
                if (index >= 0)
                {
                    if (Vector2.Distance(_stateHistory[index].position, serverState.position) > CorrectionThreshold)
                    {
                        _targetState = serverState;
                    }

                    _stateHistory.RemoveRange(0, index + 1);
                }
            }
            else
            {
                //this is not our player , we just get the latest state from the server
                _targetState = serverState;
            }
        }

        private void Interpolate()
        {
            //Here we interpolate the position and velocity of the rigidbody based on a scale factor
            //This will help us to correct client that drifted away from the server state

            State target = _targetState.Value;
            float lerpFactor = Time.fixedDeltaTime * CorrectionScale;

            Body.position = Vector2.Lerp(Body.position, target.position, lerpFactor);
            Body.velocity = Vector2.Lerp(Body.velocity, target.velocity, lerpFactor);

            if (Vector2.Distance(Body.position, target.position) < CorrectionThreshold)
            {
                Body.position = target.position;
                Body.velocity = target.velocity;
                _targetState = null;
            }
        }

        private int BinarySearch(double targetTimestamp)
        {
            int left = 0;
            int right = _stateHistory.Count - 1;
            int bestIndex = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                double diff = Mathf.Abs((float)(_stateHistory[mid].timestamp - targetTimestamp));

                if (diff < SendRate)
                {
                    bestIndex = mid;
                    right = mid - 1; // Search for an earlier match
                }
                else if (_stateHistory[mid].timestamp < targetTimestamp)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return bestIndex;
        }
    }
}
