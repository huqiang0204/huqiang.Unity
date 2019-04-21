using huqiang.Data;
using huqiang.Manager2D;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public unsafe struct TextData
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
        public Int32 shader;
        public static int Size = sizeof(TextData);
        public static int ElementSize = Size / 4;
    }
    public class TextElement : DataConversion
    {
        public static List<Font> fonts = new List<Font>();
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
        Text Context;
        public TextData data;
        string shader;
        string fontName;
        string text;
        string spriteName;
        public unsafe override void Load(FakeStruct fake)
        {
            data = *(TextData*)fake.ip;
            shader = fake.buffer.GetData(data.shader) as string;
            text = fake.buffer.GetData(data.text) as string;
            fontName = fake.buffer.GetData(data.font) as string;
        }
        public override void LoadToObject(Component game)
        {
            LoadToObject(game, ref data, this);
        }
        public static void LoadToObject(Component game, ref TextData dat, TextElement image)
        {
            var a = game.GetComponent<Text>();
            if (a == null)
                return;
            a.alignByGeometry = dat.alignByGeometry;
            a.alignment = dat.alignment;
            a.fontSize = dat.fontSize;
            a.fontStyle = dat.fontStyle;
            a.horizontalOverflow = dat.horizontalOverflow;
            a.lineSpacing = dat.lineSpacing;
            a.resizeTextForBestFit = dat.resizeTextForBestFit;
            a.resizeTextMaxSize = dat.resizeTextMaxSize;
            a.resizeTextMinSize = dat.resizeTextMinSize;
            a.supportRichText = dat.supportRichText;
            a.verticalOverflow = dat.verticalOverflow;
            a.color = dat.color;
            a.raycastTarget = false;
            a.color = dat.color;
            if (image.shader != "Default UI Material")
                a.material = new Material(Shader.Find(image.shader));
            a.font = FindFont(image.fontName);
            a.text = image.text;
            image.Context = a;
            
        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var txt = com as Text;
            if (txt == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, TextData.ElementSize);
            TextData* data = (TextData*)fake.ip;
            data->alignByGeometry = txt.alignByGeometry;
            data->alignment = txt.alignment;
            data->fontSize = txt.fontSize;
            data->fontStyle = txt.fontStyle;
            data->horizontalOverflow = txt.horizontalOverflow;
            data->lineSpacing = txt.lineSpacing;
            data->resizeTextForBestFit = txt.resizeTextForBestFit;
            data->resizeTextMaxSize = txt.resizeTextMaxSize;
            data->resizeTextMinSize = txt.resizeTextMinSize;
            data->supportRichText = txt.supportRichText;
            data->verticalOverflow = txt.verticalOverflow;
            data->color = txt.color;
            data->text = buffer.AddData(txt.text);
            if (txt.font != null)
                data->font = buffer.AddData(txt.font.name);
            if (txt.material != null)
                data->shader = buffer.AddData(txt.material.shader.name);
            return fake;
        }
    }
}
