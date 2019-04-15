using huqiang.UI;
using huqiang.UIEvent;
using UnityEngine;

public class LoadingPage : Page
{
    UIElement txt;
    public override void Initial(UIElement parent, object dat = null)
    {
        model = ModelManagerUI.LoadToElement("baseUI", "loading");
        base.Initial(parent, dat);
        parent.AddChild(model);
        txt = model.FindChild("Text");
        txt.RegEvent<BaseEvent>();
        txt.baseEvent.Click = (o, e) => {
            Debug.Log("click");
            txt.data.localPosition = new Vector3(Random.Range(-400,400),0,0);
        };
    }
}
