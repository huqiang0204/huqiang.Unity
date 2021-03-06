﻿using huqiang;
using huqiang.UIModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIComposite
{
    public class DropdownEx : ModelInitalS
    {
        public class PopItemMod
        {
            public EventCallBack Item;
            public Text Label;
            public GameObject Checked;
            [NonSerialized]
            public object data;
            [NonSerialized]
            public int Index;
        }
        public RectTransform main;
        ScrollY m_scroll;
        public ScrollY scrollY
        {
            get { return m_scroll; }
            set
            {
                m_scroll = value;
                if (value == null)
                    return;
                m_scroll.ItemObject = typeof(PopItemMod);
                ItemSize = m_scroll.ItemSize;
                MaxHeight = m_scroll.ScrollView.sizeDelta.y;
                m_scroll.ScrollView.gameObject.SetActive(false);
            }
        }
        public ModelElement ItemMod;
        IList DataList;
        public object BindingData { get { return DataList; }set { DataList = value as IList; } }
        public bool down = true;
        public float MaxHeight = 300;
        public float PopOffset = 0;
        public Vector2 ItemSize;
        int s_index;
        public EventCallBack callBack;
        public int SelectIndex
        {
            get { return s_index; }
            set
            {
                if (BindingData == null)
                    return;
                if (value < 0)
                {
                    s_index = -1;
                    if (ShowLabel != null)
                        ShowLabel.text = "";
                    return;
                }
                if (value >= DataList.Count)
                    value = DataList.Count - 1;
                s_index = value;
                if (ShowLabel != null)
                {
                    var dat = DataList[s_index];
                    if (dat is string)
                        ShowLabel.text = dat as string;
                    else ShowLabel.text = dat.ToString();
                }
            }
        }
        public DropdownEx()
        {
        }
        public override void Initial( ModelElement mod)
        {
            main = mod.Context;
            ShowLabel = main.GetComponentInChildren<Text>();
            callBack = EventCallBack.RegEvent<EventCallBack>(main);
            callBack.Click = Show;
        }
        public DropdownEx(ScrollY scroll, EventCallBack parent)
        {
            main = parent.Target;
            parent.Click = Show;
            ShowLabel = main.GetComponentInChildren<Text>();
            scrollY = scroll;
        }
        void Show(EventCallBack back, UserAction action)
        {
            if (m_scroll != null)
            {
                if (ItemMod != null)
                    m_scroll.ItemMod = ItemMod;
                m_scroll.BindingData= BindingData;
                m_scroll.SetItemUpdate<object, object>(ItemUpdate);
                m_scroll.eventCall.LostFocus = LostFocus;
                m_scroll.eventCall.DataContext = this;

                main.SetAsLastSibling();
                Dock();
                action.AddFocus(m_scroll.eventCall);
            }
        }
        void Dock()
        {
            if (BindingData == null)
                return;
            m_scroll.ScrollView.gameObject.SetActive(true);
            float x = main.sizeDelta.x;
            int c = DataList.Count;
            float height = c * ItemSize.y;
            if (height > MaxHeight)
                height = MaxHeight;
            scrollY.ScrollView.sizeDelta = new Vector2(x, height);

            var y = main.sizeDelta.y * 0.5f + height * 0.5f;
            var t = scrollY.ScrollView;
            t.SetParent(main);
            if (down)
                t.localPosition = new Vector3(PopOffset, -y, 0);
            else t.localPosition = new Vector3(PopOffset, y, 0);
            ItemSize.x = x;
            scrollY.ItemSize = ItemSize;
            float h = ItemSize.y * SelectIndex;
            scrollY.Refresh(0, h);
        }
        public Action<DropdownEx, object> OnSelectChanged;
        public Text ShowLabel;

        void LostFocus(EventCallBack eve, UserAction action)
        {
            m_scroll.ScrollView.gameObject.SetActive(false);
        }
        GameObject Checked;
        void ItemUpdate(object g, object o, int index)
        {
            PopItemMod button = g as PopItemMod;
            if (button == null)
                return;

            if (button.Item != null)
                (button.Item.Target.GetChild(0) as RectTransform).sizeDelta = new Vector2(ItemSize.x - 20, ItemSize.y - 10);
            if (button.Label != null)
                button.Label.rectTransform.sizeDelta = new Vector2(ItemSize.x - 20, ItemSize.y - 10);
            button.Index = index;
            button.data = o;
            if (button.Item != null)
            {
                button.Item.DataContext = button;
                button.Item.Click = ItemClick;
            }
            if (button.Label != null)
            {
                if (o is string)
                    button.Label.text = o as string;
                else button.Label.text = o.ToString();
            }
            if (button.Checked != null)
            {
                if (index == SelectIndex)
                {
                    button.Checked.SetActive(true);
                    Checked = button.Checked;
                }
                else button.Checked.SetActive(false);
            }
        }
        void ItemClick(EventCallBack eventCall, UserAction action)
        {
            if (Checked != null)
                Checked.SetActive(false);
            PopItemMod mod = eventCall.DataContext as PopItemMod;
            if (mod == null)
                return;
            if (mod.Checked != null)
                mod.Checked.SetActive(true);
            SelectIndex = mod.Index;
            if (ShowLabel != null)
            {
                if (mod.data is string)
                    ShowLabel.text = mod.data as string;
                else ShowLabel.text = mod.data.ToString();
            }
            if (OnSelectChanged != null)
                OnSelectChanged(this, mod.data);
            scrollY.ScrollView.gameObject.SetActive(false);
        }
    }
}
