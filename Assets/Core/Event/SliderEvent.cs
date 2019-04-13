using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public class SliderEvent : EventCallBack
    {
        RectTransform m_nob;
        public RectTransform Nob { get { return m_nob; } set { m_nob = value; Initial(); } }
        public Image SliderImage;
        public enum Direction
        {
            Horizontal = 0, Vertical = 1
        }
        float m_value;
        public float Value;
        Direction m_dir;
        public Direction direction { get { return m_dir; } set { m_dir = value; Initial(); } }
        void Initial()
        {
            if (m_nob != null)
            {
                if (m_dir == Direction.Horizontal)
                {
                    float x = m_Target.sizeDelta.x - m_nob.sizeDelta.x;
                    x *= 0.5f;
                    startPos = new Vector3(-x, 0, 0);
                    endPos = new Vector3(x, 0, 0);
                }
                else
                {
                    float y = m_Target.sizeDelta.y - m_nob.sizeDelta.y;
                    y *= 0.5f;
                    startPos = new Vector3(0, -y, 0);
                    endPos = new Vector3(0, y, 0);
                }
                endSize = startSize = m_nob.sizeDelta;
            }
        }
        public bool Number = false;
        public int minNum = 0;
        public int maxNum = 1;
        public bool CustomNonLine;
        public Vector3 startPos, endPos;
        public Vector2 startSize, endSize;
        public Vector3 NobOffset;
        public Action<SliderEvent> OnVlaueChanged;
        public Action<SliderEvent> OnVlaueEndChange;
        public override RectTransform Target
        {
            get { return m_Target; }
            protected set
            {
                base.Target = value;
                SliderImage = m_Target.GetComponent<Image>();
                Nob = m_Target.Find("Nob") as RectTransform;
            }
        }
        public SliderEvent()
        {
            Click = OnClick;
            Drag = DragEnd = OnDrag;
        }
        static void OnClick(EventCallBack back, UserAction action)
        {
            SliderEvent slider = back as SliderEvent;
            if (slider.direction == Direction.Horizontal)
            {
                float x = slider.GlobalPosition.x;
                float ox = action.CanPosition.x - x;
                float w = back.Target.sizeDelta.x;
                float s = w * slider.GlobalScale.x;
                slider.m_value = ox / s + 0.5f;
            }
            else
            {
                float y = slider.GlobalPosition.y;
                float oy = action.CanPosition.y - y;
                float w = back.Target.sizeDelta.y;
                float s = w * slider.GlobalScale.y;
                slider.m_value = oy / s + 0.5f;
            }
            ValueChanged(slider);
        }
        static void OnDrag(SliderEvent slider, UserAction action, Vector2 v)
        {
            if (slider.direction == Direction.Horizontal)
            {
                float w = slider.endPos.x - slider.startPos.x;
                float s = w * slider.GlobalScale.x;
                slider.m_value += v.x / s;
            }
            else
            {
                float h = slider.startPos.y - slider.endPos.y;
                float s = h * slider.GlobalScale.y;
                slider.m_value += v.y / s;
            }
        }
        static void OnEndDrag(EventCallBack back, UserAction action, Vector2 v)
        {
            var slider = back as SliderEvent;
            OnDrag(slider, action, v);
            ValueChanged(slider);
            if (slider.OnVlaueEndChange != null)
                slider.OnVlaueEndChange(slider);
        }
        static void OnDrag(EventCallBack back, UserAction action, Vector2 v)
        {
            var slider = back as SliderEvent;
            OnDrag(slider, action, v);
            ValueChanged(slider);
            if (slider.OnVlaueChanged != null)
                slider.OnVlaueChanged(slider);
        }
        static void ValueChanged(SliderEvent slider)
        {
            float o = slider.Value;
            if (slider.Number)
            {
                int v = slider.maxNum - slider.minNum;
                float a = slider.minNum + slider.m_value * v;
                slider.Value = (int)a;
            }
            else
            {
                if (slider.m_value < 0)
                    slider.m_value = 0;
                else
            if (slider.m_value > 1)
                    slider.m_value = 1;
                slider.Value = slider.m_value;
            }
            if (slider.Nob != null)
            {
                if (slider.direction == Direction.Horizontal)
                {
                    slider.Nob.localPosition = (slider.endPos - slider.startPos) * slider.Value + slider.startPos + slider.NobOffset;
                }
                else
                {
                    slider.Nob.localPosition = (slider.startPos - slider.endPos) * slider.Value + slider.endPos + slider.NobOffset;
                }
                slider.Nob.sizeDelta = (slider.endSize - slider.startSize) * slider.Value + slider.startSize;
            }
            if (slider.SliderImage != null)
            {
                slider.SliderImage.fillAmount = slider.Value;
            }
        }
        public void SetValue(float v)
        {
            if (Number)
            {
                if (v < minNum)
                    v = minNum;
                else
                if (v > maxNum)
                    v = maxNum;
            }
            else
            {
                if (v < 0)
                    v = 0;
                else
                if (v > 1) v = 1;
            }
            m_value = v;
            ValueChanged(this);
        }
    }
}
