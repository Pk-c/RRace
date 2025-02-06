
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BonusPickerFeature : NetFeature
    {
        public LayerMask BonusPickerMask;

        private Collider2D[] result = new Collider2D[1];
        private BoxCollider2D _pickerCollider = null;
        private ContactFilter2D _filter;

        public void Awake()
        {
            _pickerCollider = GetComponent<BoxCollider2D>();
            _filter.layerMask = BonusPickerMask;
            _filter.useLayerMask = true;
        }

        protected override void SharedUpdate()
        {
            if (_pickerCollider == null)
                return;

            int numCollider = _pickerCollider.OverlapCollider(_filter, result);
            if(numCollider > 0)
            {
                //Here using shared so we can spawn VFX or play sound on client when we pick something

                if( result[0].gameObject.TryGetComponent<Pickable>( out Pickable pickable))
                {
                    pickable.OnPicked(gameObject);
                }
            }
        }
    }
}