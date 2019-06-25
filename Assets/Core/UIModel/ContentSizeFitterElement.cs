using huqiang.Data;
using huqiang.Manager2D;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace huqiang.UIModel
{

   public  class ContentSizeFitterElement: DataConversion
    {
       public ContentSizeFitter.FitMode m_HorizontalFit;
        protected ContentSizeFitter.FitMode m_VerticalFit;
        Outline Context;
        public OutLineData data;
        public unsafe override void Load(FakeStruct fake)
        {
            m_HorizontalFit =(ContentSizeFitter.FitMode) fake[0];
            m_VerticalFit = (ContentSizeFitter.FitMode)fake[1];
        }
        public override void LoadToObject(Component game)
        {
            LoadToObject(game,this);
        }
        public static void LoadToObject(Component game, ContentSizeFitterElement dat)
        {
            var a = game.GetComponent<ContentSizeFitter>();
            if (a == null)
                return;
            a.horizontalFit = dat.m_HorizontalFit;
            a.verticalFit = dat.m_VerticalFit;

        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var dat = com as ContentSizeFitter;
            if (dat == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, 2);
            fake[0] = (int)dat.horizontalFit;
            fake[1] = (int)dat.verticalFit;
            return fake;
        }
    }
}
