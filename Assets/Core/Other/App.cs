using huqiang.Data;
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
        public static void Initial()
        {
            InitialA();
            //if(uiRoot==null)
            //{
            //    var ui = new GameObject("UI", typeof(Canvas));
            //    ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            //    UIRoot = new GameObject("uiRoot",typeof(RectTransform)).transform as RectTransform;
            //    UIRoot.SetParent(ui.transform);
            //    UIRoot.localPosition = Vector3.zero;
            //}else  UIRoot = uiRoot;
            Page.Root = new UI.UIElement();
            var buff = new GameObject("buffer",typeof(Canvas));
            buff.SetActive(false);
            ModelManager.CycleBuffer = buff.transform;
            EventCallBack.InsertRoot(UIRoot.root as RectTransform);
        }
        public static float AllTime;
        public static void Update()
        {
            AnimationManage.Manage.Update();
            UserAction.DispatchEvent();
            ThreadPool.ExtcuteMain();
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