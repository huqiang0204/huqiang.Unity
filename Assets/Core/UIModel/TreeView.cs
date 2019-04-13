using huqiang.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public class TreeViewNode
    {
        public bool extand;
        public string content;
        public Vector2 offset;
        public List<TreeViewNode> child = new List<TreeViewNode>();
    }
    public class TreeViewItem
    {
        public GameObject target;
        public Text text;
        public EventCallBack callBack;
        public TreeViewNode node;
    }
    public class TreeView : ModelInital
    {
        public RectTransform View;
        public Vector2 Size;//scrollView的尺寸
        Vector2 aSize;
        public Vector2 ItemSize;
        ModelElement model;
        public TreeViewNode nodes;
        public float ItemHigh = 16;
        public EventCallBack eventCall;//scrollY自己的按钮
        public ModelElement ItemMod;
        float m_point;
        SwapBuffer<TreeViewItem, TreeViewNode> swap;
        public override void Initial(RectTransform rect, ModelElement model)
        {
            View = rect;
            eventCall = EventCallBack.RegEventCallBack<EventCallBack>(rect);
            eventCall.Drag = (o, e, s) => { Scrolling(o, s); };
            eventCall.DragEnd = (o, e, s) => { Scrolling(o, s); };
            eventCall.Scrolling = Scrolling;
            eventCall.ForceEvent = true;
            eventCall.AutoColor = false;
            Size = View.sizeDelta;
            View.anchorMin = View.anchorMax = View.pivot = ScrollContent.Center;
            eventCall.CutRect = true;
            if (model != null)
            {
                ItemMod = model.FindChild("Item");
                if (ItemMod != null)
                {
                    ItemSize = ItemMod.transAttribute.sizeDelta;
                    ItemHigh = ItemSize.y;
                }
            }
            swap = new SwapBuffer<TreeViewItem, TreeViewNode>(512);
        }
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
            if (View == null)
                return;
            v.y /= eventCall.Target.localScale.y;
            Limit(back, v.y);
            Refresh();
        }
        void OnScrollEnd(EventCallBack back)
        {

        }
        float hy;
        public void Refresh()
        {
            if (nodes == null)
                return;
            hy = Size.y * 0.5f;
            aSize.y = CalculHigh(nodes, 0, 0);
            var tmp = swap.Done();
            if (tmp != null)
                RecycleItem(tmp);
        }
        protected void RecycleItem(TreeViewItem[] items)
        {
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
                    ModelManager.RecycleGameObject(it.target);
                }
            }
        }

        float CalculHigh(TreeViewNode node, int level, float high)
        {
            node.offset.x = level * ItemHigh;
            node.offset.y = high;
            UpdateItem(node);
            level++;
            high += ItemHigh;
            if (node.extand)
                for (int i = 0; i < node.child.Count; i++)
                    high = CalculHigh(node.child[i], level, high);
            return high;
        }
        void UpdateItem(TreeViewNode node)
        {
            float dy = node.offset.y - m_point;
            if (dy <= Size.y)
                if (dy + ItemHigh > 0)
                {
                    var item = swap.Exchange((o, e) => { return o.node == e; }, node);
                    if (item == null)
                    {
                        item = CreateItem();
                        swap.Add(item);
                        item.node = node;
                        if (item.text != null)
                        {
                            if (node.child.Count > 0)
                                item.text.text = "▷ " + node.content;
                            else item.text.text = node.content;
                        }

                    }
                    item.callBack.Target.localPosition = new Vector3(node.offset.x, hy - dy - ItemHigh * 0.5f, 0);
                }
        }
        protected TreeViewItem[] Buff = new TreeViewItem[512];
        int buffPoint = 0;
        protected int max_count;
        protected TreeViewItem CreateItem()
        {
            if (buffPoint > 0)
            {
                buffPoint--;
                var it = Buff[buffPoint];
                it.target.SetActive(true);
                return it;
            }
            GameObject g = ModelManager.LoadToGame(ItemMod, null, null, "");
            var t = g.transform;
            t.SetParent(View);
            t.localPosition = new Vector3(10000, 10000);
            t.localScale = Vector3.one;
            t.localEulerAngles = Vector3.zero;
            TreeViewItem a = new TreeViewItem();
            a.target = g;
            a.text = g.GetComponent<Text>();
            a.callBack = EventCallBack.RegEventCallBack<EventCallBack>(g.transform as RectTransform);
            a.callBack.Click = (o, e) => {
                var item = o.DataContext as TreeViewItem;
                if (item.node != null)
                {
                    item.node.extand = !item.node.extand;
                    Refresh();
                }
            };
            a.callBack.DataContext = a;
            return a;
        }
        protected void Limit(EventCallBack callBack, float y)
        {
            var size = Size;
            if (size.y > aSize.y)
            {
                m_point = 0;
                return;
            }
            if (y == 0)
                return;
            float vy = m_point + y;
            if (vy < 0)
            {
                m_point = 0;
                eventCall.VelocityY = 0;
                return;
            }
            else if (vy + size.y > aSize.y)
            {
                m_point = aSize.y - size.y;
                eventCall.VelocityY = 0;
                return;
            }
            m_point += y;
        }
    }
}
