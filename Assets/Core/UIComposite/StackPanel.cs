using huqiang.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huqiang.UIComposite
{
    public enum Direction
    {
        Horizontal, Vertical
    }
    public class StackPanel : ModelInitalS
    {
        public ModelElement model;

        public Direction direction = Direction.Vertical;
        public override void Initial(ModelElement mod)
        {
            model = mod;
            mod.SizeChanged = SizeChanged;
        }
        void SizeChanged(ModelElement mod)
        {
            Order();
        }
        public void Order()
        {
            var size = model.data.sizeDelta;
            var child = model.child;
            if (direction == Direction.Horizontal)
            {
                float h = size.y;
                float x = size.x * 0.5f;
                float sx = -x;
                for (int i = 0; i < child.Count; i++)
                {
                    var c = child[i];
                    if (c.data.sizeDelta.y != h)
                    {
                        c.data.sizeDelta.y = h;
                        ModelElement.ScaleSize(c);
                    }
                    float ix = c.data.sizeDelta.x;
                    c.data.localPosition.x = sx + ix * 0.5f;
                    c.data.localPosition.y = 0;
                    if (sx > x)
                    {
                        c.Context.gameObject.SetActive(false);
                    }
                    else {
                        c.Context.gameObject.SetActive(true);
                    }
                    sx += ix;
                    c.Context.localPosition = c.data.localPosition;
                }
            }
            else
            {
                float w = size.x;
                float sy = size.y * 0.5f;
                float y = -sy;
                for (int i = 0; i < child.Count; i++)
                {
                    var c = child[i];
                    if (c.data.sizeDelta.x != w)
                    {
                        c.data.sizeDelta.x = w;
                        ModelElement.ScaleSize(c);
                    }
                    float iy = c.data.sizeDelta.y;
                    c.data.localPosition.y = sy - iy * 0.5f;
                    c.data.localPosition.x = 0;
                    if (sy > y)
                    {
                        c.Context.gameObject.SetActive(false);
                    }
                    else {
                        c.Context.gameObject.SetActive(true);
                    }
                    sy -= iy;
                    c.Context.localPosition = c.data.localPosition;
                }
            }
        }
    }
}
