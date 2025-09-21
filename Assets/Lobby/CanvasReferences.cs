using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasReferences : MonoBehaviour
{
    public TMP_InputField ipInput;
    public TMP_InputField clientNameInput;
    public TMP_InputField hostNameInput;
    public TextMeshProUGUI hostIpText;
    public TextMeshProUGUI backlogTextHost;
    public TextMeshProUGUI backlogTextClient;

    public Button hostButton;
    public Button joinButton;

    private void Start()
    {
        LanLobbyManager.Instance.SetUIReferences(this);
        RefreshBtnReferences();
    }

    private void RefreshBtnReferences()
    {
        if (hostButton != null)
        {
            hostButton.onClick.RemoveAllListeners();
            hostButton.onClick.AddListener(LanLobbyManager.Instance.StartHost);
        }

        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(LanLobbyManager.Instance.JoinGame);
        }
    }
}
