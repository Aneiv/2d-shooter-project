
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LostConnectionHandler : MonoBehaviour
{
    public static LostConnectionHandler Instance { get; private set; }

    GameObject mainMenuView;
    GameObject clientMenuView;
    GameObject lostConnectionView;

    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUIReferences(CanvasReferences ui)
    {
        mainMenuView = ui.mainMenuView;
        clientMenuView = ui.clientMenuView;
        lostConnectionView = ui.lostConnectionView;
    }

    IEnumerator SetViewCoroutine()
    {
        yield return null;

        mainMenuView.SetActive(false);
        clientMenuView.SetActive(false);
        lostConnectionView.SetActive(true);
    }
    public void SetView()
    {
        StartCoroutine(SetViewCoroutine());
    }
}

