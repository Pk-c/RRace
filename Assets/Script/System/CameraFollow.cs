using UnityEngine;

namespace Game
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform _player;
        private float _smoothSpeed = 5f;
        private float _minX;
        private Vector3 _velocity = Vector3.zero; // Persistent velocity across frames

        void Start()
        {
            _minX = transform.position.x;
        }

        public void Init(Transform player, float speed)
        {
            _player = player;
            _smoothSpeed = speed;
        }

        void LateUpdate()
        {
            if (_player == null) return;

            float targetX = Mathf.Max(_player.position.x, _minX);
            Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothSpeed);
        }
    }
}
