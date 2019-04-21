using huqiang.Data;
using huqiang.UI;
using huqiang.UIEvent;
using huqiang.UIModel;
using UGUI;
using UnityEngine;

namespace huqiang
{
    public class App
    {
        static void InitialA()
        {
            ThreadPool.Initial();
            EmojiText.Emoji = UnityEngine.Resources.Load<Texture2D>("emoji");
            ModelManager.Initial();
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
        static RectTransform UIRoot;
        public static void Initial(Transform uiRoot)
        {
            InitialA();
            if (uiRoot == null)
            {
                uiRoot = new GameObject("UI", typeof(Canvas)).transform;
                uiRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                UIRoot = new GameObject("uiRoot", typeof(RectTransform)).transform as RectTransform;
            }
            Page.Root = new UI.UIElement();
            Page.Root.data.active = true;
            Page.Root.Context = new GameObject("uiRoot",typeof(RectTransform)).transform as RectTransform;
            Page.Root.Context.SetParent(uiRoot);
            Page.Root.Context.localPosition = Vector3.zero;
            Page.Root.Context.localScale = Vector3.one;
            ModelManagerUI.Initial();
            BaseEvent.InsertRoot(Page.Root);
        }
        public static float AllTime;
        public static void Update()
        {
            //AnimationManage.Manage.Update();
            //UserAction.DispatchEvent();
            UserInput.DispatchEvent();
            ThreadPool.AddMission(SubThread,null);
            ThreadPool.ExtcuteMain();
            Page.Root.Apply();
            //Resize();
            //Page.Refresh(UserAction.TimeSlice);
            AllTime += Time.deltaTime;
            //DownloadManager.UpdateMission();
        }
        static void SubThread(object obj)
        {
            UserInput.SubDispatch();
            Page.Refresh(UserInput.TimeSlice);
        }
        static void Resize()
        {
            float w = Screen.width;
            float h = Screen.height;
            float s = Scale.ScreenScale;
            UIRoot.localScale = new Vector3(s, s, s);
            w /= s;
            h /= s;
            if (Scale.ScreenWidth != w | Scale.ScreenHeight != h)
            {
                Scale.ScreenWidth = w;
                Scale.ScreenHeight = h;
                UIRoot.sizeDelta = new Vector2(w, h);
                if (Page.CurrentPage != null)
                    Page.CurrentPage.ReSize();
            }
        }
        public static void Dispose()
        {
            EventCallBack.ClearEvent();
            ThreadPool.Dispose();
            RecordManager.ReleaseAll();
        }
    }
}