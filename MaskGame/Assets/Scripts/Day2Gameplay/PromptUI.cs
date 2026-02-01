using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptUI : MonoBehaviour
{
    [SerializeField] private Image promptMaskImage;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TextMeshProUGUI partyTimerText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private GameObject flashWrongUI;

    public void ShowPrompt(MaskData mask)
    {
        if (promptMaskImage) promptMaskImage.sprite = mask.maskSprite;

        flashWrongUI.SetActive(false);

        if (promptText)
        {
            if (!string.IsNullOrEmpty(mask.description))
                promptText.text = $"Find: {mask.maskName}\nHint: {mask.description}";
            else
                promptText.text = $"Find: {mask.maskName}";
        }
    }

    public void SetPartyTimer(float secondsLeft)
    {
        if (partyTimerText) partyTimerText.text = $"Party Time: {Mathf.CeilToInt(secondsLeft)}";
    }

    public void SetTimer(float secondsLeft)
    {
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(secondsLeft)}";
    }

    public void SetScore(int score)
    {
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    // Optional feedback hooks
    public void StopFlashWrong() { flashWrongUI.SetActive(false); }
    public void FlashWrong() { flashWrongUI.SetActive(true); }
    public void FlashFail() { }

    public void SetLives(int lives) { livesText.text = lives.ToString(); }

}
