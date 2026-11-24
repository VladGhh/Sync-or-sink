using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstLevel = "TestLevel";  // scena care porneste cand apesi play

    public void Play()
    {
        SceneManager.LoadScene(firstLevel);
    }

    // in build inchide jocul iar in editor opreste play mode
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
