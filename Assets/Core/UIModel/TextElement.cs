using System;
using System.Collections.Generic;
using UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct TextAttribute
    {
        public bool alignByGeometry;
        public TextAnchor alignment;
        public Int32 fontSize;
        public FontStyle fontStyle;
        public HorizontalWrapMode horizontalOverflow;
        public float lineSpacing;
        public bool resizeTextForBestFit;
        public Int32 resizeTextMaxSize;
        public Int32 resizeTextMinSize;
        public bool supportRichText;
        public Color color;
        public VerticalWrapMode verticalOverflow;
        public Int32 font;
        public Int32 text;
        public Int32 material;
        public Int32 shader;
        public static int Size = sizeof(TextAttribute);
        public static int ElementSize = Size / 4;
        public static void LoadFromBuff(ref TextAttribute txt, void* p)
        {
            fixed (Boolean* trans = &txt.alignByGeometry)
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
    public class TextElement : ModelElement
    {
        public static List<Font> fonts=new List<Font>();
        public static Font FindFont(string str)
        {
            if (fonts == null)
                return null;
            for (int i = 0; i < fonts.Count; i++)
            {
                if (str == fonts[i].name)
                    return fonts[i];
            }
            if (fonts.Count == 0)
                TextElement.fonts.Add(Font.CreateDynamicFontFromOSFont("Arial", 16));
            return fonts[0];
        }
        public TextAttribute textAttribute;
        public Font font;
        public Material material;
        string text;
        string fontName;
        string smat;
        string shader;
        public unsafe override byte* LoadFromBuff(byte* point)
        {
            point = base.LoadFromBuff(point);
            name = buffer[transAttribute.name] ;
            tag = buffer[transAttribute.tag];
            TextAttribute.LoadFromBuff(ref textAttribute,point);
            smat = buffer[textAttribute.material];
            shader = buffer[textAttribute.shader];
            fontName = buffer[textAttribute.font];
            text = buffer[textAttribute.text];
            return point + TextAttribute.Size;
        }
        public unsafe override byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            int tsize = TextAttribute.Size;
            byte[] buff = new byte[size + tsize];
            fixed (byte* bp = &buff[0])
            {
                *(ElementAttribute*)bp = transAttribute;
                byte* a = bp+ size;
                *(TextAttribute*)a = textAttribute;
            }
            return buff;
        }
        static void Load(GameObject tar, ref TextAttribute att)
        {
            var a = tar.GetComponent<Text>();
            a.alignByGeometry = att.alignByGeometry;
            a.alignment = att.alignment;
            a.fontSize = att.fontSize;
            a.fontStyle = att.fontStyle;
            a.horizontalOverflow = att.horizontalOverflow;
            a.lineSpacing = att.lineSpacing;
            a.resizeTextForBestFit = att.resizeTextForBestFit;
            a.resizeTextMaxSize = att.resizeTextMaxSize;
            a.resizeTextMinSize = att.resizeTextMinSize;
            a.supportRichText = att.supportRichText;
            a.verticalOverflow = att.verticalOverflow;
            a.color = att.color;
            a.raycastTarget = false;
            a.enabled = true;
        }
        static void Save(GameObject tar, TextElement text)
        {
            var txt = tar.GetComponent<Text>();
            if (txt != null)
            {
                text.textAttribute.alignByGeometry = txt.alignByGeometry;
                text.textAttribute.alignment = txt.alignment;
                text.textAttribute.fontSize = txt.fontSize;
                text.textAttribute.fontStyle = txt.fontStyle;
                text.textAttribute.horizontalOverflow = txt.horizontalOverflow;
                text.textAttribute.lineSpacing = txt.lineSpacing;
                text.textAttribute.resizeTextForBestFit = txt.resizeTextForBestFit;
                text.textAttribute.resizeTextMaxSize = txt.resizeTextMaxSize;
                text.textAttribute.resizeTextMinSize = txt.resizeTextMinSize;
                text.textAttribute.supportRichText = txt.supportRichText;
                text.textAttribute.verticalOverflow = txt.verticalOverflow;
                text.textAttribute.color = txt.color;
                text.textAttribute.text = text.buffer.AddString(txt.text);
                var mat = txt.material;
                text.textAttribute.material = text.buffer.AddString(mat.name);
                text.textAttribute.shader = text.buffer.AddString(mat.shader.name);
                if (txt.font != null)
                    text.textAttribute.font = text.buffer.AddString(txt.font.name);
            }
        }
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            Load(tar, ref this.textAttribute);
            var txt = tar.GetComponent<Text>();
            if (smat != null)
                if (smat != "Default UI Material")
                    txt.material = new Material(Shader.Find(shader));
            txt.font = FindFont(fontName);
            txt.text = text;
        }
        public override void Save(GameObject tar)
        {
            base.Save(tar);
            Save(tar, this);
        }
    }
    public class EmojiTextElement : TextElement
    {
        public override void Load(GameObject tar)
        {
            base.Load(tar);
            var mat = tar.GetComponent<Graphic>().material;
            mat.SetTexture("_emoji",EmojiText.Emoji);
        }
    }
}
