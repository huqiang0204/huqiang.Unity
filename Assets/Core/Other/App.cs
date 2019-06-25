using huqiang.Data;
using huqiang.UIModel;
using UGUI;
using UnityEngine;
using huqiang.Manager2D;
using UnityEngine.UI;

namespace huqiang
{
    public class App
    {
        static void Initial()
        {
            if(Application.platform == RuntimePlatform.Android |Application.platform==RuntimePlatform.IPhonePlayer)
            {
                UserAction.inputType = UserAction.InputType.OnlyTouch;
            }
            else
            {
                UserAction.inputType = UserAction.InputType.Blend;
            }
            if (Application.platform == RuntimePlatform.WindowsEditor | Application.platform == RuntimePlatform.WindowsPlayer)
            {
                IME.Initial();
            }
        }
        static void InitialUI()
        {
            ModelManagerUI.RegComponent(new ComponentType<RectTransform, ModelElement>(ModelElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<Image, ImageElement>(ImageElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<EmojiText, TextElement>(TextElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<Text, TextElement>(TextElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<CustomRawImage, RawImageElement>(RawImageElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<RawImage, RawImageElement>(RawImageElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<RectMask2D, RectMaskElement>(RectMaskElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<Mask, MaskElement>(MaskElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<Outline, OutLineElement>(OutLineElement.LoadFromObject));
            ModelManagerUI.RegComponent(new ComponentType<ContentSizeFitter, ContentSizeFitterElement>(ContentSizeFitterElement.LoadFromObject));
        }

        static RectTransform UIRoot;
        static RectTransform Hint;
        public static void Initial(RectTransform uiRoot)
        {
            InitialUI();
            Initial();
            if (uiRoot==null)
            {
                var ui = new GameObject("UI", typeof(Canvas));
                ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                UIRoot = new GameObject("uiRoot",typeof(RectTransform)).transform as RectTransform;
                UIRoot.SetParent(ui.transform);
                UIRoot.localPosition = Vector3.zero;

                Hint = new GameObject("Hint", typeof(RectTransform)).transform as RectTransform;
                Hint.SetParent(ui.transform);
                Hint.localPosition = Vector3.zero;
                Hint.SetAsLastSibling();
                //HintPage.root = Hint;
                //NotifyControll.root = Hint;
            }
            else  UIRoot = uiRoot;
            UIRoot.sizeDelta = new Vector2(Screen.width, Screen.height);
            Page.Root = UIRoot;
            var buff = new GameObject("buffer",typeof(Canvas));
            buff.SetActive(false);
            ModelManagerUI.CycleBuffer = buff.transform;
            EventCallBack.InsertRoot(UIRoot.root as RectTransform);
            ModelManager2D.Initial();
            Scale.NormalDpi = Screen.dpi;
        }
        public static float AllTime;
        public static void Update()
        {
            AnimationManage.Manage.Update();
            UserAction.DispatchEvent();
       
            ThreadMission.ExtcuteMain();
            Resize();
            Page.Refresh(UserAction.TimeSlice);
            AllTime += Time.deltaTime;
            DownloadManager.UpdateMission();
        }
        static void Resize()
        {
            float w = Screen.width;
            float h = Screen.height;
            float s = Scale.ScreenScale;
            UIRoot.localScale = new Vector3(s, s, s);
            if (Hint != null)
                Hint.localScale = new Vector3(s, s, s);
            w /= s;
            h /= s;
            UIRoot.sizeDelta = new Vector2(w, h);
            if (Scale.ScreenWidth != w | Scale.ScreenHeight != h)
            {
                Scale.ScreenWidth = w;
                Scale.ScreenHeight = h;
                if (Page.CurrentPage != null)
                    Page.CurrentPage.ReSize();
            }
        }
        public static void Dispose()
        {
            EventCallBack.ClearEvent();
            ThreadMission.DisposeAll();
            RecordManager.ReleaseAll();
        }
    }
}