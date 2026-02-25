using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReader : MonoBehaviour
{
    public static readonly string rootPath = $"{Application.dataPath}/Game Data/";

    //Read from file path outside resources
    public static List<string> ReadFile(string path, bool includeBlankLines = false)
    {
        if(path.StartsWith('/'))
        {
            path = rootPath+path;
        }

        List<string> lines = new List<string>();

        try
        {
            using(StreamReader sr = new StreamReader(path))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if(includeBlankLines || !string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line);
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"File reading error: {ex.Message}");
        }

        return lines;
    }

    //Read from Resource Asset Name
    public static List<string> ReadAsset(string path, bool includeBlankLines = false)
    {
        TextAsset asset = Resources.Load<TextAsset>(path);

        if(asset == null)
        {
            return null;
        }

        return ReadAsset(asset, includeBlankLines);
    }

    //Read from Resource Asset
    public static List<string> ReadAsset(TextAsset text, bool includeBlankLines = false)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(text.text))
        {
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }
        }
        return lines;
    }
}
