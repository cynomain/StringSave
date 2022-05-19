using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

using _SSP = cynomain.StringSave.StringSaveParser;

namespace cynomain.StringSave
{

    [System.Serializable]
    public class StringStorage
    {
        public const string HEADER = "$SSV";
        public const int SAVE_VERSION = 1;

        public Dictionary<string, string> database; //key value

        /// <summary>
        /// Creates a new StringStorage object using a StringSave formatted string
        /// </summary>
        /// <param name="formatted"></param>
        public StringStorage(string formatted)
        {
            database = StringSaveParser.TextToStrStorage(formatted).database;
        }

        public StringStorage()
        {
            database = new Dictionary<string, string>();
        }

        public StringStorageObject this[string key]
        {
            get
            {
                return Get(key);
            }
        }

        public void Set<T>(string key, T value)
        {
            string result = "";
            bool isArray = typeof(T).IsArray;
            if (!isArray)
            {
                result = _SSP.V1Parser.ParseToText(value);
            }
            else
            {
                Debug.LogWarning("[StringSave] Type is an array. Please use SetArray instead.");
                return;
            }

            if (database.ContainsKey(key))
            {
                database[key] = result;
            }
            else
            {
                database.Add(key, result);
            }
        }

        public void SetArray<TArray>(string key, TArray[] arrayvalue)
        {
            if (database.ContainsKey(key))
            {
                database[key] = _SSP.V1Parser.ParseArrayToText(arrayvalue);
            }
            else
            {
                database.Add(key, _SSP.V1Parser.ParseArrayToText(arrayvalue));
            }
        }

        public StringStorageObject Get(string key)
        {
            if (!KeyExists(key))
            {
                Debug.LogError("[StringSave] Key not found in StringStorage: " + key);
                return null;
            }
            return new StringStorageObject(database[key]);
        }

        public bool KeyExists(string key)
        {
            return database.ContainsKey(key);
        }

        public bool TryGet(string key, out StringStorageObject sso)
        {
            sso = Get(key);
            return sso != null;
        }

        public StringStorageObject PrioritizeGet(params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (KeyExists(keys[i]))
                {
                    StringStorageObject sso = null;
                    if (TryGet(keys[i], out sso))
                    {
                        return sso;
                    }
                    //Doesnt return null incase something went wrong;
                }
            }
            Debug.LogError("[StringSave] Couldn't get a value from keys : " + keys); //CHANGE THIS
            return null;
        }

        public static readonly System.Type[] SUPPORTED_TYPES = new System.Type[] { 
            typeof(int), 
            typeof(float), 
            typeof(bool), 
            typeof(Vector2), 
            typeof(Vector3), 
            typeof(Quaternion), 
            typeof(long), 
            typeof(short), 
            typeof(string), 
            typeof(double), 
            typeof(decimal), 
            typeof(char) /*Enum*/ 
        };

        public static readonly System.Type[] SUPPORTED_ARRAY_TYPES = new System.Type[]
        {
            typeof(int),
            typeof(float),
            typeof(string),
            typeof(bool),
            typeof(byte)
        };

