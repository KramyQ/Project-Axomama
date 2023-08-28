using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class HostBtn : MonoBehaviour
{
    public Button yourButton;
    [SerializeField] private Canvas mainmenu;

    [SerializeField]
    private Camera mainCam;
    [SerializeField]
    private GameObject console;

    void Start () {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        console.SetActive(true);
        mainCam.enabled = false;
        mainmenu.enabled = false;
        NetworkManager.Singleton.StartHost();
    }
}
