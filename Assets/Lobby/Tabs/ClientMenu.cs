using TMPro;
using UnityEngine;

public class ClientMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInput;

    public void SetInputIpAdress(string ipAdress)
    {
        ipInput.text = ipAdress;
    }
}
