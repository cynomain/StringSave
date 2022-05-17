using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cynomain.StringSave;

public class Debugsss : MonoBehaviour
{
    public StringStorage ss = new StringStorage();
    public StringStorage ssNew = new StringStorage();

    // Start is called before the first frame update
    void Start()
    {
        /*
        StrSaveTest();
        WriteTest();
        Debug.LogError("TEST ERROR");
        Debug.LogWarning("TEST WARN");
        Debug.LogException(new Exception("Test exception"));
        */
        StringStorage ss11 = new StringStorage();
        //ss11.Set("testv1", 10);
        ss11.Set("testv2", 20);
        ss11.Set("testv3", 30);
        Debug.Log(ss11.PrioritizeGet("testv3", "testv2", "testv1")); 
    }

    void StrSaveTest()
    {
        int i = 12121;
        ss.Set("testint", i);
        string str = "strerere";
        ss.Set("teststring", str);
        float f = 1.23243434545f;
        ss.Set("testfloat", f);
        long l = 1210291029L;
        ss.Set("testlong", l);
        bool b = true;
        ss.Set("testbool", b);
        Vector2 v2 = new Vector2(2f, 3f);
        ss.Set("testvec2", v2);
        Vector3 v3 = new Vector3(4f, 5f, 6f);
        ss.Set("testvec3", v3);
        short s = 2;
        ss.Set("testshort", s);
        TestClass tc = new TestClass("namaaa", 6, 13);
        ss.Set("testclass", tc);
        ss.Set("testend", "END");
        string parsedtoString = StringSaveParser.StrStorageToText(ss);
        Debug.Log(parsedtoString);
        Debug.Log("---BEGIN PARSING BACK");
        ssNew = StringSaveParser.TextToStrStorage(parsedtoString);
        foreach (var item in ssNew.database)
        {
            Debug.Log(item.Key + ":" + item.Value);
        }
        Debug.Log("---BEGIN CONVERT TEST");
        Debug.Log(ssNew["testint"].AsInt());
        Debug.Log(ssNew["teststring"].AsString());
        Debug.Log(ssNew["testfloat"].AsFloat());
        Debug.Log(ssNew["testlong"].AsLong());
        Debug.Log(ssNew["testbool"].AsBool());
        Debug.Log(ssNew["testvec2"].AsVector2());
        Debug.Log(ssNew["testvec3"].AsVector3());
        Debug.Log(ssNew["testshort"].AsShort());
        Debug.Log(ssNew["testclass"].AsTypeJSON<TestClass>());
        Debug.Log(ssNew["testend"].AsString());
    }

    void WriteTest()
    {
        string extrPath = "/StrSave/strsave1.ss";
        Debug.Log("---WRITE TEST");
        FileReadWrite.WriteFilePersistentDataPath(extrPath, ss.ToString());
    }

    void ReadTest()
    {
        string extrPath = "/StrSave/strsave1.ss";
        Debug.Log("---READ TEST");
        string read = FileReadWrite.ReadFilePersistentDataPath(extrPath);
        Debug.Log(read);
        Debug.Log("---BEGIN PARSING FILE");
        StringStorage ssFromFile = new StringStorage(read);
        foreach (var item in ssNew.database)
        {
            Debug.Log(item.Key + ":" + item.Value);
        }
        Debug.Log(ssFromFile["testint"].AsInt());
        Debug.Log(ssFromFile["teststring"].AsString());
        Debug.Log(ssFromFile["testfloat"].AsFloat());
        Debug.Log(ssFromFile["testlong"].AsLong());
        Debug.Log(ssFromFile["testbool"].AsBool());
        Debug.Log(ssFromFile["testvec2"].AsVector2());
        Debug.Log(ssFromFile["testvec3"].AsVector3());
        Debug.Log(ssFromFile["testshort"].AsShort());
        Debug.Log(ssFromFile["testclass"].AsTypeJSON<TestClass>());
        Debug.Log(ssFromFile["testend"].AsString());
    }

    public class TestClass
    {
        public string name;
        public int classnum;
        public int absen;

        public TestClass(string n, int c, int a)
        {
            name = n;
            classnum = c;
            absen = a;
        }

        public override string ToString()
        {
            return $"nama : {name} | kelas {classnum} | absen {absen}";
        }
    }
}
