using UnityEngine;

public class GameDefaultSettings : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);//dont destroy after scene change
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;//set framerate to refresh rate
    }
}
