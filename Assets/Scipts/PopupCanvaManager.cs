using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupCanvaManager : MonoBehaviour
{
    public GameObject popupCanvas;

    public void ShowPopupCanvas()
    {
        Debug.Log("Open Popup Canvas");
        popupCanvas.SetActive(true);
    }

    public void ClosePopupCanvas()
    {
        Debug.Log("Close Popup Canvas");
        popupCanvas.SetActive(false);
    }
}
