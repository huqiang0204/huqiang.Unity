﻿using System;
using System.Collections.Generic;
using huqiang.UIModel;
using huqiang.UIComposite;
using huqiang.UIEvent;
using UnityEngine;
using UnityEngine.UI;

public class LayoutTestPage : UIPage
{
    class View
    {
        public Layout Layout;
    }
    View view;
    public override void Initial(ModelElement parent, object dat = null)
    {
        model = ModelManagerUI.CloneModel("baseUI", "layout");
        view = LoadUI<View>("baseUI", "layout");
        base.Initial(parent, dat);
        var area = view.Layout.MainArea;
        area.auxiliary.AddContent("page0");
        var d = area.AddArea(LayoutArea.Dock.Down, 0.3f);
        var context = d.auxiliary.AddContent("page1");

        d.model.Context.GetComponent<Image>().color = Color.red;
        var one = d.AddArea(LayoutArea.Dock.Right, 0.4f);
        context = one.auxiliary.AddContent("page2");
        context.LoadPopWindow<GridTestWindow>();
        d.auxiliary.Refresh();

        one.model.Context.GetComponent<Image>().color = Color.green;
        var top = area.AddArea(LayoutArea.Dock.Top, 0.2f);
        top.auxiliary.AddContent("page3");
        top.model.Context.GetComponent<Image>().color = Color.yellow;

        var l = top.AddArea(LayoutArea.Dock.Left, 0.4f);
        l.model.Context.GetComponent<Image>().color = Color.blue;
        l.auxiliary.headDock = LayoutAuxiliary.HeadDock.Down;

        context = l.auxiliary.AddContent("page5");
        context.LoadPopWindow<GridTestWindow2>();
        l.auxiliary.Refresh();
    } 
}
public class GridTestWindow : PopWindow
{
    class View
    {
        public ScrollY Scroll;
    }
    class Item
    {
        public Text Text;
    }
    View view;
    public override void Initial(ModelElement parent, UIPage ui, object obj = null)
    {
        view = LoadUI<View>("baseUI", "gridscroll");
        base.Initial(parent, ui, obj);

        List<int> testData = new List<int>();
        for (int i = 0; i < 33; i++)
            testData.Add(i);
        view.Scroll.BindingData = testData;
        view.Scroll.SetItemUpdate<Item, int>((o, e, i) => {
            o.Text.text = i.ToString();
        });
        view.Scroll.Refresh();
        view.Scroll.eventCall.Click = (o, e) =>
        {
            //if (e.IsRightButtonUp)
            //    UIMenu.Instance.ShowMenu<TestMenu>(this, e.CanPosition);
            Debug.Log("click");
        };
    }
    public override void Cmd(string cmd, object dat)
    {
        switch(cmd)
        {
            case "menu":
                Debug.Log(dat);
                break;
        }
    }
}
public class GridTestWindow2 : PopWindow
{
    class View
    {
        public ScrollX Scroll;
    }
    class Item
    {
        public Text Text;
    }
    View view;
    public override void Initial(ModelElement parent, UIPage ui, object obj = null)
    {
        view = LoadUI<View>("baseUI", "gridscroll");
        base.Initial(parent, ui, obj);

        List<int> testData = new List<int>();
        for (int i = 0; i < 44; i++)
            testData.Add(i);
        view.Scroll.BindingData = testData;
        view.Scroll.SetItemUpdate<Item, int>((o, e, i) => {
            o.Text.text = i.ToString();
            Debug.Log(i.ToString());
        });
        view.Scroll.Refresh();
    }
}
//public class TestMenu : MenuWindow
//{
//    class View
//    {
//        public EventCallBack Menu;
//    }
//    View view;
//    public override void Initial(ModelElement parent, UIBase ui, object obj = null)
//    {
//        model = ModelManagerUI.CloneModel("baseUI", "Menu");
//        base.Initial(parent, ui, obj);
//        view = model.ComponentReflection<View>();
//        view.Menu.Click = (o, e) => {
//            UIMenu.Instance.HideMenu();
//            Context.Cmd("menu",e);
//        };
//    }
//}