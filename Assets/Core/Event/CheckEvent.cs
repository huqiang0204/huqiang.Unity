using System;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{

    public class CheckEvent : EventCallBack
    {
        bool m_checked=false;
        public Sprite spriteA;
        public Sprite spriteB;
        public bool Checked { get { return m_checked; } set { m_checked = value;ChangePicture(); } }
        void ChangePicture()
        {
            if (graphic == null)
                return;
            if (m_checked)
            {
                if(graphic is Image)
                {
                    (graphic as Image).sprite = spriteA;
                }else if(graphic is RawImage)
                {
                    (graphic as RawImage).SetSprite(spriteA);
                }
            }
            else {
                if (graphic is Image)
                {
                    (graphic as Image).sprite = spriteB;
                }
                else if (graphic is RawImage)
                {
                    (graphic as RawImage).SetSprite(spriteB);
                }
            }
        }
        //public override RectTransform Target
        //{
        //    get
        //    {
        //        return base.Target;
        //    }
        //    protected set
        //    {
        //        base.Target = value;
        //        ChangePicture();
        //    }
        //}
        public Action<CheckEvent> ValueChanged;
        public CheckEvent()
        {
            Click = (o,e) => { Checked = !m_checked; if (ValueChanged != null) ValueChanged(this); };
        }
    }
}