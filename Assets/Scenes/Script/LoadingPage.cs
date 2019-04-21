using huqiang.UI;
using huqiang.UIComposite;
using huqiang.UIEvent;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPage : Page
{
    class View
    {
        public UIElement LeftUp;
        public UIElement Center;
        public UIElement List;
        public UIElement friend;
        public UIElement Right;
        public ScrollY FriendsRanking;
    }
    UIElement txt;
    System.Random ran;
    public override void Initial(UIElement parent, object dat = null)
    {
        model = ModelManagerUI.LoadToElement("baseUI", "RankingList");
        base.Initial(parent, dat);
        model.SetParent(parent);
        var view = model.ComponentReflection<View>();
        view.LeftUp.data.active = false;
        view.Right.data.active = false;
        view.friend.data.active = false;
        List<string> data = new List<string>();
        for (int i = 0; i < 100; i++)
            data.Add("sdfsdfsdf"+i);
        view.FriendsRanking.BindingData = data;
        view.FriendsRanking.Refresh();
        //ran = new System.Random();
        //txt.baseEvent.Click = (o, e) => {
        //    Debug.Log("click");
        //    txt.data.localPosition = new Vector3(ran.Next(-400,400),0,0);
        //    txt.IsChanged = true;
        //};
    }
}
