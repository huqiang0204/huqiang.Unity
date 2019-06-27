using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PopWindow : UIBase
{
    public Func<bool> Back { get; set; }
    protected Page MainPage;
    public virtual void Initial(Transform parent, Page page, object obj = null)
    {
        base.Initial(parent, page, obj);
        MainPage = page;
        if (model != null)
            if (page != null)
                model.SetParent(page.model);
    }
    //用来初始化弹窗的虚方法
    public virtual void InitialPop(object obj = null)
    {

    }
    public virtual void Show(object obj = null) { if (main != null) main.SetActive(true); }
    public virtual void Hide()
    {
        if (main != null)
            main.SetActive(false);
    }
    public virtual bool Handling(string cmd, object dat)
    {
        return false;
    }
}