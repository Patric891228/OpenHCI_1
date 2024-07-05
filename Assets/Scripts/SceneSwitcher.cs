using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // 切換到指定場景
    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

// Enter_Scene
// Main_Scene
// SituationChoose_Scene
// IntervienIntro_Scene
// ReportIntro_Scene
// SceneCreation_Scene
// Simulation_Scene
// Result_Scene
// Setting_Scene