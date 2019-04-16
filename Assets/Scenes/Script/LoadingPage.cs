using huqiang.UI;
using huqiang.UIEvent;
using UnityEngine;

public class LoadingPage : Page
{
    ModelElement txt;
    System.Random ran;
    public override void Initial(ModelElement parent, object dat = null)
    {
        model = ModelManagerUI.LoadToElement("baseUI", "loading");
        base.Initial(parent, dat);
        parent.AddChild(model);
        txt = model.FindChild("Text");
        txt.RegEvent<BaseEvent>();
        ran = new System.Random();
        txt.baseEvent.Click = (o, e) => {
            Debug.Log("click");
            txt.data.localPosition = new Vector3(ran.Next(-400,400),0,0);
            txt.IsChanged = true;
        };
    }
}
