using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class SwitchSceneManager : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
        }
    }
    void OnDestroy()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEnter);
        }
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log("Button clicked, attempting to switch to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    

    // 2D UI interaction
    public void SceneSwitchTo()
    {
        Debug.Log("Button clicked, attempting to switch to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
