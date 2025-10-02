using System.IO;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private SettingsData settings;
    private string filePath;

    public bool isSinglePlayerMode = true; //may be in use later
    [HideInInspector] public int highscore;
    public static GameSettings Instance { get; private set; }
    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "settings.json");
        LoadSettings();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);//dont destroy after scene change
    }
    void Start()
    {
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;//set framerate to refresh rate
        //Highscore load from PlayerPrefs
        highscore = PlayerPrefs.GetInt("highscore", 0);

    }
    private void LoadSettings()
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

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(Application.persistentDataPath + "/settings.json", json);
    }

    public SettingsData GetSettings()
    {
        return settings;
    }
}
//-- Settings --
[System.Serializable]
public class SettingsData
{
    public bool inGameConsoleEnable = false;
}
