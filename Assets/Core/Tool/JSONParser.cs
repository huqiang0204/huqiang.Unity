
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace TinyJson
{
    public  class JSONParser
    {
        Stack<List<string>> splitArrayPool;
        StringBuilder stringBuilder;
        Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache;
        //Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache;
        public static T FromJson<T>(string json)
        {
           return (T) new JSONParser().FromJson(typeof(T),json);
        }
        public object FromJson(Type type, string json)
        {
            // Initialize, if needed, the ThreadStatic variables
            //if (null == propertyInfoCache) propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            if (null == fieldInfoCache) fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            if (null == stringBuilder) stringBuilder = new StringBuilder();
            if (null == splitArrayPool) splitArrayPool = new Stack<List<string>>();

            //Remove all whitespace not within strings to make parsing simpler
            stringBuilder.Length = 0;
            var arry = json.ToCharArray();
            for (int i = 0; i < json.Length; i++)
            {
                char c = arry[i];
                if (c == '"')
                {
                    i = AppendUntilStringEnd(true, i, json);
                    continue;
                }
                if (char.IsWhiteSpace(c))
                    continue;

                stringBuilder.Append(c);
            }

            //Parse the thing!
            return ParseValue(type, stringBuilder.ToString());
        }

        int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            char[] arry = json.ToCharArray();
            stringBuilder.Append(arry[startIdx]);
            for (int i = startIdx+1; i<arry.Length; i++)
            {
                if (arry[i] == '\\')
                {
                    if (appendEscapeCharacter)
                        stringBuilder.Append(arry[i]);
                    stringBuilder.Append(arry[i + 1]);
                    i++;//Skip next character as it is escaped
                }
                else if (arry[i] == '"')
                {
                    stringBuilder.Append(arry[i]);
                    return i;
                }
                else
                    stringBuilder.Append(arry[i]);
            }
            return json.Length - 1;
        }

        //Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings
        List<string> Split(string json)
        {
            List<string> splitArray = splitArrayPool.Count > 0 ? splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();
            if(json.Length == 2)
                return splitArray;
            int parseDepth = 0;
            stringBuilder.Length = 0;
            char[] arry = json.ToCharArray();
            for (int i = 1; i<json.Length-1; i++)
            {
                switch (arry[i])
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '"':
                        i = AppendUntilStringEnd(true, i, json);
                        continue;
                    case ',':
                    case ':':
                        if (parseDepth == 0)
                        {
                            splitArray.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }

                stringBuilder.Append(arry[i]);
            }

            splitArray.Add(stringBuilder.ToString());

            return splitArray;
        }

        internal  object ParseValue(Type type, string json)
        {
            char[] arry = json.ToCharArray();
            if (type == typeof(string))
            {
                if (json.Length <= 2)
                    return string.Empty;
                if (json == "null")
                    return null;
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 1; i<json.Length-1; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length - 1)
                    {
                        int j = "\"\\nrtbf/".IndexOf(arry[i + 1]);
                        if (j >= 0)
                        {
                            stringBuilder.Append("\"\\\n\r\t\b\f/".ToCharArray()[j]);
                            ++i;
                            continue;
                        }
                        if (arry[i + 1] == 'u' && i + 5 < arry.Length - 1)
                        {
                            UInt32 c = 0;
                            if (UInt32.TryParse(json.Substring(i + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, null, out c))
                            {
                                stringBuilder.Append((char)c);
                                i += 5;
                                continue;
                            }
                        }
                    }
                    stringBuilder.Append(arry[i]);
                }
                return stringBuilder.ToString();
            }
            if (type == typeof(int))
            {
                int result;
                int.TryParse(json, out result);
                return result;
            }
            if (type == typeof(long))
            {
                long result;
                long.TryParse(json, out result);
                return result;
            }
            if (type == typeof(byte))
            {
                byte result;
                byte.TryParse(json, out result);
                return result;
            }
            if (type == typeof(float))
            {
                float result;
                float.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
            if (type == typeof(double))
            {
                double result;
                double.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
            if (type == typeof(bool))
            {
                return json.ToLower() == "true";
            }
            if (json == "null")
            {
                return null;
            }
            if (type.IsArray)
            {
                Type arrayType = type.GetElementType();
                if (arry[0] != '[' || arry[arry.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json);
                Array newArray = Array.CreateInstance(arrayType, elems.Count);
                for (int i = 0; i < elems.Count; i++)
                    newArray.SetValue(ParseValue(arrayType, elems[i]), i);
                splitArrayPool.Push(elems);
                return newArray;
            }
            //if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            //{
            //    Type listType = type.GetGenericArguments()[0];
            //    if (arry[0] != '[' || arry[arry.Length - 1] != ']')
            //        return null;
            //    List<string> elems = Split(json);
            //    var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });
            //    for (int i = 0; i < elems.Count; i++)
            //        list.Add(ParseValue(listType, elems[i]));
            //    splitArrayPool.Push(elems);
            //    return list;
            //}
            //if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            //{
            //    Type keyType, valueType;
            //    {
            //        Type[] args = type.GetGenericArguments();
            //        keyType = args[0];
            //        valueType = args[1];
            //    }
            //    //Refuse to parse dictionary keys that aren't of type string
            //    if (keyType != typeof(string))
            //        return null;
            //    //Must be a valid dictionary element
            //    if (arry[0] != '{' || arry[arry.Length - 1] != '}')
            //        return null;
            //    //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            //    List<string> elems = Split(json);
            //    if (elems.Count % 2 != 0)
            //        return null;
            //    var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });
            //    for (int i = 0; i < elems.Count; i += 2)
            //    {
            //        if (elems[i].Length <= 2)
            //            continue;
            //        string keyValue = elems[i].Substring(1, elems[i].Length - 2);
            //        object val = ParseValue(valueType, elems[i + 1]);
            //        dictionary.Add(keyValue, val);
            //    }
            //    return dictionary;
            //}
            if (type == typeof(object))
            {
                return ParseAnonymousValue(json);
            }    
            if (arry[0] == '{' && arry[arry.Length - 1] == '}')
            {
                return ParseObject(type, json);
            }
            return null;
        }

        object ParseAnonymousValue(string json)
        {
            if (json.Length == 0)
                return null;
            var arry = json.ToCharArray();
            if (arry[0] == '{' && arry[arry.Length - 1] == '}')
            {
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                    return null;
                var dict = new Dictionary<string, object>(elems.Count / 2);
                for (int i = 0; i < elems.Count; i += 2)
                    dict.Add(elems[i].Substring(1, elems[i].Length - 2), ParseAnonymousValue(elems[i + 1]));
                return dict;
            }
            if (arry[0] == '[' && arry[arry.Length - 1] == ']')
            {
                List<string> items = Split(json);
                var finalList = new List<object>(items.Count);
                for (int i = 0; i < items.Count; i++)
                    finalList.Add(ParseAnonymousValue(items[i]));
                return finalList;
            }
            if (arry[0] == '"' && arry[arry.Length - 1] == '"')
            {
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\", string.Empty);
            }
            if (char.IsDigit(arry[0]) || arry[0] == '-')
            {
                if (json.Contains("."))
                {
                    double result;
                    double.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                    return result;
                }
                else
                {
                    int result;
                    int.TryParse(json, out result);
                    return result;
                }
            }
            if (json == "true")
                return true;
            if (json == "false")
                return false;
            // handles json == "null" as well as invalid JSON
            return null;
        }

        object ParseObject(Type type, string json)
        {
            object instance = Activator.CreateInstance(type);

            //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            List<string> elems = Split(json);
            if (elems.Count % 2 != 0)
                return instance;

            Dictionary<string, FieldInfo> nameToField;
            //Dictionary<string, PropertyInfo> nameToProperty;
            if (!fieldInfoCache.TryGetValue(type, out nameToField))
            {
                var tp= type.GetFields();
                nameToField = GetPublickFeild(tp);
                fieldInfoCache.Add(type, nameToField);
            }

            //if (!propertyInfoCache.TryGetValue(type, out nameToProperty))
            //{
            //    nameToProperty = new Dictionary<string, PropertyInfo>();
            //    var pps = type.GetProperties();
            //    for (int i = 0; i < pps.Length; i++)
            //        nameToProperty.Add(pps[i].Name,pps[i]);
            //    propertyInfoCache.Add(type, nameToProperty);
            //}

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                    continue;
                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                FieldInfo fieldInfo;
                //PropertyInfo propertyInfo;
                if (nameToField.TryGetValue(key, out fieldInfo))
                    fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                //else if (nameToProperty.TryGetValue(key, out propertyInfo))
                //    propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
            }
            return instance;
        }
        Dictionary<string, FieldInfo> GetPublickFeild(FieldInfo[] feilds)
        {
            Dictionary<string, FieldInfo> pairs;
            pairs = new Dictionary<string, FieldInfo>();
            if (feilds != null)
                for (int i = 0; i < feilds.Length; i++)
                {
                    if (feilds[i].IsPublic)
                    {
                        if (pairs.ContainsKey(feilds[i].Name))
                        {
                            pairs.Remove(feilds[i].Name);
                        }
                        pairs.Add(feilds[i].Name, feilds[i]);
                    }
                }
            return pairs;
        }
    }
}
