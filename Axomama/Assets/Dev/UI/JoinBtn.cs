using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class JoinBtn : MonoBehaviour
{
    public Button yourButton;
    [SerializeField] private Canvas mainmenu;
    [SerializeField] private TextMeshProUGUI ipAdressBox;

    void Start () {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        // NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
        //     ipAdressBox.text,  // The IP address is a string
        //     (ushort)7777 // The port number is an unsigned short
        // );
        mainmenu.enabled = false;
        NetworkManager.Singleton.StartClient();
    }
}
