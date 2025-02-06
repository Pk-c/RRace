using Mirror;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class JumpFeature : NetFeature
    {
        [SerializeField] private float MaxJumpForce = 12f;
        [SerializeField] private float MinJumpForce = 3f;
        [SerializeField] private float JumpChargeTime = 0.2f;
        [SerializeField] private LayerMask GroundLayer;
        [SerializeField] private float GroundCheckDistance = 0.1f;

        private Rigidbody2D _body;
        private BoxCollider2D _boxCollider;
        private bool _isJumping;
        private float _jumpStartTime;
        private bool _isGrounded;
        private Vector2 _groundCheckSize;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _groundCheckSize = new Vector2(_boxCollider.bounds.size.x * 0.9f, 0.1f);
        }

        protected override void OwnerUpdate()
        {
            CheckGrounded();
            HandleJumpInput();
        }

        private void CheckGrounded()
        {
            Vector2 boxCenter = new Vector2(transform.position.x, _boxCollider.bounds.min.y);
            RaycastHit2D hit = Physics2D.BoxCast(boxCenter, _groundCheckSize, 0f, Vector2.down, GroundCheckDistance, GroundLayer);
            _isGrounded = hit.collider != null;
        }

        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _isJumping = true;
                _jumpStartTime = Time.time;
                _body.velocity = new Vector2(_body.velocity.x, MinJumpForce);
            }

            if (Input.GetButton("Jump") && _isJumping)
            {
                float holdTime = Time.time - _jumpStartTime;
                if (holdTime < JumpChargeTime)
                {
                    float extraForce = Mathf.Lerp(0f, MaxJumpForce - MinJumpForce, holdTime / JumpChargeTime);
                    _body.velocity = new Vector2(_body.velocity.x, MinJumpForce + extraForce);
                }
            }

            if (Input.GetButtonUp("Jump") || (Time.time - _jumpStartTime >= JumpChargeTime) || _body.velocity.y < 0)
            {
                _isJumping = false;
            }
        }
    }
}