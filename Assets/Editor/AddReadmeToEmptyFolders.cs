using UnityEngine;
using UnityEditor;
using System.IO;

public class ReadmeUtility : MonoBehaviour
{
    private const string readmeFileName = "README.txt";
    private const string readmeContent =
@"This folder is currently empty.
This README.txt is used to ensure the folder is included in version control systems
and Unity package exports. You can safely delete it when the folder has content.";

    [MenuItem("Tools/Export Helper/Add README.txt to Empty Folders")]
    public static void AddToEmptyFolders()
    {
        string rootPath = Application.dataPath;
        int added = 0;

        foreach (string dir in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
        {
            string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            bool onlyMetaOrNone = true;

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension != ".meta" && !file.EndsWith(readmeFileName))
                {
                    onlyMetaOrNone = false;
                    break;
                }
            }

            if (onlyMetaOrNone)
            {
                string readmePath = Path.Combine(dir, readmeFileName);
                if (!File.Exists(readmePath))
                {
                    File.WriteAllText(readmePath, readmeContent);
                    added++;
                }
            }
        }

        Debug.Log($"📁 Boş klasörlerde {added} adet README.txt dosyası oluşturuldu.");
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Export Helper/Force Add README.txt to All Folders")]
    public static void ForceAddToAllFolders()
    {
        string rootPath = Application.dataPath;
        int added = 0;

        foreach (string dir in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
        {
            string readmePath = Path.Combine(dir, readmeFileName);
            if (!File.Exists(readmePath))
            {
                File.WriteAllText(readmePath, readmeContent);
                added++;
            }
        }

        Debug.Log($"💥 Tüm klasörlerde {added} adet README.txt dosyası zorla eklendi.");
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Export Helper/Delete All README.txt Files")]
    public static void DeleteAllReadmes()
    {
        string rootPath = Application.dataPath;
        int deleted = 0;

        foreach (string dir in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
        {
            string readmePath = Path.Combine(dir, readmeFileName);
            if (File.Exists(readmePath))
            {
                File.Delete(readmePath);
                deleted++;
            }
        }

        Debug.Log($"🗑️ Toplamda {deleted} adet README.txt dosyası silindi.");
        AssetDatabase.Refresh();
    }
}
