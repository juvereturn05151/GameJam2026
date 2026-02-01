using TMPro; // optional (remove if you don't use TMP)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject root;     // panel root
    [SerializeField] private Button restartButton;

    [Header("Optional Text")]
    [SerializeField] private TMP_Text messageText; // optional
    [SerializeField] private string gameOverMessage = "Game Over";
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] GameObject youWin;

    void Awake()
    {
        //if (restartButton)
        //    restartButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(bool isLiveZero, int score)
    {
        if (isLiveZero)
        {
            if (root) root.SetActive(true);
        }
        else 
        {
            if (youWin) youWin.SetActive(true);
        }

        scoreText.text = "Final Score: " + score.ToString();

        if (messageText)
            messageText.text = gameOverMessage;
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
    }

    public void OnClickRestart()
    {
        // Call start game
        FadingUI.Instance.StartFadeIn();
        FadingUI.Instance.OnStopFading.AddListener(LoadTargetScene);
    }

    private void LoadTargetScene()
    {
        SceneManager.LoadScene("Day2GameplayScene");
    }
}
