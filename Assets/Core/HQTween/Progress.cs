﻿using huqiang.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang
{
    public class Progress : AnimatInterface
    {
        public Progress()
        {
            AnimationManage.Manage.AddAnimat(this);
            AnimationManage.Manage.DontReleaseOnClear(this);
        }
        public void Dispose()
        {
            AnimationManage.Manage.ReleaseAnimat(this);
        }
        bool playing = false;
        public string name;
        AssetBundleCreateRequest abcr;
        public Action<Progress, AssetBundleCreateRequest> PlayOver;
        public Action<Progress, AssetBundleCreateRequest> Schedule;
        public void Play(AssetBundleCreateRequest ab)
        {
            playing = true;
            abcr = ab;
        }
        public void Update(float time)
        {
            if (playing)
            {
                if (abcr == null)
                    playing = false;
                else
                {
                    if (abcr.isDone)
                    {
                        playing = false;
                        var bs = abcr.assetBundle;
                        ElementAsset.AddBundle(name,bs);
                        AnimationManage.Manage.ReleaseAnimat(this);
                        if (PlayOver != null)
                            PlayOver(this, abcr);
                    }
                    else
                    {
                        if (Schedule != null)
                            Schedule(this, abcr);
                    }
                }
            }
        }
    }
}
