﻿using huqiang.Data;
using huqiang.Manager2D;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public class MaskElement : DataConversion
    {
        Mask Context;
        bool data;
        public unsafe override void Load(FakeStruct fake)
        {
            data = *(bool*)fake.ip;
        }
        public override void LoadToObject(Component game)
        {
            var a = game.GetComponent<Mask>();
            if (a == null)
                return;
            a.enabled = true;
            a.showMaskGraphic = data;
            Context = a;
        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var img = com as Mask;
            if (img == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, 1);
            *(bool*)fake.ip = img.showMaskGraphic;
            return fake;
        }
    }
    public class RectMaskElement : DataConversion
    {
        public RectMask2D Context;
        public override void LoadToObject(Component game)
        {
            var a = game.GetComponent<RectMask2D>();
            if (a == null)
                return;
            Context = a;
        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            return null;
        }
    }
}
