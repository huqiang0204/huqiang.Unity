using System;
using UnityEngine;

namespace huqiang.UIModel
{
    public class ScrollX : ScrollContent
    {
        static void CenterScroll(ScrollX scroll)
        {
            var eve = scroll.eventCall;
            var tar = scroll.eventCall.ScrollDistanceX;
            float v = scroll.Point + tar;
            float sx = scroll.ItemSize.x;
            float ox = v % sx;
            tar -= ox;
            if (ox > sx * 0.5f)
                tar += sx;
            scroll.eventCall.ScrollDistanceX = tar;
            v = scroll.Point + tar + scroll.ScrollView.sizeDelta.x * 0.5f;
            int i = (int)(v / sx);
            int c = scroll.DataLength;
            i %= c;
            if (i < 0)
                i += c - 1;
            scroll.PreDockindex = i;
        }
        public bool ItemDockCenter;
        public int PreDockindex { get; private set; }
        public EventCallBack eventCall;
        protected float width;
        int Row= 1;
        float m_point;
        public float Point;
        public Action<ScrollX, Vector2> Scroll;
        public Action<ScrollX> ScrollStart;
        public Action<ScrollX> ScrollEnd;
        public Action<ScrollX> ScrollToLeft;
        public Action<ScrollX> ScrollToRight;
        public ScrollX()
        {
        }
        public ScrollX(RectTransform rect)
        {
            Initial(rect, null);
        }
        public override void Initial(RectTransform rect, ModelElement model)
        {
            ScrollView = rect;
            eventCall = EventCallBack.RegEvent<EventCallBack>(rect);
            eventCall.Drag = Draging;
            eventCall.DragEnd = (o, e, s) => {
                Scrolling(o, s);
                if (ItemDockCenter)
                    CenterScroll(this);
                if (ScrollStart != null)
                    ScrollStart(this);
                if (eventCall.VelocityX == 0)
                    OnScrollEnd(o);
            };
            eventCall.Scrolling = Scrolling;
            eventCall.PointerUp = (o, e) => { };
            eventCall.ScrollEndX = OnScrollEnd;
            eventCall.ForceEvent = true;
            eventCall.AutoColor = false;
            Size = ScrollView.sizeDelta;
            ScrollView.anchorMin = ScrollView.anchorMax = ScrollView.pivot = Center;
            eventCall.CutRect = true;
            Model = model;
            SetItemModel(0);
        }
        void Draging(EventCallBack back, UserAction action, Vector2 v)
        {
            back.DecayRateX = 0.998f;
            Scrolling(back, v);
        }
        void Scrolling(EventCallBack back, Vector2 v)
        {
            if (ScrollView == null)
                return;
            v.x /= eventCall.Target.localScale.x;
            float x = Limit(back, v.x);
            Order();
            if (x != 0)
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
                    back.DecayRateX = 0.988f;
                    float d = 0.25f + m_point;
                    back.ScrollDistanceX = d * eventCall.Target.localScale.x;
                }
                else if (m_point + Size.x > ActualSize.x)
                {
                    back.DecayRateX = 0.988f;
                    float d =  m_point + Size.x + 0.25f- ActualSize.x;
                    back.ScrollDistanceX = d * eventCall.Target.localScale.x;
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
        protected void Calcul()
        {
            float h = Size.y - ItemOffset.y;
            h /= ItemSize.y;
            Row = (int)h;
            if (Row < 1)
                Row = 1;
            int c = DataLength;
            int a = c % Row;
            c /= Row;
            if (a > 0)
                c++;
            width = c * ItemSize.x;
            if (width < Size.x)
                width = Size.x;
            ActualSize = new Vector2(width, Size.y);
        }
        /// <summary>
        /// 将视野转到新增的Item处，
        /// </summary>
        /// <param name="index_">新增item的下标</param>
        public void ShowByIndex(int index_)
        {
            int num = DataLength;
            if (index_ > num - 1) return;
            ActualSize = Vector2.zero;
            if (DataLength== 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].target.SetActive(false);
#if DEBUG
                Debug.Log("没有绑定的数据");
#endif
                return;
            }
            if (ItemMod == null)
            {
#if DEBUG
                Debug.Log("没有绑定UI模型");
#endif
                return;
            }
            if (ItemSize.x == 0)
            {
#if DEBUG
                Debug.Log("模型的尺寸不正确");
#endif
                return;
            }
            float moveX = index_ * ItemSize.x;
            Calcul();
            Initialtems();
            Order(moveX, true);
        }
        public override void Refresh(float x = 0, float y = 0)
        {
            Size = ScrollView.sizeDelta;
            ActualSize = Vector2.zero;
            if (DataLength == 0)
            {
                for (int i = 0; i < Items.Count; i++)
                    Items[i].target.SetActive(false);
                return;
            }
            if (ItemMod == null)
            {
                return;
            }
            if (ItemSize.x == 0)
            {
                return;
            }
            Calcul();
            Order(true);
        }
        void Order(bool force = false)
        {
            int len = DataLength;
            float lx = ItemSize.x;
            int sr = (int)(m_point / lx);//起始索引
            int er = (int)((m_point + Size.x) / lx) + 1;
            sr *= Row;
            er *= Row;//结束索引
            int e = er - sr;//总计显示数据
            if (e > len)
                e = len;
            if (scrollType == ScrollType.Loop)
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
            float ox = 0;
            for (int i = 0; i < e; i++)
            {
                UpdateItem(index, ox, force);
                index++;
                if (index >= len)
                {
                    index = 0;
                    ox = ActualSize.x;
                }
            }
        }
        void UpdateItem(int index, float ox, bool force)
        {
            float lx = ItemSize.x;
            int col = index / Row;
            float dx= lx * col + ox;
            dx -= m_point;
            float ss =   0.5f * Size.x- 0.5f * lx;
            dx = dx-ss;
            float oy = Size.y * 0.5f- (index % Row) * ItemSize.y - ItemSize.y * 0.5f + ItemOffset.y;
            var a = PopItem(index);
            a.target.transform.localPosition = new Vector3(dx, oy, 0);
            if (a.index < 0 | force)
            {
                var dat = GetData(index);
                a.datacontext = dat;
                a.index = index;
                if (ItemUpdate != null)
                {
                    if (a.obj == null)
                        ItemUpdate(a.target, dat, index);
                    else ItemUpdate(a.obj, dat, index);
                }
            }
        }
        public void SetSize(Vector2 size)
        {
            Size = size;
            ScrollView.sizeDelta = size;
            Refresh();
        }
        protected float Limit(EventCallBack callBack, float x)
        {
            var size = Size;
            switch (scrollType)
            {
                case ScrollType.None:
                    float vx = m_point - x;
                    if (vx < 0)
                    {
                        m_point = 0;
                        eventCall.VelocityX = 0;
                        if (ScrollToLeft != null)
                            ScrollToLeft(this);
                        return 0;
                    }
                    else if (vx + size.x > ActualSize.x)
                    {
                        m_point = ActualSize.x - size.x;
                        eventCall.VelocityX = 0;
                        if (ScrollToRight != null)
                            ScrollToRight(this);
                        return 0;
                    }
                    m_point -= x;
                    break;
                case ScrollType.Loop:
                    m_point -= x;
                    float ax = ActualSize.x;
                    if (m_point < 0)
                        m_point += ax;
                    else if (m_point > ax)
                        m_point %= ax;
                    break;
                case ScrollType.BounceBack:
                    m_point -= x;
                    if (!callBack.Pressed)
                    {
                        if (m_point < 0)
                        {
                            if (x > 0)
                                if (eventCall.DecayRateX >= 0.99f)
                                {
                                    eventCall.DecayRateX = 0.9f;
                                    eventCall.VelocityX = eventCall.VelocityX;
                                }
                        }
                        else if (m_point + size.x > ActualSize.x)
                        {
                            if (x < 0)
                            {
                                if (eventCall.DecayRateX >= 0.99f)
                                {
                                    eventCall.DecayRateX = 0.9f;
                                    eventCall.VelocityX = eventCall.VelocityX;
                                }
                            }
                        }
                    }
                    break;
            }
            return x;
        }
    }
}
