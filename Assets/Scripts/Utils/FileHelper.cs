using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileHelper
{
    public static T ReadData<T>(string path) where T : BaseObject
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        return null;
    }

    public static string LoadResourceTextFile(string path, bool isReadFromResource)
    {
        string result = "";

        if (isReadFromResource)
        {
            TextAsset targetFile = Resources.Load<TextAsset>(path);
            result = targetFile.text;
        }
        else
        {
            result = ReadText(path + ".txt");
        }

      
        return result;
    }

    public static string ReadText(string path)
    {
        if (File.Exists(path))
        {
            string result = File.ReadAllText(path);
            return result;
        }

        return "";
    }


}

public class BaseObject
{
    public BaseObject()
    {
        
    }
}
