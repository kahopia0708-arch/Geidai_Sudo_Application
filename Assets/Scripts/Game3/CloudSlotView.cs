using UnityEngine;
using UnityEngine.EventSystems;

namespace Geidai.Game3
{
    /// <summary>
    /// PitchItemを置くための雲スロット。
    /// </summary>
    public class CloudSlotView : MonoBehaviour, IDropHandler
    {
        private PitchItemView _occupant;

        /// <summary>
        /// 現在この雲に置かれているPitchItem。
        /// </summary>
        public PitchItemView Occupant => _occupant;

        public bool IsEmpty => _occupant == null;

        public void OnDrop(PointerEventData eventData)
        {
            
            if (eventData.pointerDrag == null)
                return;

            PitchItemDragHandler dragHandler =
                eventData.pointerDrag.GetComponent<PitchItemDragHandler>();

            if (dragHandler == null)
                return;

            dragHandler.TryPlaceInSlot(this);
        }

        public bool CanAccept(PitchItemView item)
        {
            return _occupant == null || _occupant == item;
        }

        public void Accept(PitchItemView item)
        {
            _occupant = item;
        }

        public void Release(PitchItemView item)
        {
            if (_occupant == item)
            {
                _occupant = null;
            }
        }
    }
}
