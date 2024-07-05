using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class SwitchSceneManager : MonoBehaviour
{
    public string sceneName;

    // 2D UI interaction
    public void SceneSwitchTo()
    {
        Debug.Log("SceneSwitchTo: Button clicked, attempting to switch to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
