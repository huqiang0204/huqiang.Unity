using huqiang.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public class EmojiInput
    {
        class InputView
        {
            public EventCallBack cancel;
            public RawImage back;
            public RawImage line;
            public EmojiText input;
            public Image done;
            public ScrollY scroll;
        }
        class ScrollItem
        {
            public EventCallBack Item;
        }
        static EmojiText target;
        public static void ShowInput(EmojiText suport)
        {
            target = suport;
            ShowInput();
            //view.input.InputString = suport.text;
        }
        public static void ShowInput(EmojiText suport, Sprite close)
        {
            target = suport;
            ShowInput();
        }
        public static void ShowInput(EmojiText suport,Sprite close, Color backColor,Color LineColor)
        {
            target = suport;
            ShowInput();
        }
        static ModelElement model;
        static InputView view;
        static Texture2D t2d;
        static Material mat;
        static void ShowInput()
        {
            view = new InputView();
            //if (model == null)
            //    model = ModelManagerUI.LoadToGame("EmojiInput", view, Page.Root);
            //else
                ModelManagerUI.LoadToGame(model, view, Page.Root);
            view.scroll.DataLength = EmojiMap.Length;
            view.scroll.ItemObject = typeof(ScrollItem);
            view.scroll.ItemSize = new Vector2(108, 108);
            view.scroll.ItemUpdate = ItemUpdate;
            view.scroll.Refresh();
            view.cancel.Click = (o, e) => { ModelManagerUI.RecycleGameObject(model.Main); };
        }
        static CharUV charUV;
        static void ItemUpdate(object mod,object dat,int index)
        {
             var content = EmojiMap.GetChar(index,ref charUV);
            var item = mod as ScrollItem;
            var raw = item.Item.graphic as RawImage;
            raw.texture = EmojiText.Emoji;
            float x = charUV.uv0.x;
            float y = charUV.uv1.y;
            float x2 = charUV.uv2.x;
            float y2 = charUV.uv2.y;
            raw.uvRect = new Rect(x, y2, x2-x,y-y2);
            item.Item.Click = ItemClick;
            item.Item.DataContext = content;
        }
        static void ItemClick(EventCallBack eve,UserAction action)
        {
            view.input.text = new string(eve.DataContext as char[]);
        }
    }
}
