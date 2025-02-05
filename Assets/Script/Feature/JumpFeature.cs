using Mirror;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class JumpFeature : NetFeature
    {
        [SerializeField] private float maxJumpForce = 12f;
        [SerializeField] private float minJumpForce = 3f;
        [SerializeField] private float jumpChargeTime = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.1f;

        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private bool isJumping;
        private float jumpStartTime;
        private bool isGrounded;
        private float currentJumpForce;
        private Vector2 groundCheckSize;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            groundCheckSize = new Vector2(boxCollider.bounds.size.x * 0.9f, 0.1f);
        }

        protected override void OwnerUpdate()
        {
            CheckGrounded();
            HandleJumpInput();
        }

        private void CheckGrounded()
        {
            Vector2 boxCenter = new Vector2(transform.position.x, boxCollider.bounds.min.y);
            RaycastHit2D hit = Physics2D.BoxCast(boxCenter, groundCheckSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
            isGrounded = hit.collider != null;

            //Debug
            //Color debugColor = isGrounded ? Color.green : Color.red;
            //Debug.DrawRay(boxCenter, Vector2.down * groundCheckDistance, debugColor);
            //Debug.DrawLine(boxCenter - new Vector2(groundCheckSize.x / 2, 0), boxCenter + new Vector2(groundCheckSize.x / 2, 0), debugColor);
        }

        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                isJumping = true;
                jumpStartTime = Time.time;
                currentJumpForce = minJumpForce;
                body.velocity = new Vector2(body.velocity.x, currentJumpForce);
            }

            if (Input.GetButton("Jump") && isJumping)
            {
                float holdTime = Time.time - jumpStartTime;
                if (holdTime < jumpChargeTime)
                {
                    float extraForce = Mathf.Lerp(0f, maxJumpForce - minJumpForce, holdTime / jumpChargeTime);
                    body.velocity = new Vector2(body.velocity.x, minJumpForce + extraForce);
                }
            }

            if (Input.GetButtonUp("Jump") || (Time.time - jumpStartTime >= jumpChargeTime) || body.velocity.y < 0)
            {
                isJumping = false;
            }
        }
    }
}