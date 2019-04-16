using huqiang.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct RawImageAttribute
    {
        public Rect uvRect;
        public Color color;
        public Int32 material;
        public Int32 shader;
        public Int32 assetName;
        public Int32 textureName;
        public static int Size = sizeof(RawImageAttribute);
        public static int ElementSize = Size / 4;
        public static void LoadFromBuff(ref RawImageAttribute raw, void* p)
        {
            fixed (Rect* trans = &raw.uvRect)
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
    public class RawImageElement:ModelElement
    {
        RawImageAttribute raw;
        public Texture texture;
        public Material material;
        string assetName;
        public string textureName;
        public string shader;
        string smat;
        public unsafe override byte* LoadFromBuff(byte* point)
        {
            point = base.LoadFromBuff(point);
            RawImageAttribute.LoadFromBuff(ref raw, point);
            smat = buffer[raw.material];
            shader = buffer[raw.shader];
            assetName = buffer[raw.assetName];
            textureName = buffer[raw.textureName];
            return point + RawImageAttribute.Size;
        }
        public unsafe override byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            int tsize = RawImageAttribute.Size;
            byte[] buff = new byte[size + tsize];
            fixed (byte* bp = &buff[0])
            {
                *(ElementAttribute*)bp = transAttribute;
                byte* a = bp + size;
                *(RawImageAttribute*)a = raw;
            }
            return buff;
        }
        static void Load(GameObject tar, RawImageElement raw)
        {
            var a = tar.GetComponent<RawImage>();
            a.uvRect = raw.raw.uvRect;
            a.color = raw.raw.color;
            a.raycastTarget = false;
            if (raw.smat != null)
                if (raw.smat != "Default UI Material")
                    a.material = new Material(Shader.Find(raw.shader));
            if (raw.textureName != null)
                a.texture = ElementAsset.FindTexture(raw.assetName,raw. textureName);
        }
        static void Save(GameObject tar, RawImageElement raw)
        {
            var b = tar.GetComponent<RawImage>();
            if (b != null)
            {
                if (b.texture != null)
                {
                    string tn = b.texture.name;
                    raw.raw.textureName = raw.buffer.AddString(tn);
                    var an = ElementAsset.TxtureFormAsset(tn);
                    raw.raw.assetName = raw.buffer.AddString(an);
                }
                var mat = b.material;
                raw.raw.material = raw.buffer.AddString(mat.name);
                raw.raw.shader = raw.buffer.AddString(mat.shader.name);
                raw.raw.color = b.color;
                raw.raw.uvRect = b.uvRect;
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
        public void SetTexture(Texture t2d)
        {
            Main.GetComponent<RawImage>().texture=t2d;
        }
    }
}
