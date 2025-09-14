using UnityEngine;

public class GameDefaultSettings : MonoBehaviour
{
    public bool isSinglePlayerMode = true; //may be in use later
    [HideInInspector] public int highscore;
    public static GameDefaultSettings Instance { get; private set; }
    private void Awake()
    {
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
}
