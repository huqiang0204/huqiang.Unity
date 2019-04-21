using System;
using UnityEngine;

namespace huqiang.UIModel
{
    public class DragContent : ModelInital
    {
        public static Vector3 Correction(Vector2 parentSize, Vector3 sonPos, Vector2 sonSize)
        {
            if (sonSize.x <= parentSize.x)
            {
                sonPos.x = 0;
                if (sonSize.y <= parentSize.y)
                {
                    sonPos.y = 0;
                    return sonPos;
                }
            }
            else
            {
                if (sonSize.y <= parentSize.y)
                {
                    sonPos.y = 0;
                }
            }

            Vector2 dotA = Vector2.zero;
            if (sonPos.x != 0)
            {
                float right = parentSize.x * 0.5f;
                float left = -right;
                float w = sonSize.x * 0.5f;
                float a = sonPos.x - w;
                if (a > left)
                {
                    sonPos.x = left + w;
                }
                else
                {
                    a = sonPos.x + w;
                    if (a < right)
                        sonPos.x = right - w;
                }
            }
            if (sonPos.y != 0)
            {
                float top = parentSize.y * 0.5f;
                float down = -top;
                float h = sonSize.y * 0.5f;
                float a = sonPos.y - h;
                if (a > down)
                {
                    sonPos.y = down + h;
                }
                else
                {
                    a = sonPos.y + h;
                    if (a < top)
                        sonPos.y = top - h;
                }
            }
            return sonPos;
        }
        public RectTransform view;
        public RectTransform Content;
        public EventCallBack eventCall;
        public DragContent()
        {
        }
        public DragContent(RectTransform rect)
        {
            Initial(rect, null);
        }
        public override void Initial(RectTransform rect, ModelElement model)
        {
            view = rect;
            eventCall = EventCallBack.RegEventCallBack<EventCallBack>(rect);
            eventCall.Drag = (o, e, s) => { Scrolling(o, s); };
            eventCall.DragEnd = (o, e, s) => { Scrolling(o, s); };
            eventCall.Scrolling = Scrolling;
            eventCall.ForceEvent = true;
            view.anchorMin = view.anchorMax = view.pivot = new Vector2(0.5f, 0.5f);
            eventCall.CutRect = true;
            Content = view.GetChild(0) as RectTransform;
        }
        public Action<DragContent, Vector2> Scroll;
        void Scrolling(EventCallBack back, Vector2 v)
        {
            if (view == null)
                return;
            if (Content == null)
                return;
            v.x /= eventCall.Target.localScale.x;
            v.y /= eventCall.Target.localScale.y;

            var p = Content.localPosition;
            var s = Content.sizeDelta;
            p.x += v.x;
            p.y += v.y;
            v = Correction(view.sizeDelta, p, s);
            if (v.x == 0)
                back.VelocityX = 0;
            if (v.y == 0)
                back.VelocityY = 0;
            Content.localPosition = v;
            if (Scroll != null)
                Scroll(this, v);
        }
        public float Pos
        {
            get
            {
                float y = Content.sizeDelta.y - view.sizeDelta.y;
                float p = Content.localPosition.y;
                p += 0.5f * y;
                p /= y;
                if (p < 0)
                    p = 0;
                else if (p > 1)
                    p = 1;
                return p;
            }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 1)
                    value = 1;
                float y = Content.sizeDelta.y - view.sizeDelta.y;
                if (y < 0)
                    y = 0;
                y *= (value - 0.5f);
                Content.localPosition = new Vector3(0, y, 0);
            }
        }
    }
}
