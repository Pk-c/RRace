using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct Skin
    {
        public Sprite Idle;
        public Sprite Jump;
    }

    [RequireComponent(typeof (Rigidbody2D),typeof(SpriteRenderer))]
    public class PlayerVisual: MonoBehaviour
    {
        [SerializeField]
        private Skin[] Skins;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _body;
        private int _skinIndex = 0;

        public void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            int count = SceneManager.GetObjectsCount<PlayerVisual>();

            if (Skins.Length > 0)
            {
                _skinIndex = count % Skins.Length;
                _spriteRenderer.sprite = Skins[_skinIndex].Idle;
            }

            SceneManager.Register(this);
        }

        public void Update()
        {
            _spriteRenderer.sprite = Mathf.Approximately(_body.velocity.y,0) ? Skins[_skinIndex].Idle : Skins[_skinIndex].Jump;

            if(_body.velocity.x >= 0.5f && _spriteRenderer.flipX )
                _spriteRenderer.flipX = false;
            if (_body.velocity.x <= -0.5f && !_spriteRenderer.flipX)
                _spriteRenderer.flipX = true;
        }

        public void OnDestroy()
        {
            SceneManager.Remove(this);
        }
    }
}
