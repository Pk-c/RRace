using UnityEngine;

namespace Game
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform _player;
        private float _smoothSpeed = 5f;
        private float _minY;
        private float _minX;

        void Start()
        {
            _minY = transform.position.y;
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

            Vector3 targetPosition = new Vector3(_player.position.x, _player.position.y, transform.position.z);
            targetPosition.x = Mathf.Max(targetPosition.x, _minX);
            targetPosition.y = Mathf.Max(targetPosition.y, _minY);
            transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
        }
    }
}
