using UnityEngine;
using UnityEngine.EventSystems;

namespace NonsensicalKit.UGUI.VideoManager
{
    public class VideoProgressSlider : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public delegate void ProgressSliderDragHandle(bool isDrag);
        public event ProgressSliderDragHandle OnProgressSliderDrag;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnProgressSliderDrag(true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnProgressSliderDrag(false);
        }
    }
}
