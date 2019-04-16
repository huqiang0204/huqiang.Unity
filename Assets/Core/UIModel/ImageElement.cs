using huqiang.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct ImageAttribute
    {
        public float alphaHit;
        public float fillAmount;
        public bool fillCenter;
        public bool fillClockwise;
        public Image.FillMethod fillMethod;
        public Int32 fillOrigin;
        public bool preserveAspect;
        public Image.Type type;
        public Int32 material;
        public Int32 shader;
        public Color color;
        public Int32 assetName;
        public Int32 textureName;
        public Int32 spriteName;
        public static int Size = sizeof(ImageAttribute);
        public static int ElementSize = Size / 4;
        public static void LoadFromBuff(ref ImageAttribute img, void* p)
        {
            fixed (float* trans = &img.alphaHit)
            {
                Int32* a =(Int32*) trans;
                Int32* b = (Int32*)p;
                for (int i = 0; i < ElementSize; i++)
                {
                    *a = *b;
                    a++;
                    b++;
                }
            }
        }
    }
    public class ImageElement : ModelElement
    {
        public ImageAttribute imageAttribute;

        public Sprite sprite;
        public Material material;
        string assetName;
        public string textureName;
        public string spriteName;
        public string shader;
        string smat;
        public unsafe override byte* LoadFromBuff(byte* point)
        {
            point = base.LoadFromBuff(point);
            ImageAttribute.LoadFromBuff(ref imageAttribute,point);
            smat = buffer[imageAttribute.material] ;
            shader = buffer[imageAttribute.shader] ;
            assetName = buffer[imageAttribute.assetName] ;
            textureName = buffer[imageAttribute.textureName] ;
            spriteName = buffer[imageAttribute.spriteName] ;
            return point + ImageAttribute.Size;
        }
        public unsafe override byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            int tsize = ImageAttribute.Size;
            byte[] buff = new byte[size + tsize];
            fixed (byte* bp = &buff[0])
            {
                *(ElementAttribute*)bp = transAttribute;
                byte* a = bp + size;
                *(ImageAttribute*)a = imageAttribute;
            }
            return buff;
        }
        static void Load(GameObject tar, ImageElement image)
        {
            var att = image.imageAttribute;
            var a = tar.GetComponent<Image>();
            a.alphaHitTestMinimumThreshold = att.alphaHit;
            a.fillAmount = att.fillAmount;
            a.fillCenter = att.fillCenter;
            a.fillClockwise = att.fillClockwise;
            a.fillMethod = att.fillMethod;
            a.fillOrigin = att.fillOrigin;
            a.preserveAspect = att.preserveAspect;
            a.type = att.type;
            a.raycastTarget = false;
            a.color = att.color;
            if (image.smat != null)
                if (image.smat != "Default UI Material")
                     a.material = new Material(Shader.Find(image.shader));
            if (image.spriteName != null)
                a.sprite = ElementAsset.FindSprite(image.assetName, image.textureName, image.spriteName);
        }
        static void Save(GameObject tar,ImageElement image)
        {
            var b = tar.GetComponent<Image>();
            if (b != null)
            {
                image.imageAttribute.alphaHit = b.alphaHitTestMinimumThreshold;
                image.imageAttribute.fillAmount = b.fillAmount;
                image.imageAttribute.fillCenter = b.fillCenter;
                image.imageAttribute.fillClockwise = b.fillClockwise;
                image.imageAttribute.fillMethod = b.fillMethod;
                image.imageAttribute.fillOrigin = b.fillOrigin;
                image.imageAttribute.preserveAspect = b.preserveAspect;
                if (b.sprite != null)
                {
                    image.imageAttribute.spriteName = image.buffer.AddString(b.sprite.name);
                    string tn = b.sprite.texture.name;
                    image.imageAttribute.textureName = image.buffer.AddString(tn);
                    var an = ElementAsset.TxtureFormAsset(tn);
                    if (an != null)
                        image.imageAttribute.assetName = image.buffer.AddString(an);
                    else image.imageAttribute.assetName = -1;
                }
                else image.imageAttribute.spriteName = -1;
                image.imageAttribute.type = b.type;
                var mat = b.material;
                image.imageAttribute.material = image.buffer.AddString(mat.name);
                image.imageAttribute.shader = image.buffer.AddString(mat.shader.name);
                image.imageAttribute.color = b.color;
            }
        }
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            Load(tar, this);
        }
        public override void Save(GameObject tar)
        {
            base.Save(tar);
            Save(tar, this);
        }
        public void SetSprite(Sprite sprite)
        {
            Main.GetComponent<Image>().sprite = sprite;
        }
    }
    public class ViewportElement : ImageElement
    {
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            var mask = tar.GetComponent<Mask>();
            if(mask!=null)
            mask.showMaskGraphic = false;
        }
    }
}
