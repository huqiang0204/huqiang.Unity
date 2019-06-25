using huqiang.Data;
using huqiang.UIModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public class EventCallBack
    {
        /// <summary>
        /// 设置默认最小的按钮框
        /// </summary>
        public static Vector2 MinBox = new Vector2(80, 80);
        static List<RectTransform> Roots;
        public static void InsertRoot(RectTransform rect, int index = 0)
        {
            if (rect == null)
                return;
            if (Roots == null)
            {
                Roots = new List<RectTransform>();
                Roots.Add(rect);
                return;
            }
            for (int i = 0; i < Roots.Count; i++)
            {
                if (rect == Roots[i])
                {
                    Roots.RemoveAt(i);
                    break;
                }
            }
            if (index > Roots.Count)
                index = Roots.Count;
            Roots.Insert(index, rect);
        }
        internal static Container<EventCallBack> container = new Container<EventCallBack>();
        public Vector3[] Rectangular { get; private set; }
        /// <summary>
        /// 暂停事件
        /// </summary>
        public static bool PauseEvent;
        internal static void DispatchEvent(UserAction action)
        {
            if (PauseEvent)
                return;
            if (Roots == null)
                return;
            if (Roots != null)
                for (int j = 0; j < Roots.Count; j++)
                {
                    var t = Roots[j];
                    if (t != null)
                        for (int i = t.childCount - 1; i >= 0; i--)
                        {
                            var r = t.GetChild(i);
                            if (DispatchEvent(r as RectTransform, Vector3.zero, Vector3.one, Quaternion.identity, action))
                                goto label;
                        }
                }
            label:;
        }
        static bool DispatchEvent(RectTransform rt, Vector3 pos, Vector3 scale, Quaternion quate, UserAction action)
        {
            if (rt == null)
            {
                Debug.Log("null trans");
                return false;
            }
            if (!rt.gameObject.activeSelf)
                return false;
            Vector3 p = rt.localPosition;
            Vector3 o = Vector3.zero;
            o.x = p.x * scale.x;
            o.y = p.y * scale.y;
            o.z = p.z * scale.z;
            o += pos;
            Vector3 s = rt.localScale;
            Quaternion q = rt.localRotation * quate;
            s.x *= scale.x;
            s.y *= scale.y;
            EventCallBack callBack = container.Find((e)=> { return e.Target == rt; });
            if (callBack == null)
            {
                for (int i = rt.childCount - 1; i >= 0; i--)
                {
                    if (DispatchEvent(rt.GetChild(i) as RectTransform, o, s, q, action))
                    {
                        return true;
                    }
                }
            }
            else if (callBack.forbid)
            {
                for (int i = rt.childCount - 1; i >= 0; i--)
                {
                    if (DispatchEvent(rt.GetChild(i) as RectTransform, o, s, q, action))
                        return true;
                }
            }
            else
            {
                callBack.pgs = scale;
                callBack.GlobalScale = s;
                callBack.GlobalPosition = o;
                callBack.GlobalRotation = q;
                bool inside = false;
                float w = rt.sizeDelta.x * s.x;
                float h = rt.sizeDelta.y * s.y;
                if (!callBack.UseActualSize)
                {
                    if (w < MinBox.x)
                        w = MinBox.x;
                    if (h < MinBox.y)
                        h = MinBox.y;
                }
                if (callBack.IsCircular)
                {
                    float x = action.CanPosition.x - o.x;
                    float y = action.CanPosition.y - o.y;
                    w *= 0.5f;
                    if (x * x + y * y < w * w)
                        inside = true;
                }
                else
                {
                    float x1 = 0.5f * w;
                    float x0 = -x1;
                    float y1 = 0.5f * h;
                    float y0 = -y1;

                    var v = action.CanPosition;
                    var Rectangular = callBack.Rectangular;
                    Rectangular[0] = q * new Vector3(x0, y0) + o;
                    Rectangular[1] = q * new Vector3(x0, y1) + o;
                    Rectangular[2] = q * new Vector3(x1, y1) + o;
                    Rectangular[3] = q * new Vector3(x1, y0) + o;
                    inside = Physics2D.DotToPolygon(Rectangular, v);
                }
                if (inside)
                {
                    action.CurrentEntry.Add(callBack);
                    for (int i = rt.childCount - 1; i >= 0; i--)
                    {
                        if (DispatchEvent(rt.GetChild(i) as RectTransform, o, s, q, action))
                        {
                            if (callBack.ForceEvent)
                            {
                                if (!callBack.forbid)
                                    break;
                            }
                            return true;
                        }
                    }
                    if (action.IsLeftButtonDown | action.IsRightButtonPressed | action.IsMiddleButtonPressed)
                    {
                        callBack.OnMouseDown(action);
                    }
                    else if (action.IsLeftButtonUp | action.IsRightButtonUp | action.IsMiddleButtonUp)
                    {
                        if (callBack.Pressed)
                            callBack.OnMouseUp(action);
                    }
                    else
                    {
                        callBack.OnMouseMove(action);
                    }
                    if (callBack.Penetrate)
                        return false;
                    return true;
                }
                else if (!callBack.CutRect)
                {
                    for (int i = rt.childCount - 1; i >= 0; i--)
                    {
                        if (DispatchEvent(rt.GetChild(i) as RectTransform, o, s, q, action))
                            return true;
                    }
                }
            }
            return false;
        }
        internal static void Rolling()
        {
            for (int i = 0; i < container.Count; i++)
                if (container[i] != null)
                    if (!container[i].forbid)
                        if (!container[i].Pressed)
                            DuringSlide(container[i]);
        }
        static void DuringSlide(EventCallBack back)
        {
            if (back.mVelocity.x == 0 & back.mVelocity.y == 0)
                return;
            back.xTime += UserAction.TimeSlice;
            back.yTime += UserAction.TimeSlice;
            float x = 0, y = 0;
            bool endx = false, endy = false;
            if (back.mVelocity.x != 0)
            {
                float t = (float)MathH.PowDistance(back.DecayRateX, back.maxVelocity.x, back.xTime);
                x = t - back.lastX;
                back.lastX = t;
                float vx = Mathf.Pow(back.DecayRateX, back.xTime) * back.maxVelocity.x;
                if (vx < 0.001f & vx > -0.001f)
                {
                    back.mVelocity.x = 0;
                    endx = true;
                }
                else back.mVelocity.x = vx;
            }
            if (back.mVelocity.y != 0)
            {
                float t = (float)MathH.PowDistance(back.DecayRateY, back.maxVelocity.y, back.yTime);
                y = t - back.lastY;
                back.lastY = t;
                float vy = Mathf.Pow(back.DecayRateY, back.yTime) * back.maxVelocity.y;
                if (vy < 0.001f & vy > -0.001f)
                {
                    back.mVelocity.y = 0;
                    endy = true;
                }
                else back.mVelocity.y = vy;
            }
            if (back.Scrolling != null)
                back.Scrolling(back, new Vector2(x, y));
            if (endx)
                if (back.ScrollEndX != null)
                    back.ScrollEndX(back);
            if (endy)
                if (back.ScrollEndY != null)
                    back.ScrollEndY(back);
        }

        public static void ReleaseEvent(RectTransform rect)
        {
            int index = container.FindIndex((o) => { return o.Target == rect; });
            if (index>-1)
            {
                var ins = container[index];
                container.RemoveAt(index);
                var ua = ins.FocusAction;
                if (ua != null)
                    ua.RemoveFocus(ins);
            }
        }

        public static T RegEvent<T>(RectTransform rect) where T : EventCallBack, new()
        {
            if (rect == null)
                return null;
            int index = container.FindIndex((o)=> { return o.Target == rect; });
            if(index>-1)
            {
                T t =container[index] as T;
                if (t != null)
                    return t;
                container.RemoveAt(index);
            }
            T back = new T();
            back.Target = rect;
            container.Add(back);
            return back;
        }
        public static EventCallBack RegEvent(RectTransform rect, Type type)
        {
            if (rect == null)
                return null;
            if (type == typeof(EventCallBack))
            {
            }
            else if (type.IsSubclassOf(typeof(EventCallBack)))
            {
            }
            else return null;
            int index = container.FindIndex((o) => { return o.Target == rect; });
            if (index > -1)
                container.RemoveAt(index);
            EventCallBack back = Activator.CreateInstance(type) as EventCallBack;
            back.Target = rect;
            container.Add(back);
            return back;
        }
        public static void ClearRoot()
        {
            if (Roots != null)
                Roots.Clear();
        }
        public static void ClearEvent()
        {
            container.Clear();
            UserAction.ClearAll();
            TextInputEvent.Reset();
        }

        static void Reset(EventCallBack eventCall)
        {
            eventCall.PointerDown = null;
            eventCall.PointerUp = null;
            eventCall.Click = null;
            eventCall.PointerEntry = null;
            eventCall.PointerMove = null;
            eventCall.PointerLeave = null;
            eventCall.Drag = null;
            eventCall.DragEnd = null;
            eventCall.Scrolling = null;
            eventCall.AutoColor = true;
            eventCall.forbid = false;
            eventCall.mVelocity = Vector2.zero;
            eventCall.maxVelocity = Vector2.zero;
            eventCall.CutRect = false;
            eventCall.ForceEvent = false;
            eventCall.Penetrate = false;
        }
        Vector2 mVelocity;
        public float VelocityX { get { return mVelocity.x; } set { maxVelocity.x = mVelocity.x = value; RefreshRateX(); } }
        public float VelocityY { get { return mVelocity.y; } set { maxVelocity.y = mVelocity.y = value; RefreshRateY(); } }
        public void StopScroll()
        {
            mVelocity.x = 0;
            mVelocity.y = 0;
        }
        int xTime;
        int yTime;
        float lastX;
        float lastY;
        Vector2 maxVelocity;
        Vector2 sDistance;
        public float ScrollDistanceX
        {
            get { return sDistance.x; }
            set
            {
                if (value == 0)
                    maxVelocity.x = 0;
                else
                    maxVelocity.x = (float)MathH.DistanceToVelocity(DecayRateX, value);
                mVelocity.x = maxVelocity.x;
                sDistance.x = value;
                xTime = 0;
                lastX = 0;
            }
        }
        public float ScrollDistanceY
        {
            get { return sDistance.y; }
            set
            {
                if (value == 0)
                    maxVelocity.y = 0;
                else
                    maxVelocity.y = (float)MathH.DistanceToVelocity(DecayRateY, value);
                mVelocity.y = maxVelocity.y;
                sDistance.y = value;
                yTime = 0;
                lastY = 0;
            }
        }
        public float DecayRateX = 0.998f;
        public float DecayRateY = 0.998f;
        public float speed = 1f;
        public static long ClickTime = 1800000;
        public static float ClickArea = 400;
        public Vector2 RawPosition { get; protected set; }
        Vector2 LastPosition;
        public int HoverTime { get; protected set; }
        public long pressTime { get; internal set; }
        public long entryTime { get; protected set; }
        public long stayTime { get; protected set; }
        public bool Pressed { get; internal set; }
        protected bool forbid;
        public bool Forbid
        {
            get { return forbid; }
            set
            {
                forbid = value;
                if (AutoColor)
                {
                    var g = Target.GetComponent<Graphic>();
                    if (g != null)
                    {
                        if (forbid)
                        {
                            Color a = Color.white;
                            a.r = g_color.r * 0.6f;
                            a.g = g_color.g * 0.6f;
                            a.b = g_color.b * 0.6f;
                            a.a = g_color.a;
                            g.color = a;
                        }
                        else
                        {
                            if (Pressed)
                            {
                                Color a = Color.white;
                                a.r = g_color.r * 0.8f;
                                a.g = g_color.g * 0.8f;
                                a.b = g_color.b * 0.8f;
                                a.a = g_color.a;
                                g.color = a;
                            }
                            else
                            {
                                g.color = g_color;
                            }
                        }
                    }
                }
            }
        }
        public bool CutRect = false;
        /// <summary>
        /// 强制事件不被子组件拦截
        /// </summary>
        public bool ForceEvent = false;
        /// <summary>
        /// 允许事件穿透
        /// </summary>
        public bool Penetrate = false;
        /// <summary>
        /// 当此项开启时忽略最小尺寸校正
        /// </summary>
        public bool UseActualSize = false;
        public bool IsCircular = false;
        public bool entry { get; protected set; }
        private int index;
        public bool AutoColor = true;
        Color g_color;
        public object DataContext;
        Vector3 pgs = Vector3.one;
        public Vector3 GlobalScale = Vector3.one;
        public Vector3 GlobalPosition;
        public Quaternion GlobalRotation;

        protected RectTransform m_Target;
        public Graphic graphic;
        public virtual RectTransform Target
        {
            get { return m_Target; }
            protected set
            {
                m_Target = value;
                graphic = m_Target.GetComponent<Graphic>();
                if (graphic != null)
                    g_color = graphic.color;
            }
        }
        public Action<EventCallBack, UserAction> PointerDown;
        public Action<EventCallBack, UserAction> PointerUp;
        public Action<EventCallBack, UserAction> Click;
        public Action<EventCallBack, UserAction> PointerEntry;
        public Action<EventCallBack, UserAction> PointerMove;
        public Action<EventCallBack, UserAction> PointerLeave;
        public Action<EventCallBack, UserAction> PointerHover;
        public Action<EventCallBack, UserAction> MouseWheel;
        public Action<EventCallBack, UserAction, Vector2> Drag;
        public Action<EventCallBack, UserAction, Vector2> DragEnd;
        public Action<EventCallBack, Vector2> Scrolling;
        public Action<EventCallBack> ScrollEndX;
        public Action<EventCallBack> ScrollEndY;
        public Action<EventCallBack, UserAction> LostFocus;

        UserAction FocusAction;
        public EventCallBack()
        {
            Rectangular = new Vector3[4];
        }
        public virtual void OnMouseDown(UserAction action)
        {
            if (!action.MultiFocus.Contains(this))
                action.MultiFocus.Add(this);
            if (AutoColor)
            {
                if (graphic != null)
                {
                    Color a = Color.white;
                    a.r = g_color.r * 0.8f;
                    a.g = g_color.g * 0.8f;
                    a.b = g_color.b * 0.8f;
                    a.a = g_color.a;
                    graphic.color = a;
                }
            }
            Pressed = true;
            pressTime = action.EventTicks;
            RawPosition = action.CanPosition;
            if (PointerDown != null)
                PointerDown(this, action);
            entry = true;
            FocusAction = action;
            mVelocity = Vector2.zero;
        }
        protected virtual void OnMouseUp(UserAction action)
        {
            Pressed = false;
            entry = false;
            if (AutoColor)
            {
                if (graphic != null)
                    if (!forbid)
                        graphic.color = g_color;
            }
            if (PointerUp != null)
                PointerUp(this, action);
            long r = UserAction.Ticks - pressTime;
            if (r <= ClickTime)
            {
                float x = RawPosition.x - action.CanPosition.x;
                float y = RawPosition.y - action.CanPosition.y;
                x *= x;
                y *= y;
                x += y;
                if (x < ClickArea)
                    if (Click != null)
                        Click(this, action);
            }
        }
        protected virtual void OnMouseMove(UserAction action)
        {
            if (!entry)
            {
                entry = true;
                entryTime = DateTime.Now.Ticks;
                if (PointerEntry != null)
                    PointerEntry(this, action);
                LastPosition = action.CanPosition;
            }
            else
            {
                stayTime = action.EventTicks - entryTime;
                if (action.CanPosition == LastPosition)
                {
                    HoverTime += UserAction.TimeSlice * 10000;
                    if (HoverTime > ClickTime)
                        if (PointerHover != null)
                            PointerHover(this, action);
                }
                else
                {
                    HoverTime = 0;
                    LastPosition = action.CanPosition;
                    if (PointerMove != null)
                        PointerMove(this, action);
                }
            }
        }
        internal virtual void OnMouseLeave(UserAction action)
        {
            entry = false;
            if (m_Target == null)
                return;
            if (m_Target.gameObject == null)
                return;
            if (AutoColor)
            {
                if (graphic != null)
                    if (!forbid)
                        graphic.color = g_color;
            }
            if (PointerLeave != null)
                PointerLeave(this, action);
        }
        internal virtual void OnFocusMove(UserAction action)
        {
            if (Pressed)
                OnDrag(action);
        }
        protected virtual void OnDrag(UserAction action)
        {
            if (Drag != null)
            {
                var v = action.Motion;
                v.x /= pgs.x;
                v.y /= pgs.y;
                Drag(this, action, v);
            }
        }
        internal virtual void OnDragEnd(UserAction action)
        {
            if (Scrolling != null)
            {
                var v = action.Velocities;
                v.x /= GlobalScale.x;
                v.y /= GlobalScale.y;
                maxVelocity = mVelocity = v;
                RefreshRateX();
                RefreshRateY();
            }
            if (DragEnd != null)
            {
                var v = action.Motion;
                v.x /= pgs.x;
                v.y /= pgs.y;
                DragEnd(this, action, v);
            }
        }
        internal virtual void OnLostFocus(UserAction action)
        {
            FocusAction = null;
            if (LostFocus != null)
                LostFocus(this, action);
        }
        public virtual void Reset()
        {
            Reset(this);
        }
        void RefreshRateX()
        {
            xTime = 0;
            lastX = 0;
            if (maxVelocity.x == 0)
                sDistance.x = 0;
            else
                sDistance.x = (float)MathH.PowDistance(DecayRateX, maxVelocity.x, 1000000);
        }
        void RefreshRateY()
        {
            yTime = 0;
            lastY = 0;
            if (maxVelocity.y == 0)
                sDistance.y = 0;
            else
                sDistance.y = (float)MathH.PowDistance(DecayRateY, maxVelocity.y, 1000000);
        }
        public Vector3 ScreenToLocal(Vector3 v)
        {
            v -= GlobalPosition;
            if (GlobalScale.x != 0)
                v.x /= GlobalScale.x;
            else v.x = 0;
            if (GlobalScale.y != 0)
                v.y /= GlobalScale.y;
            else v.y = 0;
            if (GlobalScale.z != 0)
                v.z /= GlobalScale.z;
            else v.z = 0;
            var q = Quaternion.Inverse(GlobalRotation);
            v = q * v;
            return v;
        }
        protected virtual void Initial()
        {
        }
    }
}
