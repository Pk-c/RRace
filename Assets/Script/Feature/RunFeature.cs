using Mirror;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PlayerInputController))]
    public class RunFeature : NetFeature
    {
        private const float STOP_THRESHOLD = 0.1f;

        [SerializeField] private float MaxSpeed = 5f;
        [SerializeField] private float Acceleration = 10f;
        [SerializeField] private float Deceleration = 5f;
        [SerializeField] private float Friction = 2f;
        [SerializeField] private LayerMask WallLayer;
        [SerializeField] private float WallCheckDistance = 0.1f;

        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private float _moveInput;
        private PlayerInputController _playerInputController;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _playerInputController = GetComponent<PlayerInputController>();
        }

        protected override void OwnerUpdate()
        {
            _moveInput = _playerInputController.InputEnabled ? Input.GetAxis("Horizontal") : 0.0f;
        }

        protected override void OwnerFixedUpdate()
        {
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (!Mathf.Approximately( _moveInput,0))
            {
                //Wall AntiStick
                if ((_moveInput > 0 && !CheckDirection(Vector2.right)) || (_moveInput < 0 && !CheckDirection(Vector2.left)))
                {
                    float targetSpeed = _moveInput * MaxSpeed;
                    float speedDiff = targetSpeed - _body.velocity.x;
                    float accelerationRate = Mathf.Abs(targetSpeed) > STOP_THRESHOLD ? Acceleration : Deceleration;
                    float movement = accelerationRate * speedDiff * Time.fixedDeltaTime;
                    _body.velocity += new Vector2(movement, 0);
                    return;
                }
            }
            
            if (Mathf.Abs(_body.velocity.x) > STOP_THRESHOLD)
            {
                float slowDown = Mathf.Sign(_body.velocity.x) * Friction * Time.fixedDeltaTime;
                _body.velocity -= new Vector2(slowDown, 0);
            }
            else
            {
                _body.velocity = new Vector2(0, _body.velocity.y);
            }
        }

        bool CheckDirection(Vector2 dir)
        {
            Vector2 boxSize = new Vector2(0.1f, _collider.bounds.size.y * 0.9f);
            Vector2 boxCenter = (Vector2)transform.position;
            return Physics2D.OverlapBox(boxCenter + dir * WallCheckDistance, boxSize, 0, WallLayer) != null;
        }
    }
}
