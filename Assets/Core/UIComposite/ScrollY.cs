using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.UIModel
{
    public class ScrollY : ScrollContent
    {
        static void CenterScroll(ScrollY scroll)
        {
            var eve = scroll.eventCall;
            var tar = scroll.eventCall.ScrollDistanceY;
            float ty = scroll.ScrollView.sizeDelta.y * 0.5f;
            float v = scroll.Point + tar+ty;
            float sy = scroll.ItemSize.y;
            float oy = v % sy;
            tar -= oy;
            if (oy > sy * 0.5f)
                tar += sy;
            tar += sy * 0.5f;
            scroll.eventCall.ScrollDistanceY = tar;
            v = scroll.Point + tar + ty;
            int i = (int)(v / sy);
            int c = scroll.DataLength;
            i %= c;
            if (i < 0)
                i += c - 1;
            scroll.PreDockindex = i;
        }
        public EventCallBack eventCall;//scrollY自己的按钮
        protected float height;
        int Column = 1;
        float m_point;
        /// <summary>
        /// 滚动的当前位置，从0开始
        /// </summary>
        public float Point { get { return m_point; } set { Refresh(0,value - m_point); } }
        float m_pos = 0;
        /// <summary>
        /// 0-1之间
        /// </summary>
        public float Pos
        {
            get {
                var p = m_point/(ActualSize.y - Size.y);
                if (p < 0)
                    p = 0;
                else if (p > 1)
                    p = 1;
                return p;
            }
            set
            {
                if (value < 0 | value > 1)
                    return;
                m_point = value * (ActualSize.y - Size.y);
                Order();
            }
        }
        public bool ItemDockCenter;
        public int PreDockindex { get; private set; }

        public ScrollY()
        {
        }
        public ScrollY(RectTransform rect)
        {
            Initial(rect, null);
        }
        ModelElement mModel;
        public override void Initial(RectTransform rect, ModelElement model)
        {
            base.Initial(rect,model);
            ScrollView = rect;
            eventCall = EventCallBack.RegEvent<EventCallBack>(rect);
            eventCall.Drag = Draging;
            eventCall.DragEnd = (o, e, s) => {
                Scrolling(o, s);
                if (ItemDockCenter)
                    CenterScroll(this);
                if (ScrollStart != null)
                    ScrollStart(this);
                if (eventCall.VelocityY== 0)
                    OnScrollEnd(o);
            };
            eventCall.Scrolling = Scrolling;
            eventCall.ScrollEndY = OnScrollEnd;
            eventCall.ForceEvent = true;
            eventCall.AutoColor = false;
            Size = ScrollView.sizeDelta;
            ScrollView.anchorMin = ScrollView.anchorMax = ScrollView.pivot = Center;
            eventCall.CutRect = true;
            mModel = model;
        }
        public void ChangeItem(string item)
        {
            if (mModel != null)
            {
                m_point = 0;
                ItemMod = mModel.Find(item);
                if (ItemMod != null)
                    ItemSize = ItemMod.data.sizeDelta;
            }
        }
        public Action<ScrollY, Vector2> Scroll;
        public Action<ScrollY> ScrollStart;
        public Action<ScrollY> ScrollEnd;
        public Action<ScrollY> ScrollToTop;
        public Action<ScrollY> ScrollToDown;
        void Draging(EventCallBack back, UserAction action, Vector2 v)
        {
            back.DecayRateY = 0.998f;
            Scrolling(back, v);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="back"></param>
        /// <param name="v">移动的实际像素位移</param>
        void Scrolling(EventCallBack back, Vector2 v)
        {
            if (ScrollView == null)
                return;
            v.y /= eventCall.Target.localScale.y;
            back.VelocityX = 0;
            v.x = 0;
            float x = 0;
            float y = 0;
            switch (scrollType)
            {
                case ScrollType.None:
                    y = ScrollNone(back, ref v, ref x, ref m_point).y;
                    break;
                case ScrollType.Loop:
                    y = ScrollLoop(back, ref v, ref x, ref m_point).y;
                    break;
                case ScrollType.BounceBack:
                    y = BounceBack(back, ref v, ref x, ref m_point).y;
                    break;
            }
            Order();
            if (y != 0)
            {
                if (Scroll != null)
                    Scroll(this, v);
            }
            else
            {
                if (ScrollEnd != null)
                    ScrollEnd(this);
            }
        }
        void OnScrollEnd(EventCallBack back)
        {
            if (scrollType == ScrollType.BounceBack)
            {
                if (m_point < 0)
                {
                    back.DecayRateY = 0.988f;
                    float d = 0.25f - m_point;
                    back.ScrollDistanceY = d * eventCall.Target.localScale.y;
                }
                else if (m_point + Size.y > ActualSize.y)
                {
                    back.DecayRateY = 0.988f;
                    float d = ActualSize.y - m_point - Size.y - 0.25f;
                    back.ScrollDistanceY = d * eventCall.Target.localScale.y;
                }
                else
                {
                    if (ScrollEnd != null)
                        ScrollEnd(this);
                }
            }
            else if (ScrollEnd != null)
                ScrollEnd(this);
        }
        public void Calcul()
        {
            float w = Size.x - ItemOffset.x;
            w /= ItemSize.x;
            Column = (int)w;
            if (Column < 1)
                Column = 1;
            int c = DataLength;
            int a = c % Column;
            c /= Column;
            if (a > 0)
                c++;
            height = c * ItemSize.y;
            //height += OffsetStart + OffsetEnd;
            if (height < Size.y)
                height = Size.y;
            ActualSize = new Vector2(Size.x, height);
        }
        public override void Refresh(float x = 0, float y = 0)
        {
            Size = ScrollView.sizeDelta;
            m_point = y;
            ActualSize = Vector2.zero;
            if (DataLength== 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].target.SetActive(false);
                return;
            }
            if (ItemMod == null)
            {
                return;
            }
            if (ItemSize.y == 0)
            {
                return;
            }
            Calcul();
            Order(true);
        }
        /// <summary>
        /// 指定下标处的位置重排
        /// </summary>
        /// <param name="_index"></param>
        public void ShowByIndex(int _index)
        {
            Size = ScrollView.sizeDelta;
            ActualSize = Vector2.zero;
            if (DataLength==0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].target.SetActive(false);
                return;
            }
            if (ItemMod == null)
            {
                return;
            }
            if (ItemSize.y == 0)
            {
                return;
            }
            float y = _index * ItemSize.y;
            m_point = y;
            Calcul();
            Order(true);
        }
        void Order(bool force=false)
        {
            int len = DataLength;
            float ly = ItemSize.y;
            int sr = (int)(m_point /ly);//起始索引
            int er = (int)((m_point + Size.y) / ly)+1;
            sr *= Column;
            er *= Column;//结束索引
            int e = er - sr;//总计显示数据
            if (e > len)
                e = len;
            if(scrollType==ScrollType.Loop)
            {
                if (er >= len)
                {
                    er -= len;
                    RecycleInside(er, sr);
                }
                else
                {
                    RecycleOutside(sr, er);
                }
            }
            else
            {
                if (sr < 0)
                    sr = 0;
                if (er >= len)
                    er = len;
                e = er - sr;
                RecycleOutside(sr, er);
            }
       
            PushItems();//将未被回收的数据压入缓冲区
            int index = sr;
            float oy = 0;
            for (int i=0;i<e;i++)
            {
                UpdateItem(index,oy,force);
                index++;
                if (index >= len)
                {
                    if (scrollType != ScrollType.Loop)
                        break;
                    index = 0;
                    oy = ActualSize.y;
                }
            }
        }
        void UpdateItem(int index,float oy,bool force)
        {
            float ly = ItemSize.y;
            int row = index / Column;
            float dy = ly * row + oy;
            dy -= m_point;
            float ss = 0.5f * Size.y - 0.5f * ly;
            dy = ss - dy;
            float ox = (index%Column) * ItemSize.x + ItemSize.x * 0.5f + ItemOffset.x - Size.x * 0.5f;
            var a = PopItem(index);
            a.target.transform.localPosition = new Vector3(ox, dy, 0);
            Items.Add(a);
            if(a.index<0 | force)
            {
                var dat = GetData(index);
                a.datacontext = dat;
                a.index = index;
                ItemUpdate(a.obj, dat, index);
            }
        }
        public void SetSize(Vector2 size)
        {
            Size = size;
            ScrollView.sizeDelta = size;
            Refresh();
        }
    }
}
