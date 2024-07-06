using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class SwitchSceneManager : MonoBehaviour
{
    public string sceneName;

    // Triggered by button
    public void SceneSwitchTo()
    {
        Debug.Log("SceneSwitchTo: Button clicked, attempting to switch to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
    
    // Triggered by animation
    public void SceneSwitchToByAnimation(string scene)
    {
        Debug.Log("SceneSwitchTo: Button clicked, attempting to switch to scene: " + scene);
        SceneManager.LoadScene(scene);
    }
}
