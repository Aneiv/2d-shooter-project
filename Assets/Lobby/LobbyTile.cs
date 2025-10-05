using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _ipText;
    [SerializeField] private TextMeshProUGUI _playerCountText;
    [SerializeField] private Image _connectionDotImg;
    [SerializeField] private Button joinBtn;
    private GameObject _lobbyMenu;
    private GameObject _clientMenu;

    public void Initialize(GameObject clientMenu, GameObject lobbyMenu)
    {
        _clientMenu = clientMenu;
        _lobbyMenu = lobbyMenu;
    }
    public void SetText(string username, string ipAdress, int currentPlayers, int maxPlayers, bool isOpen)
    {
        _usernameText.text = username;
        _ipText.text = ipAdress;
        _playerCountText.text = currentPlayers.ToString() + " / " + maxPlayers.ToString();

        _connectionDotImg.color = isOpen ? Color.green : Color.red;
        joinBtn.gameObject.SetActive(isOpen);
    }

    public void JoinLobby()
    {
        Debug.Log("Joing to: " + _ipText.text);
        if (_lobbyMenu != null && _clientMenu != null)
        {
            _lobbyMenu.SetActive(false);
            _clientMenu.SetActive(true);

            // auto-writes ip adress of host
            ClientMenu clientMenuScript = _clientMenu.GetComponent<ClientMenu>();
            if (clientMenuScript != null)
            {
                clientMenuScript.SetInputIpAdress(_ipText.text);
            }

        }
    }
}