        /// <summary>
        /// Converts this StringStorage to a formatted string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                return StringSaveParser.StrStorageToText(this);
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }

    public class StringSaveParser
    {
        public static string StrStorageToText(StringStorage store)
        {
            string header = $"$SSV:{StringStorage.SAVE_VERSION}\n";
            string content = "";
            StringBuilder sb = new StringBuilder(header);
            int count = 0;
            foreach (var item in store.database)
            {
                if (count >= store.database.Count - 1)
                {
                    //end
                    sb.Append($"{item.Key}={item.Value}");
                }
                else
                {
                    sb.Append($"{item.Key}={item.Value}\n");
                    count++;
                }
                //Debug.Log(store.database.Count + " vs " + count);
            }
            content = sb.ToString();
            return content;
        }

        public static StringStorage TextToStrStorage(string text)
        {
            if (!text.StartsWith(StringStorage.HEADER))
            {
                //NOT VALID
                Debug.LogError("[StringSave] Text is not a valid StringSave data");
                return null;
            }
            //string noEnter = Utils.RemoveEnter(text); //remove enter
            string[] divided = text.Split('\n'); //split \n 
                string versionStr = divided[0]; //get firs one for versions
                string versionStrnobr = versionStr.Replace("$", ""); //remove []
                Utils.DictionaryPair ver = Utils.stringtodictionary(versionStrnobr, ':'); //strnobrackets to ver
                int version = int.Parse(ver.value); //version
                Debug.Log($"[StringSave] StringSave is parsing version ({version})");
            List<string> noVersion = divided.ToList();
            noVersion.RemoveAt(0); //remove version
                                   //Debug.Log("Noversion first : " + noVersion[0]);
            //DETERMINE VERSION
            switch (version)
            {
                case 1:
                    return V1Parser.ParseFromText(noVersion.ToArray());
                default:
                    Debug.LogError($"[StringSave] UNKNOWN VERSION OF STRINGSAVE : {version}. Trying default ({StringStorage.SAVE_VERSION})");
                    return V1Parser.ParseFromText(noVersion.ToArray()); //DEFAULT
            }
        }

        public static class V1Parser
        {
            public static StringStorage ParseFromText(string[] divided)
            {
                StringStorage ss = new StringStorage();
                for (int i = 0; i < divided.Length; i++)
                {
                    try
                    {
                        Utils.DictionaryPair dvc = Utils.stringtodictionary(divided[i]);
                        ss.Set(dvc.key, dvc.value);
                    }
                    catch
                    {
                        Debug.LogError("[StringSave] Failed parsing StringSave V1 from text");
                        throw;
                    }
                }
                return ss;
            }

            public static string ParseToText<T>(T value)
            {
                string result;
                if (Utils.TypeIsSupported<T>())
                {
                    result = value.ToString();
                }
                else if (typeof(T).IsEnum)
                {
                    result = System.Enum.GetName(typeof(T), value);
                }
                else
                {
                    try
                    {
                        string json = JsonUtility.ToJson(value);
                        result = json;
                    }
                    catch (System.Exception e)
                    {
                        throw new System.Exception("[StringStorage] An error occured while converting value to JSON. JsonUtility errror : " + e.Message);
                    }
                }

                return result;
            }

            public static string ParseArrayToText<T>(T[] arr)
            {
                if (!Utils.TypeIsSupportedArray<T>())
                {
                    throw new System.Exception("[StringStorage] Type specified is not the type of a supported array type. ");
                }
                StringBuilder sb = new StringBuilder("[");
                for (int i = 0; i < arr.Length; i++)
                {
                    sb.Append(arr[i].ToString());
                    if (i != arr.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append("]");
                return sb.ToString();
            }

            public static T[] ParseStringToArray<T>(string value, System.Func<string, T> convertFunction)
            {
                string nobrackets = value.Replace("[", "").Replace("]", "");
                string[] split = nobrackets.Split(',');
                List<T> result = new List<T>();
                try
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        //Debug.Log(split[i]);
                        result.Add(convertFunction(split[i]));
                    }
                    return result.ToArray();
                }
                catch (System.Exception e)
                {
                    throw new System.Exception($"[StringSave] String cannot be converted to {typeof(T)}[]. Exception: " + e.Message);
                }
            }
        }

        public static class Utils
        {
            /*
            public static string RemoveEnter(string s)
            {
                string removekurung = s.Replace("(", "");
                removekurung = removekurung.Replace(")", "");
                string replacement = Regex.Replace(removekurung, @"\t|\n|\r", ""); //AND TANDAKURUNG
                return replacement;
            }
            */

            public static bool TypeIsSupported<T>()
            {
                return System.Array.Exists(StringStorage.SUPPORTED_TYPES, (System.Type type) => type == typeof(T));
            }

            public static bool TypeIsSupportedArray<T>()
            {
                return System.Array.Exists(StringStorage.SUPPORTED_ARRAY_TYPES, (System.Type type) => type == typeof(T));
            }

            public static bool IsArrayString(string str)
            {
                return str.StartsWith("[") && str.EndsWith("]");
            }

            public static string ParseString(string str)
            {
                string result = str.Replace("\n", "~$n~").Replace(",", "~$c~");
                return result;
            }

            public static string ParseStringBack(string str)
            {
                string result = str.Replace("~$n~", "\n").Replace("~$c~", ",");
                return result;
            }

            public static DictionaryPair stringtodictionary(string s)
            {
                return stringtodictionary(s, '=');
            }

            public static DictionaryPair stringtodictionary(string s, char separator)
            {
                string[] sarr = s.Split(separator);
                /*
                foreach (var item in sarr)
                {
                    Debug.Log(item);
                }
                */
                return new DictionaryPair(sarr[0], sarr[1]);
            }

            public struct DictionaryPair
            {
                public string key;
                public string value;

                public DictionaryPair(string key, string value)
                {
                    this.key = key;
                    this.value = value;
                }
            }

            public static string RemoveParenthesis(string s)
            {
                string replacement = s.Replace("(", "");
                replacement = replacement.Replace(")", "");
                return replacement;
            }
        }
    }

    /// <summary>
    /// A temporary object to store values before getting converted to a type
    /// </summary>
    public class StringStorageObject
    {
        public string value;

        public StringStorageObject(string val)
        {
            this.value = val;
        }

        public int AsInt()
        {
            int i = -1;
            bool b = int.TryParse(value, out i);
            if (b)
            {
                return i;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Int");
            }
        }

        public string AsString()
        {
            return value;
        }

        public float AsFloat()
        {
            float f = -1f;
            bool b = float.TryParse(value, out f);
            if (b)
            {
                return f;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Float");
            }
        }

        public bool AsBool()
        {
            bool b;
            bool c = bool.TryParse(value, out b);
            if (c)
            {
                return b;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Bool");
            }
        }

        public Vector2 AsVector2()
        {
            string tempval = StringSaveParser.Utils.RemoveParenthesis(value);
            string[] temp = tempval.Split(',');
            float x = 0;
            if (!float.TryParse(temp[0], out x))
            {
                throw new System.Exception("[StringSave] StringStorage Vector2 data value of X cannot be converted to a float");
            }
            float y = 0;
            if (!float.TryParse(temp[1], out y))
            {
                throw new System.Exception("[StringSave] StringStorage Vector2 data value of Y cannot be converted to a float");
            }
            return new Vector2(x, y);
        }

        public Vector3 AsVector3()
        {
            string tempval = StringSaveParser.Utils.RemoveParenthesis(value);
            string[] temp = tempval.Split(',');
            float x = 0;
            if (!float.TryParse(temp[0], out x))
            {
                throw new System.Exception("[StringSave] StringStorage Vector3 data value of X cannot be converted to a float");
            }
            float y = 0;
            if (!float.TryParse(temp[1], out y))
            {
                throw new System.Exception("[StringSave] StringStorage Vector3 data value of Y cannot be converted to a float");
            }
            float z = 0;
            if (!float.TryParse(temp[2], out z))
            {
                throw new System.Exception("[StringSave] StringStorage Vector3 data value of Z cannot be converted to a float");
            }
            return new Vector3(x, y, z);
        }

        public Quaternion AsQuaternion()
        {
            string tempval = StringSaveParser.Utils.RemoveParenthesis(value);
            string[] temp = tempval.Split(',');
            float x = 0;
            if (!float.TryParse(temp[0], out x))
            {
                throw new System.Exception("[StringSave] StringStorage Quaternion data value of X cannot be converted to a float");
            }
            float y = 0;
            if (!float.TryParse(temp[1], out y))
            {
                throw new System.Exception("[StringSave] StringStorage Quaternion data value of Y cannot be converted to a float");
            }
            float z = 0;
            if (!float.TryParse(temp[2], out z))
            {
                throw new System.Exception("[StringSave] StringStorage Quaternion data value of Z cannot be converted to a float");
            }
            float w = 0;
            if (!float.TryParse(temp[3], out w))
            {
                throw new System.Exception("[StringSave] StringStorage Quaternion data value of W cannot be converted to a float");
            }
            return new Quaternion(x, y, z, w);
        }

        public long AsLong()
        {
            long l = -1;
            bool b = long.TryParse(value, out l);
            if (b)
            {
                return l;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Long");
            }
        }

        public short AsShort()
        {
            short s = -1;
            bool b = short.TryParse(value, out s);
            if (b)
            {
                return s;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Short");
            }
        }

        public double AsDouble()
        {
            double d = -1;
            bool b = double.TryParse(value, out d);
            if (b)
            {
                return d;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Double");
            }
        }

        public decimal AsDecimal()
        {
            decimal d = -1;
            bool b = decimal.TryParse(value, out d);
            if (b)
            {
                return d;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Double");
            }
        }

        public char AsChar()
        {
            char c = '\0';
            bool b = char.TryParse(value, out c);
            if (b)
            {
                return c;
            }
            else
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Char");
            }
        }

        /// <summary>
        /// Parses value as JSON of a class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AsTypeJSON<T>()
        {
            T obj;
            try
            {
                obj = JsonUtility.FromJson<T>(value);
                return obj;
            }
            catch (System.Exception e)
            {
                throw new System.Exception($"[StringSave] StringStorage JSONType data cannot be converted to {typeof(T)}. Exception : {e.Message}");
            }
            throw new System.Exception($"[StringSave] StringStorage JSONType data cannot be converted to {typeof(T)}. Some sort of bug must've happened, the try catch didn't work..");
        }

        /// <summary>
        /// Parses value as Enum
        /// </summary>
        /// <typeparam name="TEnum">Type of enum</typeparam>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public TEnum AsEnum<TEnum>()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new System.Exception($"[StringSave] {typeof(TEnum)} is not an enum type.");
            }

            TEnum en;

            try
            {
                en = (TEnum)System.Enum.Parse(typeof(TEnum), value);
                return en;
            }
            catch (System.Exception e)
            {
                throw new System.Exception($"[StringSave] StringStorage Enum data cannot be converted to {typeof(TEnum)}. Exception : {e.Message}");
            }
        }

        public int[] AsIntArray()
        {
            if (!StringSaveParser.Utils.IsArrayString(value))
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Int[] because value is not an array.");
            }
            return _SSP.V1Parser.ParseStringToArray<int>(value, (str) => int.Parse(str));
        }

        public byte[] AsByteArray()
        {
            if (!StringSaveParser.Utils.IsArrayString(value))
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Byte[] because value is not an array.");
            }
            return _SSP.V1Parser.ParseStringToArray(value, (str) => byte.Parse(str));
        }

        public string[] AsStringArray()
        {
            if (!StringSaveParser.Utils.IsArrayString(value))
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to String[] because value is not an array.");
            }
            return _SSP.V1Parser.ParseStringToArray(value, (str) => str);
        }

        public bool[] AsBoolArray()
        {
            if (!StringSaveParser.Utils.IsArrayString(value))
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Bool[] because value is not an array.");
            }
            return _SSP.V1Parser.ParseStringToArray(value, (str) => System.Convert.ToBoolean(str));
        }

        public float[] AsFloatArray()
        {
            if (!StringSaveParser.Utils.IsArrayString(value))
            {
                throw new System.Exception("[StringSave] StringStorage data cannot be converted to Float[] because value is not an array.");
            }
            return _SSP.V1Parser.ParseStringToArray(value, (str) => float.Parse(str));
        }

        public override string ToString()
        {
            return value;
        }
    }
}