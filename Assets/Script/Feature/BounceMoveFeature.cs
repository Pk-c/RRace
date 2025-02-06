using Mirror;
using UnityEngine;

namespace Game
{
    public class BounceMoveFeature : NetFeature
    {
        public LayerMask WallLayer;
        public float MoveSpeed = 1.0f;

        private Rigidbody2D _body;
        private BoxCollider2D _boxCollider;
        private float _horizontalVelocity = 1.0f;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        protected override void OwnerFixedUpdate()
        {
             _body.velocity = new Vector2(_horizontalVelocity * MoveSpeed, _body.velocity.y);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject == null)
                return;

            if (LayerUtils.IsLayerInMask( WallLayer, collision.gameObject.layer))
            {
                Vector2 normal = collision.contacts[0].normal;
   
                //right wall
                if (_horizontalVelocity >= 1.0f && normal.x < -0.5f)
                {
                    _horizontalVelocity = -1.0f;
                }
                
                //left wall
                else if (_horizontalVelocity <= -1.0f && normal.x > 0.5f)
                {
                    _horizontalVelocity = 1.0f;
                }
            }
        }
    }
}