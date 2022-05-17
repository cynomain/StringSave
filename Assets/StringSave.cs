using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace cynomain.StringSave
{

    [System.Serializable]
    public class StringStorage
    {
        public const string HEADER = "[STRSAVEVER";
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
            if (System.Array.Exists(supportedTypes, (System.Type type) => type == value.GetType()))
            {
                if (!KeyExists(key))
                {
                    database.Add(key, value.ToString());
                }
                else
                {
                    database[key] = value.ToString();
                }
            }
            else
            {
                try
                {
                    string json = JsonUtility.ToJson(value);
                    if (!database.ContainsKey(key))
                    {
                        database.Add(key, json);
                    }
                    else
                    {
                        database[key] = json;
                    }
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("StringStorage : An error occured on Set() while converting value to JSON. JsonUtility errror : " + e.Message);
                }
            }
        }

        public StringStorageObject Get(string key)
        {
            if (!KeyExists(key))
            {
                Debug.LogError("KEY NOT FOUND IN STRINGSAVE : " + key);
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

        public static System.Type[] supportedTypes = new System.Type[] { typeof(int), typeof(float), typeof(bool), typeof(Vector2), typeof(Vector3), typeof(long), typeof(short), typeof(string), typeof(double), typeof(decimal) };

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
            string header = $"[STRSAVEVER={StringStorage.SAVE_VERSION}]\n";
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
                Debug.LogError("Text is not a valid StringSave data");
                return null;
            }
            //string noEnter = Utils.RemoveEnter(text); //remove enter
            string[] divided = text.Split('\n'); //split \n 
                string versionStr = divided[0]; //get firs one for versions
                string versionStrnobr = versionStr.Replace("[", "").Replace("]", ""); //remove []
                Utils.DictionaryValueContainer ver = Utils.stringtodictionary(versionStrnobr); //strnobrackets to ver
                int version = int.Parse(ver.value); //version
                Debug.Log("StringSave is parsing version " + version);
            List<string> noVersion = divided.ToList();
            noVersion.RemoveAt(0); //remove version
                                   //Debug.Log("Noversion first : " + noVersion[0]);
            //DETERMINE VERSION
            switch (version)
            {
                case 1:
                    return StringSaveV1Parser.ParseFromText(noVersion.ToArray());
                default:
                    Debug.LogError($"UNKNOWN VERSION OF STRINGSAVE : {version}. Trying default ({StringStorage.SAVE_VERSION}");
                    return StringSaveV1Parser.ParseFromText(noVersion.ToArray()); //DEFAULT
            }
        }

        public class StringSaveV1Parser
        {
            public static StringStorage ParseFromText(string[] divided)
            {
                StringStorage ss = new StringStorage();
                for (int i = 0; i < divided.Length; i++)
                {
                    try
                    {
                        Utils.DictionaryValueContainer dvc = Utils.stringtodictionary(divided[i]);
                        ss.Set(dvc.key, dvc.value);
                    }
                    catch
                    {
                        Debug.LogError("Failed parsing StringSave V1 from text");
                        throw;
                    }
                }
                return ss;
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

            public static DictionaryValueContainer stringtodictionary(string s)
            {
                string[] sarr = s.Split('=');
                /*
                foreach (var item in sarr)
                {
                    Debug.Log(item);
                }
                */
                return new DictionaryValueContainer(sarr[0], sarr[1]);
            }

            public struct DictionaryValueContainer
            {
                public string key;
                public string value;

                public DictionaryValueContainer(string key, string value)
                {
                    this.key = key;
                    this.value = value;
                }
            }

            public static string RemoveTandaKurung(string s)
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
                throw new System.Exception("StringStorage data cannot be converted to Int");
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
                throw new System.Exception("StringStorage data cannot be converted to Float");
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
                throw new System.Exception("StringStorage data cannot be converted to Bool");
            }
        }

        public Vector2 AsVector2()
        {
            string tempval = StringSaveParser.Utils.RemoveTandaKurung(value);
            string[] temp = tempval.Split(',');
            float x = 0;
            if (!float.TryParse(temp[0], out x))
            {
                throw new System.Exception("StringStorage Vector2 data value of X cannot be converted to a float");
            }
            float y = 0;
            if (!float.TryParse(temp[1], out y))
            {
                throw new System.Exception("StringStorage Vector2 data value of Y cannot be converted to a float");
            }
            return new Vector2(x, y);
        }

        public Vector3 AsVector3()
        {
            string tempval = StringSaveParser.Utils.RemoveTandaKurung(value);
            string[] temp = tempval.Split(',');
            float x = 0;
            if (!float.TryParse(temp[0], out x))
            {
                throw new System.Exception("StringStorage Vector3 data value of X cannot be converted to a float");
            }
            float y = 0;
            if (!float.TryParse(temp[1], out y))
            {
                throw new System.Exception("StringStorage Vector3 data value of Y cannot be converted to a float");
            }
            float z = 0;
            if (!float.TryParse(temp[2], out z))
            {
                throw new System.Exception("StringStorage Vector3 data value of Z cannot be converted to a float");
            }
            return new Vector3(x, y, z);
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
                throw new System.Exception("StringStorage data cannot be converted to Long");
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
                throw new System.Exception("StringStorage data cannot be converted to Short");
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
                throw new System.Exception("StringStorage data cannot be converted to Double");
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
                throw new System.Exception("StringStorage data cannot be converted to Double");
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
                throw new System.Exception($"StringStorage JSONType data cannot be converted to {typeof(T)}. Exception : {e.Message}");
            }
            throw new System.Exception($"StringStorage JSONType data cannot be converted to {typeof(T)}. Some sort of bug must've happened, the try catch didn't work.");
        }

        public override string ToString()
        {
            return value;
        }
    }
}