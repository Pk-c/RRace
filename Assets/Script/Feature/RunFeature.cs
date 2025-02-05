using Mirror;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RunFeature : NetFeature
    {
        private const float STOP_THRESHOLD = 0.1f;

        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 5f;
        [SerializeField] private float friction = 2f;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float wallCheckDistance = 0.1f;

        private Rigidbody2D body;
        private float moveInput;
        private bool isTouchingWall;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        protected override void OwnerUpdate()
        {
            moveInput = Input.GetAxis("Horizontal");
            CheckWallCollision();
        }

        protected override void OwnerFixedUpdate()
        {
            if (!isTouchingWall)
            {
                ApplyMovement();
            }
            else
            {
                body.velocity = new Vector2(0, body.velocity.y);
            }
        }

        private void ApplyMovement()
        {
            if (moveInput != 0)
            {
                float targetSpeed = moveInput * maxSpeed;
                float speedDiff = targetSpeed - body.velocity.x;
                float accelerationRate = Mathf.Abs(targetSpeed) > STOP_THRESHOLD ? acceleration : deceleration;
                float movement = accelerationRate * speedDiff * Time.fixedDeltaTime;
                body.velocity += new Vector2(movement, 0);
            }
            else
            {
                if (Mathf.Abs(body.velocity.x) > STOP_THRESHOLD)
                {
                    float slowDown = Mathf.Sign(body.velocity.x) * friction * Time.fixedDeltaTime;
                    body.velocity -= new Vector2(slowDown, 0);
                }
                else
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
        }

        private void CheckWallCollision()
        {
            Vector2 boxSize = new Vector2(0.1f, body.GetComponent<Collider2D>().bounds.size.y * 0.9f);
            Vector2 boxCenter = (Vector2)transform.position;
            isTouchingWall = Physics2D.OverlapBox(boxCenter + Vector2.left * wallCheckDistance, boxSize, 0, wallLayer) ||
                             Physics2D.OverlapBox(boxCenter + Vector2.right * wallCheckDistance, boxSize, 0, wallLayer);

            //Debug
            //Color debugColor = isTouchingWall ? Color.red : Color.green;
            //Debug.DrawRay(boxCenter, Vector2.left * wallCheckDistance, debugColor);
            //Debug.DrawRay(boxCenter, Vector2.right * wallCheckDistance, debugColor);
        }
    }
}
