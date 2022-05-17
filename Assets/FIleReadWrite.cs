using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReadWrite
{
    //NOTE : PATH MUST ADD "/" first

    #region OLDCODE
    /*
    public static bool WriteFile(string path, string text)
    {
        try
        {
            StreamWriter sw = new StreamWriter(path, true);
            sw.Write(text);
            sw.Close();
            Debug.Log("Saved File " + path);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed saving file : " + path + " | ERROR : " + e.Message);
            return false;
        }
    }

    public static string ReadFile(string path)
    {
        try
        {
            StreamReader sr = new StreamReader(path);
            string s = sr.ReadToEnd();
            return s;
        }
        catch (System.Exception)
        {
            Debug.LogError("Failed reading file : " + path);
            throw;
        }
    }
    */
    #endregion

    public static bool WriteFile(string path, string text)
    {
        try
        {
            string nofile = DirectoryWithoutFile(path);
            if (!Directory.Exists(nofile))
            {
                Debug.LogWarning("[FileReadWrite] Directory of path doesn't exist. Trying to create it");
                Directory.CreateDirectory(nofile);
                Debug.Log("[FileReadWrite] Directory created : " + nofile);
            }
            StreamWriter sw = new StreamWriter(path, false);
            sw.Write(text);
            sw.Close();
            Debug.Log("[FileReadWrite] Saved File " + path);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[FileReadWrite] Failed saving file : " + path);
            Debug.LogError("[FIleReadWrite] " + e.Message);
            return false;
        }
    }

    public static string ReadFile(string path)
    {
        try
        {
            return InternalReadFile(path);
        }
        catch (System.Exception)
        {
            Debug.LogError("[FileReadWrite] Failed reading file : " + path);
            throw;
        }
    }

    public static bool TryReadFile(string path, out string result)
    {
        try
        {
            result = InternalReadFile(path);
            return true;
        }
        catch (System.Exception)
        {
            Debug.LogError("[FileReadWrite] Failed reading file : " + path);
            result = "";
            return false;
        }
    }

    private static string InternalReadFile(string path)
    {
        StreamReader sr = new StreamReader(path);
        string s = sr.ReadToEnd();
        return s;
    }

    public static bool WriteFilePersistentDataPath(string extraPath, string text)
    {
        return WriteFile(Application.persistentDataPath + SlashCheck(extraPath), text);
    }

    public static string ReadFilePersistentDataPath(string extraPath)
    {
        return ReadFile(Application.persistentDataPath + SlashCheck(extraPath));
    }

    public static bool TryReadFilePersistentDataPath(string extraPath, out string result)
    {
        return TryReadFile(Application.persistentDataPath + SlashCheck(extraPath), out result);
    }

    public static bool WriteFileStreamingAssets(string extraPath, string text)
    {
        return WriteFile(Application.streamingAssetsPath + SlashCheck(extraPath), text);
    }

    public static string ReadFileStreamingAssets(string extraPath)
    {
        return ReadFile(Application.streamingAssetsPath + SlashCheck(extraPath));
    }

    public static bool ReadFileStreamingAssets(string extraPath, out string result)
    {
        return TryReadFile(Application.streamingAssetsPath + SlashCheck(extraPath), out result);
    }

    private static string SlashCheck(string s)
    {
        string s1 = s;
        if (!s.StartsWith("/")) //if doesnt start with /
        {
            s1 = s1.Insert(0, "/");
        }
        return s1;
    }

    private static string DirectoryWithoutFile(string path)
    {
        string[] strlist = path.Split('/');
        string news = path.Replace(strlist[strlist.Length - 1], "");
        return news;
    }
}
