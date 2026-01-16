using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private const string SaveFileName = "save.json";

    private PlayerProgress progress;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        progress = GetComponent<PlayerProgress>();
        Load();
    }

    public void Save()
    {
        if (progress == null) return;

        try
        {
            string json = JsonUtility.ToJson(progress.Data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Save failed: {ex.Message}");
        }
    }

    public void Load()
    {
        if (progress == null) return;

        try
        {
            if (!File.Exists(SavePath)) return;
            string json = File.ReadAllText(SavePath);
            PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json);
            progress.LoadFrom(data);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Load failed: {ex.Message}");
        }
    }
}

