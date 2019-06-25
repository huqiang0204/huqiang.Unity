﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using huqiang.UIModel;
using UnityEngine;

namespace huqiang
{
    public class LinkerMod
    {
        public object UI;
        public GameObject main;
    }
    public interface Linker
    {
        LinkerMod CreateUI();
        void RefreshItem(object t, object u, int index);
    }
    public class UILinker<T, U> : Linker where T : class, new() where U : class, new()
    {
        ModelElement model;
        public Action<T, U, int> ItemUpdate;
        UIContainer con;
        public UILinker(UIContainer container,ModelElement mod)
        {
            con = container;
            model = mod;
        }
        public void AddData(U dat,float high)
        {
            con.AddData(this, dat,high);
        }
        public LinkerMod CreateUI()
        {
            T ui = new T();
            LinkerMod mod = new LinkerMod();
            mod.main = ModelManagerUI.LoadToGame(model, ui, null, "");
            mod.UI = ui;
            return mod;
        }
        public void RefreshItem(object t, object u, int index)
        {
            if (ItemUpdate != null)
                ItemUpdate(t as T, u as U, index);
        }
    }
    public class ObjectLinker : Linker 
    {
        ModelElement model;
        public Action<object, object, int> ItemUpdate;
        public Action<ObjectLinker, LinkerMod> ItemCreate;
        UIContainer con;
        public ObjectLinker(UIContainer container, ModelElement mod)
        {
            con = container;
            model = mod;
        }
        public void AddData(object dat, float high)
        {
            con.AddData(this, dat, high);
        }
        public LinkerMod CreateUI()
        {
            LinkerMod mod = new LinkerMod();
            mod.main = ModelManagerUI.LoadToGame(model, null, null, "");
            if (ItemCreate != null)
                ItemCreate(this,mod);
            return mod;
        }
        public void RefreshItem(object t, object u, int index)
        {
            if (ItemUpdate != null)
                ItemUpdate(t , u , index);
        }
    }
    public class UIContainer
    {
        class Item
        {
            public Linker linker;
            public object UI;
            public object Data;
            public BindingData binding;
            public int Index = -1;
            public Vector3 pos;
            public Vector2 size;
            public GameObject main;
            public float offset;
            public float high;
        }
        class BindingData
        {
            public float offset;
            public float high;
            public object Data;
            public Linker linker;
        }
        public EventCallBack eventCall;
        public RectTransform View;
        public Action<UIContainer, Vector2> Scroll;
        public UIContainer(RectTransform rect)
        {
            items = new List<Item>();
            datas = new List<BindingData>();
            View = rect;
            eventCall = EventCallBack.RegEvent<EventCallBack>(rect);
            eventCall.AutoColor = false;
            eventCall.ForceEvent = true;
            eventCall.Drag = eventCall.DragEnd = (o, e, s) => { Scrolling(o, s); };
            eventCall.Scrolling = Scrolling;
        }
        List<Item> items;
        List<BindingData> datas;
        /// <summary>
        /// 当前指针
        /// </summary>
        float point=0;
        float start=0;
        float end=0;
        float ch=0;
        public float Height { get { return ch; } }
        public float Pos { get {
                float p= (point -start)/ (end - start - View.sizeDelta.y);
                if (p < 0)
                    p = 0;
                return p;
            }set {
                if (end - start < View.sizeDelta.y)
                    return;
                if (value > 1)
                    value = 1;
                else if (value < 0)
                    value = 0;
                point = start + value * (ch - View.sizeDelta.y);
                point -= datas[datas.Count - 1].high;
                Order();
            } }
        public int DataCount { get { return datas.Count; } }
        public void AddData(Linker linker, object data,float high)
        {
            BindingData binding = new BindingData();
            binding.linker = linker;
            binding.Data = data;
            binding.high = high;
            binding.offset = end;
            datas.Add(binding);
            end += high;
            ch = end - start;
            if (ch < View.sizeDelta.y)
                ch = View.sizeDelta.y;
        }
        Item CreateItem(BindingData data, int index)
        {
            var ui = data.linker.CreateUI();
            data.linker.RefreshItem(ui.UI, data.Data, index);
            Item item = new Item();
            item.linker = data.linker;
            item.Index = index;
            item.binding = data;
            item.Data = data.Data;
            item.UI = ui.UI;
            item.offset = data.offset;
            item.high = data.high;
            item.main = ui.main;
            var son = ui.main.transform;
            son.SetParent(View);
            son.localScale = Vector3.one;
            return item;
        }
        void Scrolling(EventCallBack scroll, Vector2 offset)
        {
            if(ch> View.sizeDelta.y)
            {
                Refresh(offset.y);
                if (Scroll != null)
                    Scroll(this, offset);
            }
        }
        public void Refresh(float offset)
        {
            float vy = View.sizeDelta.y;
            if(end-start<vy)
            {
                offset = 0;
                eventCall.StopScroll();
            }
            point += offset;
            if (point<start)
            {
                point = start;
                eventCall.StopScroll();
            }else 
            {
                float e = vy ;
                if (point-start+e > ch)
                {
                    point = end - start - e;
                    eventCall.StopScroll();
                }
            }
            Order();
        }
        void Order()
        {
            float vy = View.sizeDelta.y;
            int c = items.Count - 1;
            int ss = 0;
            for (int i = c; i >= 0; i--)
            {
                var item = items[i];
                float sy = item.offset;
                float ey = sy + item.high;
                if (sy - point > vy)
                {
                    ModelManagerUI.RecycleGameObject(item.main);
                    items.RemoveAt(i);
                }
                else if (ey < point)
                {
                    ModelManagerUI.RecycleGameObject(item.main);
                    items.RemoveAt(i);
                }
                else
                {
                    float oy = 0.5f * vy - sy + point;
                    item.pos.y = oy;
                    item.main.transform.localPosition = new Vector3(0, oy, 0);
                    ss = item.Index;
                }
            }
            ss -= 10;
            if (ss < 0)
                ss = 0;
            float over = point + vy;
            for (; ss < datas.Count; ss++)
            {
                var item = datas[ss];
                if (item.offset > over)
                    break;
                if (item.offset + item.high > point)
                    UpdateItem(item, ss);
            }
        }
        void UpdateItem(BindingData data,int index)
        {
            for (int i = 0; i < items.Count; i++)
                if (index == items[i].Index)
                    return;
            var item = CreateItem(data, index);
            items.Add(item);
            float vy = View.sizeDelta.y;
            float oy = 0.5f * vy - data.offset + point;
            item.pos.y = oy;
            item.pos = new Vector3(0, oy, 0);
            item.main.transform.localPosition = item.pos;
            item.main.transform.localScale = Vector3.one;
        }
        public void Clear()
        {
            for (int i = 0; i < items.Count; i++)
                ModelManagerUI.RecycleGameObject(items[i].main);
            items.Clear();
            point = 0;
            start = 0;
            end = 0;
            ch = 0;
            datas.Clear();
        }
    }
}