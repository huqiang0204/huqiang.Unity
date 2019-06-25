using huqiang.UIModel;
using System;
using System.Collections.Generic;
using UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang
{
    public partial class TextInputEvent : EventCallBack
    {
        static string Number = "0123456789";
        static readonly char[] Separators = { ' ', '.', ',', '\t', '\r', '\n' };
        const string EmailCharacters = "!#$%&'*+-/=?^_`{|}~";

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
        
        public static int GetPressIndex(Text text, EventCallBack callBack, ref Vector3 point,UserAction action)
        {
            if (text == null)
                return -1;
            if (text.text == "" |text.text==null)
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
            return text.cachedTextGenerator.characterCountVisible;
        }
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
        static void GetChoiceArea(Text text, int startIndex, int endIndex, List<UIVertex> vert, List<int> tri, Color color)
        {
            if (text == null)
                return;
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
                int state = CommonArea(startIndex, endIndex, ref start, ref end);
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
        static void OnValueChange(TextInputEvent textInput, string con)
        {
            var text = textInput.TextCom;
            if (textInput.CharacterLimit > 0)
                if(textInput.InputString!=null)
                if (textInput.InputString.Length + con.Length > textInput.CharacterLimit)
                {
                    int len = textInput.CharacterLimit - textInput.InputString.Length;
                    if (len <= 0)
                        return;
                    con = con.Substring(0, len);
                }
            if (Validate(textInput.characterValidation, textInput.m_InputString, textInput.StartSelect, con[0]) == 0)
                return;
            if (textInput.ValidateChar != null)
                if (textInput.ValidateChar(textInput, textInput.StartSelect, con[0]) == 0)
                    return;
            DeleteSelected(textInput);
            textInput.FristString += con;
            ApplyChange(textInput, con.Length);
            if (textInput.OnValueChanged != null)
                textInput.OnValueChanged(textInput);
        }
        static void ApplyChange(TextInputEvent input, int len)
        {
            var text = input.TextCom;
            var str = input.InputString = input.FristString + input.EndString;
            Vector2 extents = text.rectTransform.rect.size;
            var settings = text.GetGenerationSettings(extents);
            input.TextCom.cachedTextGenerator.Populate(text.text, settings);
            int index = input.StartSelect + len;
            input.ChangePoint(index);
            if(len!=0)
            {
                if (index > 0)
                    input.FristString = str.Substring(0, index);
                else input.FristString = "";
                if (index < str.Length)
                    input.EndString = str.Substring(index, str.Length - index);
                else input.EndString = "";
            }
            UpdatePoint();
        }
        static char Validate(CharacterValidation validat, string text, int pos, char ch)
        {
            if (validat == CharacterValidation.None)
                return ch;
            if (validat == CharacterValidation.Integer)
            {
                if(ch=='-')
                {
                    if (text == "")
                        return ch;
                    if (text.Length > 0)
                        return (char)0;
                }
                if (Number.IndexOf(ch) < 0)
                    return (char)0;
                return ch;
            }
            else if (validat == CharacterValidation.Decimal)
            {
                if (Number.IndexOf(ch) < 0)
                {
                    if (ch == '.')
                        if (text.IndexOf('.') < 0)
                            return ch;
                    return (char)0;
                }
                return ch;
            }
            else if (validat == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (validat == CharacterValidation.numberAndName)
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
                    // Don't allow more than one apostrophe
                    if (!text.Contains("'"))
                        // Don't allow consecutive spaces and apostrophes.
                        if (!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                              ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                            return ch;
                }

                if (ch == ' ')
                {
                    // Don't allow consecutive spaces and apostrophes.
                    if (!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                          ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                        return ch;
                }
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (validat == CharacterValidation.Name)
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
                    // Don't allow more than one apostrophe
                    if (!text.Contains("'"))
                        // Don't allow consecutive spaces and apostrophes.
                        if (!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                              ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                            return ch;
                }

                if (ch == ' ')
                {
                    // Don't allow consecutive spaces and apostrophes.
                    if (!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
                          ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
                        return ch;
                }
            }
            else if (validat == CharacterValidation.EmailAddress)
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
        static bool DeleteSelected(TextInputEvent textInput)
        {
            if (textInput.EndSelect < 0)
                return false;
            int s = textInput.StartSelect;
            int e = textInput.EndSelect;
            if (e < s)
            {
                int a = s;
                s = e;
                e = a;
            }
            var text = textInput.TextCom;
            string str = textInput.m_InputString;
            textInput.FristString = str.Substring(0, s);
            if (e >= str.Length)
                textInput.EndString = "";
            else textInput.EndString = str.Substring(e, str.Length - e);
            textInput.InputString = textInput.FristString + textInput.EndString;
            textInput.EndSelect = -1;
            textInput.StartSelect = s;
            CaretStyle = 1;
            textInput.ChangePoint(s);
            UpdatePoint();
            return true;
        }

        public InputType inputType = InputType.Standard;
        public LineType lineType = LineType.MultiLineNewline;
        ContentType m_ctpye;
        bool multiLine = true;
        public ContentType contentType
        {
            get { return m_ctpye; }
            set
            {
                m_ctpye = value;
                switch (value)
                {
                    case ContentType.Standard:
                        {
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.Default;
                            characterValidation = CharacterValidation.None;
                            break;
                        }
                    case ContentType.Autocorrected:
                        {
                            inputType = InputType.AutoCorrect;
                            touchType = TouchScreenKeyboardType.Default;
                            characterValidation = CharacterValidation.None;
                            break;
                        }
                    case ContentType.IntegerNumber:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.NumberPad;
                            characterValidation = CharacterValidation.Integer;
                            break;
                        }
                    case ContentType.DecimalNumber:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.NumbersAndPunctuation;
                            characterValidation = CharacterValidation.Decimal;
                            break;
                        }
                    case ContentType.Alphanumeric:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.ASCIICapable;
                            characterValidation = CharacterValidation.Alphanumeric;
                            break;
                        }
                    case ContentType.Name:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.NamePhonePad;
                            characterValidation = CharacterValidation.Name;
                            break;
                        }
                    case ContentType.NumberAndName:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.NamePhonePad;
                            characterValidation = CharacterValidation.numberAndName;
                            break;
                        }
                    case ContentType.EmailAddress:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Standard;
                            touchType = TouchScreenKeyboardType.EmailAddress;
                            characterValidation = CharacterValidation.EmailAddress;
                            break;
                        }
                    case ContentType.Password:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Password;
                            touchType = TouchScreenKeyboardType.Default;
                            characterValidation = CharacterValidation.None;
                            break;
                        }
                    case ContentType.Pin:
                        {
                            lineType = LineType.SingleLine;
                            inputType = InputType.Password;
                            touchType = TouchScreenKeyboardType.NumberPad;
                            characterValidation = CharacterValidation.Integer;
                            break;
                        }
                    default:
                        {
                            // Includes Custom type. Nothing should be enforced.
                            break;
                        }
                }
            }
        }
        public CharacterValidation characterValidation = CharacterValidation.None;
        public TouchScreenKeyboardType touchType = TouchScreenKeyboardType.Default;
        public int CharacterLimit = 0;
        public Text TextCom { get; private set; }
        Vector3 Point;
        public int StartSelect;
        public int EndSelect;
        string m_InputString="";
        public string InputString
        {
            get { return m_InputString; }
            set
            {
                if (value != null)
                    m_InputString = value;
                else m_InputString = "";
                UpdateText();
            }
        }
        string m_TipString = "";
        public string TipString
        {
            get { return m_TipString; }
            set
            {
                m_TipString = value;
                if (m_InputString == null | m_InputString == "")
                    UpdateText();
            }
        }
        public bool ReadOnly;
        Color textColor;
        Color m_tipColor = new Color(0, 0, 0, 0.8f);
        public Color TipColor { get { return m_tipColor; } set { m_tipColor = value; UpdateText(); } }
        public Color CaretColor = new Color(1, 1, 1, 0.8f);
        public Color SelectionColor = new Color(0.65882f, 0.8078f, 1, 0.4f);
        public List<UIVertex> SelectVertex { get; private set; }
        public List<int> SelectTriAngle { get; private set; }
        public Func<TextInputEvent , int , char ,char> ValidateChar;
        public Action<TextInputEvent> OnValueChanged;
        public Action<TextInputEvent , UserAction> OnSelectChanged;
        public Action<TextInputEvent , UserAction> OnSelectEnd;
        public Action<TextInputEvent> OnSubmit;
        public override RectTransform Target
        {
            get
            {
                return m_Target;
            }
            protected set
            {
                base.Target = value;
                TextCom = m_Target.GetComponent<Text>();
                if (TextCom != null)
                {
                    textColor = TextCom.color;
                    UpdateText();
                }
            }
        }
        public TextInputEvent()
        {
            SelectVertex = new List<UIVertex>();
            SelectTriAngle = new List<int>();
        }

        public override void OnMouseDown(UserAction action)
        {
            if (TextCom != null)
            {
                if (m_InputString != null & m_InputString != "")
                    StartSelect = GetPressIndex(TextCom, this, ref Point,action);
                else StartSelect = 0;
                BindingText(this);
            }
            base.OnMouseDown(action);
            UpdateText();
        }
        protected override void OnDrag(UserAction action)
        {
            if(Pressed)
            if (TextCom != null)
            {
                if (entry)
                {
                    if (m_InputString != null & m_InputString != "")
                    {
                        if (action.Motion != Vector2.zero)
                        {
                            CaretStyle = 2;
                            Vector3 p = Vector3.zero;
                            int end = GetPressIndex(TextCom, this, ref p,action);
                            if (end != EndSelect)
                            {
                                EndSelect = end;
                                Selected();
                                if (OnSelectChanged != null)
                                    OnSelectChanged(this,action);
                            }
                        }
                    }
                }
                else
                {
                    if (action.Motion != Vector2.zero)
                    {
                    }
                }
            }
        }
        internal override void OnDragEnd(UserAction action)
        {
            long r = action.EventTicks - pressTime;
            if (r <= ClickTime)
            {
                float x = action.CanPosition.x;
                float y = action.CanPosition.y;
                x -= RawPosition.x;
                x *= x;
                y -= RawPosition.y;
                y *= y;
                x += y;
                if (x < ClickArea)
                    return;
            }
            if (InputString == "")
                return;
            if (InputString == null)
                return;
            var p = Vector3.zero;
            EndSelect = GetPressIndex(TextCom, this, ref p,action);
            Selected();
            if (OnSelectEnd != null)
                OnSelectEnd(this,action);
        }
        void Selected()
        {
            SelectVertex.Clear();
            SelectTriAngle.Clear();
            if (EndSelect < StartSelect)
            {
                GetChoiceArea(TextCom, EndSelect, StartSelect, SelectVertex, SelectTriAngle, SelectionColor);
                GetSelectString(EndSelect,StartSelect);
            }
            else {
                GetChoiceArea(TextCom, StartSelect, EndSelect, SelectVertex, SelectTriAngle, SelectionColor);
                GetSelectString(StartSelect,EndSelect);
            }
            SelectChanged(this);
            CaretStyle = 2;
        }
        public void ChangePoint(int index)
        {
            if (TextCom != null)
            {
                if (index < 0)
                    index = 0;
                if (m_InputString == null)
                    index = 0;
                else if (index > m_InputString.Length)
                    index = m_InputString.Length;
                if (TextCom.text == null)
                    index = 0;
                var o = GetIndexPoint(TextCom, index, ref Point);
                StartSelect = index;
                var vert = SelectVertex;
                var tri = SelectTriAngle;
                vert.Clear();
                tri.Clear();
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
        }
        void UpdateText()
        {
            if (TextCom == null)
                return;
            string str = m_InputString;
            if (str == null | str == "")
            {
                TextCom.color = m_tipColor;
                TextCom.text = m_TipString;
            }
            else
            {
                TextCom.color = textColor;
                if (contentType == ContentType.Password)
                {
                    TextCom.text = new string('*', str.Length);
                }
                else
                {
                    TextCom.text = m_InputString;
                }
            }
        }
        string FristString;
        string EndString;
        public string SelectString { get; private set; }
        void GetSelectString(int s, int e)
        {
            string str = InputString;
            SelectString = "";
            if (s < 0)
                return;
            if (str == null)
                return;
            if (s > str.Length)
                return;
            int len = e - s + 1;
            if (s + len > str.Length)
                len = str.Length - s;
            SelectString = str.Substring(s, len);
        }
        public void SetStartSelect(UserAction action)
        {
            var crood= Tool.GetGlobaInfo(m_Target,false);
            GlobalPosition = crood.Postion;
            GlobalRotation = crood.quaternion;
            GlobalScale = crood.Scale;
            OnMouseDown(action);
        }
        public void CutSelected()
        {
            GUIUtility.systemCopyBuffer = SelectString;
            DeleteSelected(this);
        }
        public void CopySelected()
        {
            GUIUtility.systemCopyBuffer = SelectString;
        }
        public void Paste()
        {
            string str = GUIUtility.systemCopyBuffer;
            if (str == null)
                return;
            OnValueChange(this, str);
        }
    }
    public partial class TextInputEvent
    {
        public enum ContentType
        {
            Standard,
            Autocorrected,
            IntegerNumber,
            DecimalNumber,
            Alphanumeric,
            Name,
            NumberAndName,
            EmailAddress,
            Password,
            Pin,
            Custom
        }

        public enum InputType
        {
            Standard,
            AutoCorrect,
            Password,
        }


        public enum LineType
        {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        internal static TextInputEvent InputEvent;

        static int CaretStyle;
        static void BindingText(TextInputEvent eventCall)
        {
            Input.imeCompositionMode = IMECompositionMode.On;
            if (InputEvent != null)
            {
                if (InputEvent == eventCall)
                {
                    return;
                }
                if (InputEvent.OnSubmit != null)
                    InputEvent.OnSubmit(InputEvent);
            }
            InputEvent = eventCall;
            InputEvent.Click = OnClick;
            InputEvent.LostFocus = OnLostFocus;
            if (m_Caret != null)
            {
                var t = m_Caret.transform;
                t.SetParent(eventCall.Target);
                t.localPosition = Vector3.zero;
                t.localScale = Vector3.one;
                t.localRotation = Quaternion.identity;
            }
        }
        static void SelectChanged(TextInputEvent inputEvent)
        {
            if (m_Caret != null)
            {
                UpdatePoint();
                m_Caret.gameObject.SetActive(true);
            }
        }
        static TouchScreenKeyboard m_touch;
        static bool IsTouchKeyboard;
        static void OnEdit(TextInputEvent input)
        {
            UpdatePoint();
            if (input.ReadOnly)
                return;
            if (Application.platform == RuntimePlatform.Android |
                Application.platform == RuntimePlatform.IPhonePlayer |
                Application.platform == RuntimePlatform.WSAPlayerARM |
                Application.platform == RuntimePlatform.WSAPlayerX64 |
                Application.platform == RuntimePlatform.WSAPlayerX86)
            {
                IsTouchKeyboard = true;
                if (InputEvent.contentType == ContentType.Password)
                {
                    m_touch = TouchScreenKeyboard.Open("", InputEvent.touchType, true, InputEvent.multiLine, true);
                }
                else
                    m_touch = TouchScreenKeyboard.Open("", InputEvent.touchType, true, InputEvent.multiLine);
                m_touch.text = input.InputString;
            }
            else IsTouchKeyboard = false;
            if (input.TextCom != null)
            {
                string str = input.m_InputString;
                if (str != null)
                {
                    int s = input.StartSelect;
                    if (s > 0)
                        input.FristString = str.Substring(0, s);
                    else input.FristString = "";
                    if (s < str.Length)
                        input.EndString = str.Substring(s, str.Length - s);
                    else input.EndString = "";
                }
            }
        }
        static void OnClick(EventCallBack eventCall,UserAction action)
        {
            TextInputEvent input = eventCall as TextInputEvent;
            if (input == null)
                return;
             input.TextCom.color = input.textColor;
            if (input.contentType == ContentType.Password)
            {
                input.TextCom.text = new string('*', input.m_InputString.Length);
            }
            else
            {
                input.TextCom.text =input.m_InputString;
            }
            input.ChangePoint(input.StartSelect);
            input.EndSelect = -1;
            CaretStyle = 1;
            OnEdit(input);
        }
        static void OnLostFocus(EventCallBack eventCall,UserAction action)
        {
            TextInputEvent text = eventCall as TextInputEvent;
            if (text == InputEvent)
            {
                if (InputEvent.OnSubmit != null)
                    InputEvent.OnSubmit(InputEvent);
                InputEvent = null;
                CaretStyle = 0;
                ShowCaret();
            }
            text.UpdateText();
        }
        public static void SetCurrentInput(TextInputEvent input,UserAction action)
        {
            if (input == null)
                return;
            if (InputEvent == input)
                return;
            if (InputEvent != null)
                OnLostFocus(InputEvent,action);
            BindingText(input);
            InputEvent = input;
            input.SetStartSelect(action);
            input.EndSelect = input.StartSelect;
            input.ChangePoint(input.StartSelect);
            CaretStyle = 1;
            OnEdit(input);
        }
        static CustomRawImage m_Caret;
        static CustomRawImage Caret
        {
            get
            {
                if (m_Caret == null)
                {
                    var g = new GameObject("m_caret", typeof(CustomRawImage));
                    g.name = "m_caret";
                    m_Caret = g.GetComponent<CustomRawImage>();
                    m_Caret.rectTransform.sizeDelta = Vector2.zero;
                }
                else if (m_Caret.name == "buff")
                {
                    var g = new GameObject("m_caret", typeof(CustomRawImage));
                    g.name = "m_caret";
                    m_Caret = g.GetComponent<CustomRawImage>();
                    m_Caret.rectTransform.sizeDelta = Vector2.zero;
                }
                return m_Caret;
            }
        }

        static void UpdatePoint()
        {
            if (m_Caret != null)
            {
                if (InputEvent != null)
                {
                    m_Caret.uIVertices = InputEvent.SelectVertex;
                    m_Caret.triangle = InputEvent.SelectTriAngle;
                    m_Caret.Refresh();
                    time = 0;
                }
            }
        }
        static int GetDifferent(string a, string b)
        {
            int len = a.Length;
            if (len > b.Length)
                len = b.Length;
            for (int i = 0; i < len; i++)
                if (a[i] != b[i])
                    return i;
            return len;
        }
        internal static void Dispatch()
        {
            if (InputEvent != null)
            {
                if (!InputEvent.ReadOnly)
                    if (!InputEvent.Pressed)
                    {
                        if (IsTouchKeyboard)
                        {
                            CaretStyle = 0;
                            var str = m_touch.text;
                            var com = InputEvent.InputString;
                            if (com == null)
                            {
                                if (str != null)
                                {
                                    OnValueChange(InputEvent, str);
                                    m_touch.text = InputEvent.m_InputString;
                                }
                            }
                            else
                            {
                                if (str != null)
                                {
                                    if (com != str)
                                    {
                                        if (com.Length < str.Length)
                                        {
                                            int s = GetDifferent(com, str);
                                            int len = str.Length - com.Length;
                                            var input = str.Substring(s, len);
                                            InputEvent.FristString = com.Substring(0, s);
                                            InputEvent.EndString = com.Substring(s, com.Length - s);
                                            InputEvent.StartSelect = s;
                                            OnValueChange(InputEvent, input);
                                            m_touch.text = InputEvent.m_InputString;
                                        }
                                        else if (com.Length > str.Length)
                                        {
                                            int s = GetDifferent(com, str);
                                            int len = com.Length - str.Length;
                                            InputEvent.InputString = com.Remove(s, len);
                                            if (InputEvent.OnValueChanged != null)
                                                InputEvent.OnValueChanged(InputEvent);
                                        }
                                        else
                                        {
                                            int len = str.Length;
                                            int s = 0;int e = len - 1;
                                            bool frist = false;
                                            for(int i=0;i<len;i++)
                                            {
                                                if(frist)
                                                {
                                                    if (str[i] == com[i])
                                                    {
                                                        e = i;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (str[i] != com[i])
                                                    {
                                                        frist = true;
                                                        s = i;
                                                    }
                                                }
                                       
                                            }
                                            len = e - s;
                                            var input = str.Substring(s, len);
                                            InputEvent.FristString = com.Substring(0, s);
                                            InputEvent.EndString = com.Substring(s, com.Length - s);
                                            InputEvent.StartSelect = s;
                                            OnValueChange(InputEvent, input);
                                            m_touch.text = InputEvent.m_InputString;
                                        }
                                    }
                                }
                            }
                            if (m_touch.done)
                            {
                                if (InputEvent.OnSubmit != null)
                                    InputEvent.OnSubmit(InputEvent);
                                InputEvent = null;
                                ShowCaret();
                                return;
                            }
                        }
                        else
                        {
                            var state = KeyPressed();
                            if (state == EditState.Continue)
                            {
                                string str = Input.inputString;
                                if (str != null & str != "")
                                    OnValueChange(InputEvent, str);
                            }
                            else if (state == EditState.Finish)
                            {
                                if (InputEvent.OnSubmit != null)
                                    InputEvent.OnSubmit(InputEvent);
                                InputEvent = null;
                                CaretStyle = 0;
                                ShowCaret();
                                return;
                            }
                            else if (state == EditState.NewLine)
                            {
                                OnValueChange(InputEvent, "\r\n");
                                if (InputEvent.OnValueChanged != null)
                                    InputEvent.OnValueChanged(InputEvent);
                                return;
                            }
                        }
                    }
            }
            ShowCaret();
        }
        static float time;
        static void ShowCaret()
        {
            switch (CaretStyle)
            {
                case 1:
                    time += Time.deltaTime;
                    if (time > 1.6f)
                    {
                        time = 0;
                    }
                    else if (time > 0.8f)
                    {
                        Caret.gameObject.SetActive(false);
                    }
                    else
                    {
                        Caret.gameObject.SetActive(true);
                    }
                    break;
                case 2:

                    break;
                default:
                    Caret.gameObject.SetActive(false);
                    break;
            }
        }
        enum EditState
        {
            Done,
            Continue,
            NewLine,
            Finish
        }
        /// <summary>
        /// 每秒5次
        /// </summary>
        static float KeySpeed = 0.22f;
        static float MaxSpeed = 0.03f;
        static float KeyPressTime;
        static EditState KeyPressed()
        {
            KeyPressTime -= Time.deltaTime;
            if (Input.GetKey(KeyCode.Backspace))
            {
                if (KeyPressTime <= 0)
                {
                    if (InputEvent != null)
                    {
                        if (!DeleteSelected(InputEvent))
                        {
                            var str = InputEvent.FristString;
                            if(str!=null)
                            if (str.Length > 0)
                            {
                                int len = 1;
                                if (str.Length > 1)
                                    if (str[str.Length - 2] == '\r')
                                        len = 2;
                                InputEvent.FristString = str.Substring(0, str.Length - len);
                                ApplyChange(InputEvent, -len);
                                    if (InputEvent.OnValueChanged != null)
                                        InputEvent.OnValueChanged(InputEvent);
                                }
                        }
                    }
                    KeySpeed *= 0.8f;
                    if (KeySpeed < MaxSpeed)
                        KeySpeed = MaxSpeed;
                    KeyPressTime = KeySpeed;
                }
                return EditState.Done;
            }
            if (Input.GetKey(KeyCode.Delete))
            {
                if (KeyPressTime <= 0)
                {
                    if (InputEvent != null)
                    {
                        if (!DeleteSelected(InputEvent))
                        {
                            var str = InputEvent.EndString;
                            if(str!=null)
                            if (str.Length > 0)
                            {
                                int len = 1;
                                if (str.Length > 1)
                                    if (str[1] == '\n')
                                        len = 2;
                                InputEvent.EndString = str.Substring(len, str.Length - len);
                                ApplyChange(InputEvent, 0);
                                    if (InputEvent.OnValueChanged != null)
                                        InputEvent.OnValueChanged(InputEvent);
                                }
                        }
                    }
                    KeySpeed *= 0.5f;
                    if (KeySpeed < MaxSpeed)
                        KeySpeed = MaxSpeed;
                    KeyPressTime = KeySpeed;
                }
                return EditState.Done;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (KeyPressTime <= 0)
                {
                    if (InputEvent != null)
                    {
                        if (!DeleteSelected(InputEvent))
                        {
                            var str = InputEvent.EndString;
                            if(str!=null)
                            if (str.Length > 0)
                            {
                                int len = 1;
                                if (str.Length > 1)
                                    if (str[1] == '\n')
                                        len = 2;
                                ApplyChange(InputEvent, -len);
                            }
                        }
                    }
                    KeySpeed *= 0.5f;
                    if (KeySpeed < MaxSpeed)
                        KeySpeed = MaxSpeed;
                    KeyPressTime = KeySpeed;
                }
                return EditState.Done;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (KeyPressTime <= 0)
                {
                    if (InputEvent != null)
                    {
                        if (!DeleteSelected(InputEvent))
                        {
                            var str = InputEvent.EndString;
                            if(str!=null)
                            if (str.Length > 0)
                            {
                                int len = 1;
                                if (str.Length > 1)
                                    if (str[1] == '\n')
                                        len = 2;
                                ApplyChange(InputEvent, len);
                            }
                        }
                    }
                    KeySpeed *= 0.5f;
                    if (KeySpeed < MaxSpeed)
                        KeySpeed = MaxSpeed;
                    KeyPressTime = KeySpeed;
                }
                return EditState.Done;
            }
            KeySpeed = 0.3f;
            if (Input.GetKeyDown(KeyCode.Home))
            {
                InputEvent.ChangePoint(0);
                return EditState.Done;
            }
            if (Input.GetKeyDown(KeyCode.End))
            {
                InputEvent.ChangePoint(InputEvent.InputString.Length);
                return EditState.Done;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl))
                {
                    if (InputEvent != null)
                    {
                        InputEvent.StartSelect = 0;
                        InputEvent.EndSelect = InputEvent.TextCom.text.Length;
                        InputEvent.Selected();
                    }
                    return EditState.Done;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Return) | Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (InputEvent.lineType != LineType.MultiLineNewline)
                {
                    return EditState.Finish;
                }
                else return EditState.NewLine;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                return EditState.Finish;
            }
            return EditState.Continue;
        }
        public static void Reset()
        {
            InputEvent = null;
            //if (m_Caret != null)
            //    ModelManager.RecycleGameObject(m_Caret.gameObject);
            //if (m_Caret != null)
            //    GameObject.Destroy(m_Caret.gameObject);
            //m_Caret = null;
        }
        public enum CharacterValidation
        {
            None,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            numberAndName,
            EmailAddress,
            Custom
        }
    }
}