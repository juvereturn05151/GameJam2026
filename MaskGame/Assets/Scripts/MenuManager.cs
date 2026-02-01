using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Day2GameplayScene";

    private string targetSceneName;

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickStart()
    {
        // Set target scene to the tutorial scene
        targetSceneName = gameSceneName;

        // Call start game
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTargetScene);
    }

    private void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
