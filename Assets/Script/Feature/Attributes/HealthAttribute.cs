using Mirror;
using TMPro;
using UnityEngine;
namespace Game
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class HealthAttribute : NetworkBehaviour, IReset
    {
        public TextMeshPro _indicator;
        public int StartLife = 3;
        public float InvicibilityTime = 1.0f;
        public LayerMask DamagerMask;
        public float BlinkRate = 0.1f; // How fast to blink

        private Collider2D[] result = new Collider2D[1];
        private BoxCollider2D _hitBox = null;
        private ContactFilter2D _filter;
        private SpriteRenderer _spriteRenderer;

        private float _currentInvicibility = 0.0f;

        [SyncVar(hook = nameof(OnLifeChanged))]
        private int LifeAmount = 3;

        public bool IsDead { get { return LifeAmount <= 0; } }

        public void Awake()
        {
            _hitBox = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _filter.useLayerMask = true;
            _filter.layerMask = DamagerMask;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Reset();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdateLifeDisplay();
        }

        [Server]
        public void SetHealth(int amount)
        {
            LifeAmount = Mathf.Max(0, LifeAmount + amount);
        }

        public void Reset()
        {
            if (isServer)
            {
                LifeAmount = StartLife;
            }
        }

        private void OnLifeChanged(int oldValue, int newValue)
        {
            UpdateLifeDisplay();
        }

        private void UpdateLifeDisplay()
        {
            if (_indicator != null)
            {
                _indicator.text = LifeAmount.ToString();
            }
        }

        private void Update()
        {
            if (_currentInvicibility <= 0)
            {
                if (_hitBox == null)
                    return;

                int numCollider = _hitBox.OverlapCollider(_filter, result);
                if (numCollider > 0)
                {
                    if (result[0].gameObject != null)
                    {
                        if (LayerUtils.IsLayerInMask(DamagerMask, result[0].gameObject.layer))
                        {
                            _currentInvicibility = InvicibilityTime;
                            SetBlink();

                            if (isServer)
                            {
                                SetHealth(-1);
                            }
                        }
                    }
                }
            }
            

            if (_currentInvicibility > 0)
            {
                _currentInvicibility -= Time.deltaTime;
                if (_currentInvicibility <= 0)
                {
                    SetBlink();
                }
                else
                {
                    UpdateBlink();
                }
            }
        }

        //Quick hack for blink
        private float _blinkTimer = 0;
        private bool _isVisible = true;

        private void SetBlink()
        {
            _blinkTimer = 0;
            _isVisible = true;
            _spriteRenderer.enabled = true;
        }

        private void UpdateBlink()
        {
            if (_spriteRenderer == null) return;

            _blinkTimer += Time.deltaTime;
            if (_blinkTimer >= BlinkRate)
            {
                _blinkTimer = 0;
                _isVisible = !_isVisible;
                _spriteRenderer.enabled = _isVisible;
            }
        }
    }
}