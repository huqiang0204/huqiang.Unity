using huqiang;
using huqiang.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace huqiang.UIModel
{
    public enum ScrollType
    {
        None, Loop, BounceBack
    }
    public class ScrollContent : ModelInital
    {
        public ScrollType scrollType;
        public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        public RectTransform ScrollView;
        public Vector2 Size;//scrollView的尺寸
        public Vector2 ActualSize { get; protected set; }//相当于Content的尺寸
        public Vector2 ItemSize;
        ModelElement model;
        public Type ItemObject = typeof(GameObject);
        public ModelElement ItemMod
        {
            set
            {
                model = value;
                var c = Items.Count;
                if (c > 0)
                {
                    for (int i = 0; i < Items.Count; i++)
                        ModelManagerUI.RecycleGameObject(Items[i].target);
                    Items.Clear();
                }
            }
            get { return model; }
        }
        public Action<object , object , int > ItemUpdate;
        IList dataList;
        Array array;
        FakeArray fakeStruct;
        /// <summary>
        /// 传入类型为IList
        /// </summary>
        public object BindingData
        {
            get
            {
                if (dataList != null)
                    return dataList;
                if (array != null)
                    return array;
                return fakeStruct;
            }
            set
            {
                if (value is IList)
                {
                    dataList = value as IList;
                    array = null;
                    fakeStruct = null;
                }
                else if (value is Array)
                {
                    dataList = null;
                    array = value as Array;
                    fakeStruct = null;
                }
                else if (value is FakeArray)
                {
                    dataList = null;
                    array = null;
                    fakeStruct = value as FakeArray;
                }
                else
                {
                    dataList = null;
                    array = null;
                    fakeStruct = null;
                }
            }
        }
        int m_len;
        public int DataLength 
        {
            set { m_len = value; }
            get
            {
                if (dataList != null)
                    return dataList.Count;
                if (array != null)
                    return array.Length;
                if (fakeStruct != null)
                    return fakeStruct.Length;
                return m_len;
            }
        }
        protected  object GetData(int index)
        {
            if (dataList != null)
                return dataList[index];
            if (array != null)
                return array.GetValue(index);
            return null;
        }
        public Vector2 ItemOffset = Vector2.zero;
        public List<ScrollItem> Items;
        ScrollItem[] TmpPool;
        int tmpPoint = 0;
        protected ScrollItem[] Buff;
        int buffPoint=0;
        protected int max_count;
        public ScrollContent()
        {
            Items = new List<ScrollItem>();
            Buff =new ScrollItem[512];
            TmpPool = new ScrollItem[1024];
        }
        /// <summary>
        /// 当无法使用跨域反射时，使用此委托进行间接反射
        /// </summary>
        public Action<ScrollItem , GameObject > Reflection;
        /// <summary>
        /// 当某个ui超出Mask边界，被回收时调用
        /// </summary>
        public Action<ScrollItem> ItemRecycle;
        public override void Initial(RectTransform rect, ModelElement model)
        {
            ScrollView = rect;
        }
        public virtual void Refresh(float x = 0, float y = 0)
        {
        }
        protected void Initialtems()
        {
            int x =(int)( Size.x / ItemSize.x)+2;
            int y= (int)(Size.y/ ItemSize.y) + 2;
            max_count = x * y;
        }
        protected void PushItems()
        {
            int c = Items.Count;
            tmpPoint = c;
            int top = c - 1;
            for (int i = 0; i < c; i++)
            {
                TmpPool[top] = Items[i];
                top--;
            }
        }
        protected ScrollItem PopItem(int index)
        {
            for(int i=0;i<tmpPoint;i++)
            {
                var t = TmpPool[i];
                if (index==t.index)
                {
                    tmpPoint--;
                    TmpPool[i] = TmpPool[tmpPoint];
                    return t;
                }
            }
            var it = CreateItem();
            Items.Add(it);
            return it;
        }
        protected ScrollItem CreateItem()
        {
            if (buffPoint> 0)
            {
                buffPoint--;
                var it = Buff[buffPoint];
                it.target.SetActive(true);
                return it;
            }
            object obj = null;
            if (ItemObject != typeof(GameObject))
                obj = Activator.CreateInstance(ItemObject);
            GameObject g;
            if (Reflection == null)
                g = ModelManagerUI.LoadToGame(ItemMod, obj, null, "");
            else g = ModelManagerUI.LoadToGame(ItemMod,null,null,"");
            var t = g.transform;
            t.SetParent(ScrollView);
            t.localPosition = new Vector3(10000, 10000);
            t.localScale = Vector3.one;
            t.localEulerAngles = Vector3.zero;
            ScrollItem a = new ScrollItem();
            a.target = g;
            a.obj = obj;
            if (Reflection != null)
                Reflection(a, a.target);
            return a;
        }
        /// <summary>
        /// 回收范围内的条目
        /// </summary>
        /// <param name="down"></param>
        /// <param name="top"></param>
        protected void RecycleInside(int down, int top)
        {
            int c = Items.Count - 1;
            for (; c >= 0; c--)
            {
                var it = Items[c];
                int index = Items[c].index;
                if (index >= down & index <= top)
                {
                    RecycleItem(it, c);
                }
            }
        }
        /// <summary>
        /// 回收范围外的条目
        /// </summary>
        /// <param name="down"></param>
        /// <param name="top"></param>
        protected void RecycleOutside(int down, int top)
        {
            int c = Items.Count - 1;
            for (; c >= 0; c--)
            {
                var it = Items[c];
                int index = Items[c].index;
                if (index < down | index > top)
                {
                    RecycleItem(it, c);
                }
            }
        }
        protected void RecycleItem(ScrollItem it, int index)
        {
            Items.RemoveAt(index);
            it.target.SetActive(false);
            if(buffPoint<512)
            {
                it.index = -1;
                Buff[buffPoint] = it;
                buffPoint++;
            }
            else
            {
                ModelManagerUI.RecycleGameObject(it.target);
            }
            if (ItemRecycle != null)
                ItemRecycle(it);
        }
        public virtual void Order(float os, bool force = false)
        {
        }
        public void Clear()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var g = Items[i];
                ModelManagerUI.RecycleGameObject(g.target);
            }
            Items.Clear();
            for(int i=0;i<buffPoint;i++)
            {
                var g = Buff[i];
                ModelManagerUI.RecycleGameObject(g.target);
            }
            buffPoint = 0;
        }
        protected void RecycleItem(ScrollItem[] items)
        {
            if (items == null)
                return;
            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                if (buffPoint < 512)
                {
                    Buff[buffPoint] = it;
                    buffPoint++;
                    it.target.SetActive(false);
                }
                else
                {
                    ModelManagerUI.RecycleGameObject(it.target);
                }
            }
        }
    }
}