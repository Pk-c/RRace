using UnityEngine;
using Mirror;
using System;

namespace Game
{
    public class PredictedRigidbody2D : NetworkBehaviour
    {
        [Header("Network Settings")]
        [SerializeField]
        private Rigidbody2D Body;
        [SerializeField]
        private float CorrectionScale = 10.0f;
        [SerializeField]
        private float CorrectionThreshold = 0.1f;
        [SerializeField]
        private float SendRate = 0.05f;
        [SerializeField]
        private bool ServerOwner = false;

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

        private const int MaxHistorySize = 30;
        private State[] _stateHistory = new State[MaxHistorySize];
        private int _historyCount = 0;
        private int _historyStartIndex = 0;
        private float _lastSentTime;
        private State? _targetState = null;
        private double _lastRecievedTime = 0;
        private bool _bodyUpdated = false;

        public override void OnStartServer()
        {
            if (ServerOwner)
            {
                NetworkIdentity identity = GetComponent<NetworkIdentity>();
                identity.AssignClientAuthority(NetworkServer.localConnection);
            }

            _lastRecievedTime = NetworkTime.time;

            base.OnStartServer();
        }

        public override void OnStartClient()
        {
            _lastRecievedTime = NetworkTime.time;

            base.OnStartClient();
        }

        void Update()
        {
            if (isOwned)
            {
                if (Time.time - _lastSentTime > SendRate)
                {
                    //Client owned send state to the server
                    if (!isServer)
                    {
                        SendStateToServer();
                    }
                    else
                    {
                        //Server owned send state to client
                        State currentState = new State(Body.position, Body.velocity, NetworkTime.time);
                        RpcSyncStateToClients(currentState);
                    }

                    _lastSentTime = Time.time;
                }
            }
            else if( isServer && _bodyUpdated )
            {
                //Send player data back to all client
                State currentState = new State(Body.position, Body.velocity, NetworkTime.time);
                RpcSyncStateToClients(currentState);
                _bodyUpdated = false;
            }
        }

        void FixedUpdate()
        {           
            if (_targetState.HasValue)
            {
                Interpolate();
            }
        }

        void SendStateToServer()
        {
            State currentState = new(Body.position, Body.velocity, NetworkTime.time);
            AddStateToHistory(currentState);
            CmdSendStateToServer(currentState);
        }


        [Command(channel = Channels.Unreliable)]
        void CmdSendStateToServer(State clientState)
        {
            //Message can be recieved in the wrong order, we need to check against last timestamp
            if (clientState.timestamp < _lastRecievedTime)
                return;

            _lastRecievedTime = clientState.timestamp;

            //Here we could make different check to prevent cheating
            //Ex : test the distance between previous and current with timestep to see if the movement seem reasonable
            Body.position = clientState.position;
            Body.velocity = clientState.velocity;
            _bodyUpdated = true;
        }

        [ClientRpc(channel = Channels.Unreliable)]
        void RpcSyncStateToClients(State serverState)
        {
            //Message can be recieved in the wrong order, so we need to check against last timestamp
            if (serverState.timestamp < _lastRecievedTime)
                return;

            //Ignore server player
            if (isServer && isOwned) return;

            _lastRecievedTime = serverState.timestamp;
            ApplyStateCorrection(serverState);
        }

        private void ApplyStateCorrection(State serverState)
        {
            if (isOwned)
            {
                _targetState = null;
                int index = FindClosestStateIndex(serverState.timestamp);
                if (index >= 0)
                {
                    int realIndex = (_historyStartIndex + index) % MaxHistorySize;
                    State pastState = _stateHistory[realIndex];

                    float positionError = Vector2.Distance(pastState.position, serverState.position);

                    if (positionError > CorrectionThreshold)
                    {
                        _targetState = serverState;
                    }

                    _historyStartIndex = (realIndex + 1) % MaxHistorySize;
                    _historyCount = Math.Max(0, _historyCount - (index + 1));
                }
            }
            else
            {
                _targetState = serverState;
            }
        }

        private void Interpolate()
        {
            State target = _targetState.Value;
            float deltaTime = Time.fixedDeltaTime; 
            float smoothingFactor = Mathf.Clamp01(CorrectionScale * deltaTime);
            Vector2 newPosition = Vector2.Lerp(Body.position, target.position, smoothingFactor);
            Vector2 newVelocity = Vector2.Lerp(Body.velocity, target.velocity, smoothingFactor);

            // Apply extrapolation if needed (if updates are delayed)
            if (NetworkTime.time - _lastRecievedTime > SendRate)
            {
                float extrapolationTime = (float)(NetworkTime.time - _lastRecievedTime);
                newPosition += newVelocity * extrapolationTime; // Trying to predict movement
            }

            Body.position = newPosition;
            Body.velocity = newVelocity;

            // If close enough, stop correcting
            if (Vector2.Distance(Body.position, target.position) < CorrectionThreshold)
            {
                Body.position = target.position;
                Body.velocity = target.velocity;
                _targetState = null;
            }
        }

        private int FindClosestStateIndex(double targetTimestamp)
        {
            for (int i = _historyCount - 1; i >= 0; i--)
            {
                int realIndex = (_historyStartIndex + i) % MaxHistorySize;
                if (Mathf.Abs((float)(_stateHistory[realIndex].timestamp - targetTimestamp)) <= SendRate)
                {
                    return i;
                }
            }
            return -1;
        }

        private void AddStateToHistory(State state)
        {
            int insertIndex = (_historyStartIndex + _historyCount) % MaxHistorySize;
            _stateHistory[insertIndex] = state;

            if (_historyCount < MaxHistorySize)
            {
                _historyCount++;
            }
            else
            {
                _historyStartIndex = (_historyStartIndex + 1) % MaxHistorySize;
            }
        }
    }
}
