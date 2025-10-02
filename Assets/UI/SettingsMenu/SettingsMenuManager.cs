using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    public Toggle inGameConsoleToggle;

    private SettingsData settings;
    private string filePath;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "settings.json");
        LoadSettings();
    }

    private void OnEnable()
    {
        settings = GameSettings.Instance.GetSettings();
        
        //start state
        inGameConsoleToggle.isOn = settings.inGameConsoleEnable;
        //listeners
        inGameConsoleToggle.onValueChanged.AddListener(OnInGameConsoleChanged);
    }
    private void OnDisable()
    {
        inGameConsoleToggle.onValueChanged.RemoveListener(OnInGameConsoleChanged);
    }
    public void LoadSettings()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            settings = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            settings = new SettingsData { };
        }
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(Application.persistentDataPath + "/settings.json", json);
    }

    private void OnInGameConsoleChanged(bool value)
    {
        //Debug.Log("SavedChanges");
        settings.inGameConsoleEnable = value;
        SaveSettings();
    }
}
