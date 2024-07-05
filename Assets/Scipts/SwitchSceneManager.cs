using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

public class SwitchSceneManager : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        // Debug.Log("SwitchSceneManager script attached and running.\n Target scene: " + sceneName);
    }
    
    public void SceneSwitchTo()
    {
        Debug.Log("Button clicked, attempting to switch to scene: " + sceneName);
        // Debug.Log("Switching to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
