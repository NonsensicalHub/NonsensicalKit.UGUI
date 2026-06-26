using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    [RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
    public class GridGroupFix : MonoBehaviour
    {
        private GridLayoutGroup _layout;
        private RectTransform _rectTransform;

        private int _defaultLeft;
        private int _defaultRight;
        private bool _nowCenter;

        private void Awake()
        {
            _layout = GetComponent<GridLayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _defaultLeft = _layout.padding.left;
            _defaultRight = _layout.padding.right;
            _nowCenter = _layout.childAlignment == TextAnchor.UpperCenter;
        }

        private void Update()
        {
            Check();
        }

        private void Check()
        {
            var childCount = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    childCount++;
                }
            }

            var width = _rectTransform.rect.width;
            var cellWidth = _layout.cellSize.x;
            var spacing = _layout.spacing.x;
            var centerWidth = width - _defaultLeft - _defaultRight;
            var value = childCount * cellWidth + (childCount - 1) * spacing;

            var crt = value > centerWidth;
            if (crt != _nowCenter)
            {
                _nowCenter = crt;
                if (_nowCenter)
                {
                    _layout.padding.left = _defaultLeft;
                    _layout.padding.right = _defaultRight;
                    _layout.childAlignment = TextAnchor.UpperCenter;
                }
                else
                {
                    var n1 = (int)(centerWidth / (cellWidth + spacing));
                    var n2 = n1 * (cellWidth + spacing) + cellWidth;

                    if (n2 < centerWidth)
                    {
                        n1++;
                    }

                    var paddingWidth = width - (n1 * (cellWidth + spacing) - spacing);
                    _layout.padding.left = (int)(paddingWidth * 0.5f);
                    _layout.padding.right = (int)(paddingWidth * 0.5f);
                    _layout.childAlignment = TextAnchor.UpperLeft;
                }
            }
        }
    }
}
