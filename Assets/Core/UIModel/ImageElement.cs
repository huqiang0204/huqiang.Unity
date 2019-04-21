using huqiang.Data;
using huqiang.ModelManager2D;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct ImageData
    {
        public float alphaHit;
        public float fillAmount;
        public bool fillCenter;
        public bool fillClockwise;
        public Image.FillMethod fillMethod;
        public Int32 fillOrigin;
        public bool preserveAspect;
        public Image.Type type;
        public Int32 shader;
        public Color color;
        public Int32 assetName;
        public Int32 textureName;
        public Int32 spriteName;
        public static int Size = sizeof(ImageData);
        public static int ElementSize = Size / 4;
    }
    public class ImageElement : DataConversion
    {
        Image Context;
        public ImageData data;
        string shader;
        public string assetName;
        public string textureName;
        public string spriteName;
        public unsafe override void Load(FakeStruct fake)
        {
            data = *(ImageData*)fake.ip;
            shader = fake.buffer.GetData(data.shader) as string;
            assetName = fake.buffer.GetData(data.assetName) as string;
            textureName = fake.buffer.GetData(data.textureName) as string;
            spriteName = fake.buffer.GetData(data.spriteName) as string;
        }
        public override void LoadToObject(Component game)
        {
            LoadToObject(game, ref data, this);
        }
        public static void LoadToObject(Component game, ref ImageData dat, ImageElement image)
        {
            var a = game.GetComponent<Image>();
            if (a == null)
                return;
            a.alphaHitTestMinimumThreshold = dat.alphaHit;
            a.fillAmount = dat.fillAmount;
            a.fillCenter = dat.fillCenter;
            a.fillClockwise = dat.fillClockwise;
            a.fillMethod = dat.fillMethod;
            a.fillOrigin = dat.fillOrigin;
            a.preserveAspect = dat.preserveAspect;
            a.type = dat.type;
            a.raycastTarget = false;
            a.color = dat.color;
            if (image.shader != "Default UI Material")
                a.material = new Material(Shader.Find(image.shader));
            if (image.spriteName != null)
                a.sprite = ElementAsset.FindSprite(image.assetName, image.textureName, image.spriteName);
            else a.sprite = null;
            image.Context = a;
        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var img = com as Image;
            if (img == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, ImageData.ElementSize);
            ImageData* data = (ImageData*)fake.ip;
            data->alphaHit = img.alphaHitTestMinimumThreshold;
            data->fillAmount = img.fillAmount;
            data->fillCenter = img.fillCenter;
            data->fillClockwise = img.fillClockwise;
            data->fillMethod = img.fillMethod;
            data->fillOrigin = img.fillOrigin;
            data->preserveAspect = img.preserveAspect;
            data->type = img.type;
            data->color = img.color;
            if (img.sprite != null)
            {
                var tn = img.sprite.texture.name;
                var sn = img.sprite.name;
                var an = ElementAsset.TxtureFormAsset(img.sprite.texture.name);
                data->assetName = buffer.AddData(an);
                data->textureName = buffer.AddData(tn);
                data->spriteName = buffer.AddData(sn);
            }
            if (img.material != null)
                data->shader = buffer.AddData(img.material.shader.name);
            return fake;
        }
    }
}
