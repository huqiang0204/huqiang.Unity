using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public partial class TextInput
    {
        public  int startSelect { get; private set; }
        public int endSelect { get; private set; }
        bool DeleteSelectString()
        {
            if(endSelect>-1)
            {
                int len = emojiString.Length;
                if (endSelect < startSelect)
                    emojiString.RemoveAt(endSelect, startSelect - endSelect+1);
                else emojiString.RemoveAt(startSelect,endSelect);
                if (endSelect < startSelect)
                    startSelect = endSelect;
                endSelect = -1;
                return true;
            }
            return false;
        }
        public void InsertString(string str)
        {
            DeleteSelectString();
            if (emojiString.FullString == "")
            {
                emojiString.FullString = str;
                startSelect = str.Length+1;
            }
            else
            {
                int len = emojiString.Length;
                emojiString.Insert(startSelect, str);
                startSelect += emojiString.Length-len;
            }
        }
        public bool MoveLeft()
        {
            startSelect = -1;
            if (startSelect > 0)
            {
                startSelect--;
                return true;
            }
            return false;
        }
        public bool MoveRight()
        {
            endSelect = -1;
            if(startSelect< emojiString.Length)
            {
                startSelect++;
                return true;
            }
            return false;
        }
        public bool DeleteLeft()
        {
            if(!DeleteSelectString())
            {
                if (startSelect > 0)
                {
                    emojiString.RemoveAt(startSelect-1,1);
                    startSelect--;
                    return true;
                }
                else return false;
            }
            return true;
        }
        public bool DeleteRight()
        {
            if (!DeleteSelectString())
            {
                if (startSelect < emojiString.Length)
                {
                    emojiString.RemoveAt(startSelect,1);
                    return true;
                }
                else return false;
            }
            else return true;
        }

        TextInputEvent.CharacterValidation charValidation;
        int caretPos;
        int caretSelectPos;
        const string EmailCharacters = "!#$%&'*+-/=?^_`{|}~";
        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (charValidation == TextInputEvent.CharacterValidation.None)
                return ch;
            if (charValidation == TextInputEvent.CharacterValidation.Integer || charValidation == TextInputEvent.CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool dashInSelection = text.Length > 0 && text[0] == '-' && ((caretPos == 0 && caretSelectPos > 0) || (caretSelectPos == 0 && caretPos > 0));
                bool selectionAtStart = caretPos == 0 || caretSelectPos == 0;
                if (!cursorBeforeDash || dashInSelection)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart)) return ch;
                    if (ch == '.' && charValidation == TextInputEvent.CharacterValidation.Decimal && !text.Contains(".")) return ch;
                }
            }
            else if (charValidation == TextInputEvent.CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (charValidation == TextInputEvent.CharacterValidation.Name)
            {
                if (char.IsLetter(ch))
                {
                    // Character following a space should be in uppercase.
                    if (char.IsLower(ch) && ((pos == 0) || (text[pos - 1] == ' ')))
                    {
                        return char.ToUpper(ch);
                    }

                    // Character not following a space or an apostrophe should be in lowercase.
                    if (char.IsUpper(ch) && (pos > 0) && (text[pos - 1] != ' ') && (text[pos - 1] != '\''))
                    {
                        return char.ToLower(ch);
                    }

                    return ch;
                }

                if (ch == '\'')
                {
                    if (!text.Contains("'"))
                        if (!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                              ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                            return ch;
                }

                if (ch == ' ')
                {
                    if (!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                          ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                        return ch;
                }
            }
            else if (charValidation == TextInputEvent.CharacterValidation.EmailAddress)
            {
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (EmailCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            return (char)0;
        }

        #region Event
        /// <summary>
        /// 获取当前索引文字位置
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool GetIndexPoint(Text text, int index, ref Vector3 point)
        {
            ///仅可见区域的顶点 0=左上1=右上2=右下3=左下
            IList<UIVertex> vertex = text.cachedTextGenerator.verts;
            ///仅可见区域的行数
            IList<UILineInfo> lines = text.cachedTextGenerator.lines;
            float top = lines[lines.Count - 1].topY;
            float high = lines[lines.Count - 1].height;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].startCharIdx > index)
                {
                    top = lines[i - 1].topY;
                    high = lines[i - 1].height;
                    break;
                }
            }
            int max = vertex.Count;
            index *= 4;
            if (index >= max)
                index = max - 3;
            point.x = vertex[index].position.x;
            float y = vertex[index].position.y;
            float down = top - high;
            point.z = high;
            point.y = top - high * 0.5f;
            if (down > y)
            {
                point.y -= high;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取当前鼠标点击到的文字位置
        /// </summary>
        /// <param name="text"></param>
        /// <param name="callBack"></param>
        /// <param name="point">如果选择，则传递选中的位置</param>
        /// <param name="action"></param>
        /// <returns>返回-1未选中,范围为0到显示结尾</returns>
        public static int GetPressIndex(Text text, EventCallBack callBack, ref Vector3 point, UserAction action)
        {
            if (text == null)
                return -1;
            if (text.text == "" | text.text == null)
                return -1;
            float fs = text.fontSize;
            IList<UILineInfo> lines = text.cachedTextGenerator.lines;
            IList<UIVertex> vertex = text.cachedTextGenerator.verts;
            var pos = callBack.GlobalPosition;
            var scale = callBack.GlobalScale;
            for (int i = 0; i < lines.Count; i++)
            {
                float top = lines[i].topY;
                float high = lines[i].height;
                float down = top - high;
                down *= scale.y;
                down += pos.y;
                if (down < action.CanPosition.y)
                {
                    int index = lines[i].startCharIdx;
                    int end = text.text.Length - index;
                    if (i < lines.Count - 1)
                        end = lines[i + 1].startCharIdx - index;
                    int p = index * 4;
                    for (int j = 0; j < end; j++)
                    {
                        float x = vertex[p + 2].position.x;
                        x *= scale.x;
                        x += pos.x;
                        if (x > action.CanPosition.x)
                        {
                            point.x = vertex[p].position.x;
                            point.y = top - high * 0.5f;
                            point.z = high;
                            return index;
                        }
                        index++;
                        p += 4;
                        if (p + 2 >= vertex.Count)
                            break;
                    }
                    if (index == text.text.Length)
                    {
                        float it = lines[lines.Count - 1].topY;
                        float ih = lines[lines.Count - 1].height;
                        point.x = vertex[vertex.Count - 2].position.x;
                        point.y = it - ih * 0.5f;
                        point.z = ih;
                    }
                    else
                    {
                        point.x = vertex[p - 4].position.x;
                        point.y = top - high * 0.5f;
                        point.z = high;
                    }
                    return index;
                }
            }
            float t = lines[lines.Count - 1].topY;
            float h = lines[lines.Count - 1].height;
            point.x = vertex[vertex.Count - 1].position.x;
            point.y = t - h * 0.5f;
            point.z = h;
            return text.cachedTextGenerator.characterCountVisible-1;
        }
        /// <summary>
        /// 获取选择矩形的左右坐标
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        static Vector2 GetLineRect(IList<UIVertex> vertex, int start, int end)
        {
            if (vertex.Count == 0)
                return Vector2.zero;
            int s = start * 4;
            int e = end * 4 + 2;
            if (e > vertex.Count)
                e = vertex.Count - 1;
            return new Vector2(vertex[s].position.x, vertex[e].position.x);
        }
        static int CommonArea(int s1, int e1, ref int s2, ref int e2)
        {
            if (s1 > e2)
                return 0;
            if (s2 > e1)
                return 2;
            if (s2 < s1)
                s2 = s1;
            if (e2 > e1)
                e2 = e1;
            return 1;
        }
        /// <summary>
        /// 获取选中的区域
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="vert"></param>
        /// <param name="tri"></param>
        /// <param name="color"></param>
        public static void GetChoiceArea(Text text, List<UIVertex> vert, List<int> tri, Color color,int startSelect,int endSelect)
        {
            EmojiText.CreateEmojiMesh(text, new List<EmojiInfo>());
            IList<UILineInfo> lines = text.cachedTextGenerator.lines;
            IList<UIVertex> vertex = text.cachedTextGenerator.verts;
            float top = 0;
            float down = 0;
            int end = text.cachedTextGenerator.characterCount;
            int max = lines.Count;
            int se = max - 1;
            for (int i = 0; i < lines.Count; i++)
            {
                int start = lines[i].startCharIdx;
                if (i < se)
                {
                    end = lines[i + 1].startCharIdx - 1;
                }
                else
                {
                    end = text.cachedTextGenerator.characterCountVisible;
                }
                int state;
                if (startSelect < endSelect)
                    state = CommonArea(startSelect, endSelect, ref start, ref end);
                else state = CommonArea(endSelect, startSelect, ref start, ref end);
                if (state == 2)
                {
                    break;
                }
                else
                if (state == 1)
                {
                    top = lines[i].topY;
                    down = top - lines[i].height;
                    var w = GetLineRect(vertex, start, end);
                    int st = vert.Count;
                    var v = new UIVertex();
                    v.position.x = w.x;
                    v.position.y = down;
                    v.color = color;
                    vert.Add(v);
                    v.position.x = w.x;
                    v.position.y = top;
                    v.color = color;
                    vert.Add(v);
                    v.position.x = w.y;
                    v.position.y = down;
                    v.color = color;
                    vert.Add(v);
                    v.position.x = w.y;
                    v.position.y = top;
                    v.color = color;
                    vert.Add(v);
                    tri.Add(st);
                    tri.Add(st + 1);
                    tri.Add(st + 2);
                    tri.Add(st + 2);
                    tri.Add(st + 1);
                    tri.Add(st + 3);
                }
            }
        }
        public static void GetCaretPoint(Text text,List<UIVertex> vert,List<int>tri,int index,Color CaretColor)
        {
            vert.Clear();
            tri.Clear();
            if (index < 0)
                return;
            EmojiText.CreateEmojiMesh(text,new List<EmojiInfo>());
            Vector3 Point = new Vector3();
            var o = GetIndexPoint(text, index, ref Point);
            if (o)
            {
                float left = Point.x - 0.5f;
                float right = Point.x + 0.5f;
                float h = Point.z;
                h *= 0.4f;
                float top = Point.y + h;
                float down = Point.y - h;
                var v = new UIVertex();
                v.position.x = left;
                v.position.y = down;
                v.color = CaretColor;
                vert.Add(v);
                v.position.x = left;
                v.position.y = top;
                v.color = CaretColor;
                vert.Add(v);
                v.position.x = right;
                v.position.y = down;
                v.color = CaretColor;
                vert.Add(v);
                v.position.x = right;
                v.position.y = top;
                v.color = CaretColor;
                vert.Add(v);
            }
            else
            {
                var v = new UIVertex();
                vert.Add(v);
                vert.Add(v);
                vert.Add(v);
                vert.Add(v);
            }
            tri.Add(0);
            tri.Add(1);
            tri.Add(2);
            tri.Add(2);
            tri.Add(1);
            tri.Add(3);
        }
        #endregion
    }
}
