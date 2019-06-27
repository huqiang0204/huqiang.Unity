using huqiang;
using System;
using System.Collections.Generic;
using huqiang.UIModel;
using UnityEngine;
using huqiang.Data;

public class UIBase
{
    static int point;
    static UIBase[] buff=new UIBase[1024];
    public static T GetUI<T>()where T:UIBase
    {
        for (int i = 0; i < point; i++)
            if (buff[i] is T)
                return buff[i] as T;
        return null;
    }
    public static List<T> GetUIs<T>() where T : UIBase
    {
        List<T> tmp = new List<T>();
        for(int i = 0; i < point; i++)
            if (buff[i] is T)
               tmp.Add(buff[i] as T);
        return tmp;
    }
    public static void ClearUI()
    {
        for (int i = 0; i < point; i++)
            buff[i] = null;
        point = 0;
    }
    int Index;
    public UIBase()
    {
        Index = point;
        buff[point] = this;
        point++;
    }
    public object DataContext;
    public ModelElement Parent { get; protected set; }
    public GameObject main { get; protected set; }
    public ModelElement model { get; protected set; }
    protected UIBase UIParent;
    public T LoadUI<T>(string asset,string name)where T:class,new()
    {
        model = ModelManagerUI.CloneModel(asset, name);
        T t= new T();
        ModelManagerUI.LoadToGame(model, t);
        return t;
    }
    public virtual void Initial(ModelElement parent, UIBase ui, object obj = null)
    {
        DataContext = obj;
        UIParent = ui;
        Parent = parent;
        if (model != null)
            main = model.Main;
        if (model != null)
        {
            model.SetParent(parent);
            var t = model.Context;
            t.SetParent(Parent.Context);
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
        }

    }
    public virtual void Dispose() {
        if (main != null)
            ModelManagerUI.RecycleGameObject(main);
        point--;
        if (buff[point] != null)
            buff[point].Index = Index;
        buff[Index] = buff[point];
        buff[point] = null;
        Resources.UnloadUnusedAssets();
    }
    public virtual void Save()
    {
    }
    public virtual void Recovery()
    {
    }
    public virtual object CollectionData()
    {
        return null;
    }
    public virtual void Cmd(string cmd, object dat)
    {
    }
    public virtual void Cmd(Int32 cmd,Int32 type, object dat)
    {
    }
    public virtual void ReSize()
    {
        if (model != null)
            ModelElement.ScaleSize(model);
    }
    public virtual void Update(float time)
    {
    }
    public virtual void ChangeLanguage()
    {

    }
}
public class UIPage:UIBase
{
    //public  PopType _popType = PopType.None;//默认
    class PageInfo
    {
        public Type Pagetype;
        public object DataContext;
        public Type PopType;
        public object PopData;
    }
    static Stack<PageInfo> pages=new Stack<PageInfo>();
    static Type typePop = typeof(PopWindow);
    public static void ClearStack()
    {
        pages.Clear();
    }
    public static ModelElement Root { get;  set; }
    public static UIPage CurrentPage { get; private set; }
    public static void LoadPage<T>(object dat = null) where T : UIPage, new()
    {
        if (CurrentPage is T)
        {
            CurrentPage.Show(dat);
            return;
        }
        EventCallBack.ClearEvent();
        AnimationManage.Manage.ReleaseAll();
        if (CurrentPage != null)
        {
            CurrentPage.Save();
            CurrentPage.Dispose();
        }
        var t = new T();
        t.Initial(Root, dat);
        t.ReSize();
        CurrentPage = t;
        t.ChangeLanguage();
    }
    public static void LoadPage(Type type, object dat = null)
    {
        if (typeof(UIPage).IsAssignableFrom(type))
        {
            if (CurrentPage != null)
                if (CurrentPage.GetType() == type)
                {
                    CurrentPage.Show(dat);
                    return;
                }
            EventCallBack.ClearEvent();
            AnimationManage.Manage.ReleaseAll();
            if (CurrentPage != null)
                CurrentPage.Dispose();
            var t = Activator.CreateInstance(type) as UIPage;
            CurrentPage = t;
            t.Initial(Root, dat);
            t.ReSize();
            t.Recovery();
            t.ChangeLanguage();
        }
    }
    public static void Back()
    {
        if(pages.Count>0)
        {
            var page = pages.Pop();
            if (page != null)
            {
                LoadPage(page.Pagetype, page.DataContext);
                if (page.PopType != null)
                {
                    CurrentPage.PopUpWindow(page.PopType, page.PopData);
                }
            }
        }
    }
    public static void UpdateData(string cmd, object obj)
    {
        if (CurrentPage != null)
            CurrentPage.Cmd(cmd, obj);
    }
    public static void UpdateData(Int32 cmd,Int32 type,object obj)
    {
        if (CurrentPage != null)
            CurrentPage.Cmd(cmd,type,obj);
    }
    public static void Refresh(float time)
    {
        if (CurrentPage != null)
            CurrentPage.Update(time);
    }

