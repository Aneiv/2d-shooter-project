using TMPro;
using UnityEngine;

public class LobbyTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _ipText;
    private GameObject _lobbyMenu;
    private GameObject _clientMenu;

    public void Initialize(GameObject clientMenu, GameObject lobbyMenu)
    {
        _clientMenu = clientMenu;
        _lobbyMenu = lobbyMenu;
    }
    public void SetText(string username, string ipAdress)
    {
        _usernameText.text = username;
        _ipText.text = ipAdress;
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
