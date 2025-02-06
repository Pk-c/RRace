using Mirror;
using UnityEngine;

namespace Game
{
    public class BounceMoveFeature : NetFeature
    {
        public LayerMask WallLayer;
        public float MoveSpeed = 1.0f;
        public float StartDirection = 1.0f;

        private Rigidbody2D _body;
        private BoxCollider2D _boxCollider;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        protected override void OwnerFixedUpdate()
        {
             _body.velocity = new Vector2(StartDirection * MoveSpeed, _body.velocity.y);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject == null)
                return;

            if (LayerUtils.IsLayerInMask( WallLayer, collision.gameObject.layer))
            {
                Vector2 normal = collision.contacts[0].normal;
   
                //right wall
                if (StartDirection >= 1.0f && normal.x < -0.5f)
                {
                    StartDirection = -1.0f;
                }
                
                //left wall
                else if (StartDirection <= -1.0f && normal.x > 0.5f)
                {
                    StartDirection = 1.0f;
                }
            }
        }
    }
}