    public UIPage()
    {
        pops = new List<PopWindow>();
    }
    protected Type BackPage;
    protected Type BackPop;
    protected object BackData;
    protected GameObject mask;
    // public PopWindow currentPop { get; private set; }
    public PopWindow currentPop { get;  set; }
    public virtual void Initial(ModelElement parent, object dat = null) {
        DataContext = dat;
        Parent = parent;
        if (model!=null)
        {
            model.SetParent(parent);
            var t =model.Context;
            t.SetParent(Parent.Context);
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
        }
    }
    public virtual void Show(object dat=null)
    {
    }
    public override void ReSize() { base.ReSize();if (currentPop != null) currentPop.ReSize(); }
    public override void Dispose()
    {
        if (pops != null)
            for (int i = 0; i < pops.Count; i++)
        pops[i].Dispose(); 
        pops.Clear();
        currentPop = null;
        ModelManagerUI.RecycleGameObject(main);
        main = null;
        ClearUI();
    }
    public void HidePopWindow()
    {
        if (mask != null)
            mask.SetActive(false);
        if (currentPop != null)
        {
            currentPop.Hide();
        }
        currentPop = null;
    }
    List<PopWindow> pops;
    protected T ShowPopWindow<T>(object obj = null, ModelElement parent = null) where T : PopWindow, new()
    {
        if (mask != null)
            mask.gameObject.SetActive(true);
        if (currentPop != null)
        { currentPop.Hide(); currentPop = null; }
        for (int i = 0; i < pops.Count; i++)
            if (pops[i] is T)
            {
                currentPop = pops[i];
                pops[i].Show(obj);
                return pops[i] as T;
            }
        var t = new T();
        pops.Add(t);
        currentPop = t;
        if (parent == null)
            t.Initial(Parent, this, obj);
        else t.Initial(parent, this, obj);
        t.ReSize();
        return t;
    }
    protected object ShowPopWindow(Type type, object obj = null, ModelElement parent = null)
    {
        if (mask != null)
            mask.gameObject.SetActive(true);
        if (currentPop != null)
        { currentPop.Hide(); currentPop = null; }
        for (int i = 0; i < pops.Count; i++)
            if (pops[i].GetType() ==type)
            {
                currentPop = pops[i];
                pops[i].Show(obj);
                return pops[i];
            }
        var t = Activator.CreateInstance(type) as PopWindow;
        pops.Add(t);
        currentPop = t;
        if (parent == null)
            t.Initial(Parent, this, obj);
        else t.Initial(parent, this, obj);
        t.ReSize();
        return t;
    }
    public virtual T PopUpWindow<T>(object obj = null) where T : PopWindow, new()
    {
        return ShowPopWindow<T>(obj,null);
    }
    object PopUpWindow(Type type,object obj=null)
    {
        var pop= ShowPopWindow(type,obj,null) as PopWindow;
        pop.Recovery();
        return pop;
    }
    /// <summary>
    /// 释放掉当前未激活的弹窗
    /// </summary>
    public void ReleasePopWindow()
    {
        if (pops != null)
            for (int i = 0; i < pops.Count; i++)
                if (pops[i] != currentPop)
                    pops[i].Dispose();
        pops.Clear();
        if (currentPop != null)
            pops.Add(currentPop);
    }
    public void ReleasePopWindow(PopWindow window)
    {
        pops.Remove(window);
        if (currentPop == window)
        {
            currentPop = null;
            if (mask != null)
                mask.SetActive(false);
        }
        window.Dispose();
    }
    public void ReleasePopWindow<T>()
    {
        for (int i = 0; i < pops.Count; i++)
            if (pops[i] is T)
            {
                pops[i].Dispose();
                pops.RemoveAt(i);
                break;
            }
        if(currentPop is T)
        {
            currentPop = null;
            if (mask != null)
                mask.SetActive(false);
        }
    }
    public T GetPopWindow<T>() where T : PopWindow
    {
        for (int i = 0; i < pops.Count; i++)
            if (pops[i] is T)
            {
                return pops[i] as T;
            }
        return null;
    }
    public override void Save()
    {
        if (pops != null)
            for (int i = 0; i < pops.Count; i++)
                if (pops[i] != currentPop)
                    pops[i].Save();
        PageInfo page = new PageInfo();
        page.Pagetype = GetType();
        if (currentPop != null)
            if (currentPop.main != null)
                if (currentPop.main.activeSelf)
                {
                    page.PopType = currentPop.GetType();
                    page.PopData = currentPop.DataContext;
                }
        page.DataContext = DataContext;
        pages.Push(page);
    }
    public override void Update(float time)
    {
        if (currentPop != null)
            currentPop.Update(time);
    }
    public override void ChangeLanguage()
    {
        if (pops != null)
        {
            for (int i = 0; i < pops.Count; i++)
            {
                pops[i].ChangeLanguage();
            }
        }
    }
}
