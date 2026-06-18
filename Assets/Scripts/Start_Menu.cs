using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;


public class Start_Menu : MonoBehaviour
{

    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("Main_Level");
    }

    public void OnExitClick()
    {
        Debug.Log("Exit button clicked!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Only works in the editor
#else
        Application.Quit(); // Works in builds
#endif

    }

}
