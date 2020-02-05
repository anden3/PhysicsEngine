using UnityEngine;

using System.IO;
using System.Text;
using System.Collections.Generic;

public class Recorder : MonoBehaviour
{
    public string folderName = "Assets/Resources";

    private static string folderPath;
    private static Recorder instance;

    private static Dictionary<string, FileStream> files = new Dictionary<string, FileStream>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        folderPath = folderName;
    }

    public static void LogData(string file, params object[] data)
    {
        if (instance == null)
            return;

        if (!files.ContainsKey(file))
        {
            files.Add(file, File.Open(
                $"{folderPath}/{file}.txt",
                FileMode.Create, FileAccess.Write
            ));
        }

        byte[] bytes = new UTF8Encoding(true).GetBytes(
            $"{string.Join("\t", data)}\n"
        );

        files[file].Write(bytes, 0, bytes.Length);
    }

    private void OnDestroy()
    {
        foreach (FileStream fs in files.Values)
        {
            fs.Dispose();
        }
    }
}