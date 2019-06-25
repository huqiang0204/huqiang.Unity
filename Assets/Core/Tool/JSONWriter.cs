
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TinyJson
{
    public  class JSONWriter
    {
        public static string ToJson(object item)
        {
            StringBuilder stringBuilder = new StringBuilder();
            new JSONWriter().AppendValue(stringBuilder, item);
            return stringBuilder.ToString();
        }

         void AppendValue(StringBuilder stringBuilder, object item)
        {
            
            if (item == null)
            {
                stringBuilder.Append("null");
                return;
            }

            Type type = item.GetType();
            if (type == typeof(string))
            {
                stringBuilder.Append('"');
                string str = (string)item;
                for (int i = 0; i<str.Length; ++i)
                    if (str[i] < ' ' || str[i] == '"' || str[i] == '\\')
                    {
                        stringBuilder.Append('\\');
                        int j = "\"\\\n\r\t\b\f".IndexOf(str[i]);
                        if (j >= 0)
                            stringBuilder.Append("\"\\nrtbf"[j]);
                        else
                            stringBuilder.AppendFormat("u{0:X4}", (UInt32)str[i]);
                    }
                    else
                        stringBuilder.Append(str[i]);
                stringBuilder.Append('"');
            }
            else if (type == typeof(byte) || type == typeof(int))
            {
                stringBuilder.Append(item.ToString());
            }
            else if (type == typeof(float))
            {
                stringBuilder.Append(((float)item).ToString());//
            }
            else if (type == typeof(double))
            {
                stringBuilder.Append(((double)item).ToString());//System.Globalization.CultureInfo.InvariantCulture
            }
            else if (type == typeof(bool))
            {
                stringBuilder.Append(((bool)item) ? "true" : "false");
            }
            else if (item is IList)
            {
                stringBuilder.Append('[');
                bool isFirst = true;
                IList list = item as IList;
                for (int i = 0; i < list.Count; i++)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        stringBuilder.Append(',');
                    AppendValue(stringBuilder, list[i]);
                }
                stringBuilder.Append(']');
            }
            else
            {
                FieldInfo[] fieldInfos = type.GetFields();
                if (fieldInfos != null)
                {
                    stringBuilder.Append('{');
                    bool isFirst = true;
                    for (int i = 0; i < fieldInfos.Length; i++)
                    {
                        var field = fieldInfos[i];
                        var name = field.Name;
                        var ss = name.ToCharArray();
                        if (ss[0] != '_')
                        {
                            if (field.IsPublic && !field.IsStatic)
                            {
                                object value = field.GetValue(item);
                                if (isFirst)
                                    isFirst = false;
                                else
                                    stringBuilder.Append(',');
                                stringBuilder.Append('\"');
                                stringBuilder.Append(field.Name);
                                stringBuilder.Append("\":");
                                AppendValue(stringBuilder, value);
                            }
                        }
                    }
                    stringBuilder.Append('}');
                }
            }
        }
    }
}
