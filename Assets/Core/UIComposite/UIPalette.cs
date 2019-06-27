using huqiang.Other;
using huqiang.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIComposite
{
    public class UIPalette : ModelInitalS
    {
        EventCallBack callBackR;
        EventCallBack callBackC;
        RectTransform hc;
        RectTransform NobA;
        RectTransform NobB;
        RawImage template;
        RawImage htemp;
        RawImage slider;
        Palette palette;
        public Color SelectColor = Color.white;
        float Alpha;
        public Action<UIPalette> ColorChanged;
        public Action<UIPalette> TemplateChanged;
        UISlider uISlider;
        public override void Initial(ModelElement mod)
        {
            mod.InstantiateChild();
            var rect = mod.Context;
            palette = new Palette();
            callBackR = EventCallBack.RegEvent<EventCallBack>(rect);
            callBackR.IsCircular = true;
            callBackR.Drag = callBackR.DragEnd = DragingR;
            callBackR.PointerDown = PointDownR;
            NobA = rect.Find("NobA") as RectTransform;
            NobB = rect.Find("NobB") as RectTransform;
            hc = rect.Find("HTemplate") as RectTransform;
            template = hc.GetComponent<RawImage>();
            callBackC = EventCallBack.RegEvent<EventCallBack>(hc);
            callBackC.Drag = callBackC.DragEnd = DragingC;
            callBackC.PointerDown = PointDownC;
            htemp = rect.GetComponent<RawImage>();
            htemp.texture = Palette.LoadCTemplateAsync();
            template.texture = palette.texture;
            slider.texture = Palette.AlphaTemplate();
            palette.LoadHSVTAsyncM(1);
            SelectColor.a = 1;
            var son = mod.Find("Slider");
            slider = son.Context.GetComponent<RawImage>();
            uISlider = new UISlider();
            uISlider.Initial(son);
            uISlider.OnValueChanged = AlphaChanged;
            uISlider.Percentage = 1;
        }
        void DragingR(EventCallBack back, UserAction action, Vector2 v)
        {
            PointDownR(back, action);
        }
        void PointDownR(EventCallBack back, UserAction action)
        {
            float x = action.CanPosition.x - back.GlobalPosition.x;
            float y = action.CanPosition.y - back.GlobalPosition.y;
            x /= back.GlobalScale.x;
            y /= back.GlobalScale.y;
            float sx = x * x + y * y;
            float r = Mathf.Sqrt(220 * 220 / sx);
            x *= r;
            y *= r;
            if (NobA != null)
            {
                NobA.localPosition = new Vector3(x, y, 0);
            }
            float al = MathH.atan(-x, -y);
            palette.LoadHSVTAsyncM(al / 360);
            Color col = palette.buffer[Index];
            SelectColor.r = col.r;
            SelectColor.g = col.g;
            SelectColor.b = col.b;
            if (TemplateChanged != null)
                TemplateChanged(this);
        }
        void DragingC(EventCallBack back, UserAction action, Vector2 v)
        {
            PointDownC(back, action);
        }
        int Index;
        void PointDownC(EventCallBack back, UserAction action)
        {
            float x = action.CanPosition.x - back.GlobalPosition.x;
            float y = action.CanPosition.y - back.GlobalPosition.y;
            x /= back.GlobalScale.x;
            y /= back.GlobalScale.y;
            if (x < -128)
                x = -128;
            else if (x > 128)
                x = 128;
            if (y < -128)
                y = -128;
            else if (y > 128)
                y = 128;
            if (NobB != null)
            {
                NobB.localPosition = new Vector3(x, y, 0);
            }
            int dx = (int)x + 128;
            if (dx < 0)
                dx = 0;
            else if (dx > 255)
                dx = 255;
            int dy = (int)y + 128;
            if (dy < 0)
                dy = 0;
            else if (dy > 255)
                dy = 255;
            Index = dy * 256 + dx;
            if (Index >= 256 * 256)
                Index = 256 * 256 - 1;
            Color col = palette.buffer[Index];
            SelectColor.r = col.r;
            SelectColor.g = col.g;
            SelectColor.b = col.b;
            if (ColorChanged != null)
                ColorChanged(this);
        }
        void AlphaChanged(UISlider slider)
        {
            SelectColor.a = slider.Percentage;
            if (ColorChanged != null)
                ColorChanged(this);
        }
    }
}
