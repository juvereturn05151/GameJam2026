using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] 
    private RectTransform playArea;
    [SerializeField] 
    private HeadIcon headPrefab;
    [SerializeField] 
    private Text scoreText;

    [Header("Sprites")]
    [SerializeField] 
    private Sprite luigiSprite;
    [SerializeField] 
    private Sprite[] decoySprites;

    [Header("Difficulty")]
    [SerializeField] 
    private int iconCount = 25;
    [SerializeField] 
    private float minSpeed = 120f;
    [SerializeField] 
    private float maxSpeed = 240f;
    [SerializeField] 
    private int addPerWin = 3;

    [Header("Particles (Optional)")]
    [SerializeField] // null if Screen Space Overlay
    private Camera uiCamera;        
    [SerializeField] // usually Camera.main
    private Camera worldCamera;     
    [SerializeField] 
    private ParticleSystem correctFxPrefab;
    [SerializeField] // distance in front of worldCamera
    private float fxZ = 10f;        

    private readonly List<HeadIcon> _icons = new();
    private int _score = 0;

    void Start()
    {
        StartRound();
        UpdateUI();
    }

    public void StartRound()
    {
        ClearIcons();

        int targetIndex = Random.Range(0, iconCount);

        for (int i = 0; i < iconCount; i++)
        {
            var icon = Instantiate(headPrefab, playArea);

            bool isTarget = (i == targetIndex);
            Sprite sprite = isTarget ? luigiSprite : decoySprites[Random.Range(0, decoySprites.Length)];

            // random position
            icon.GetComponent<RectTransform>().anchoredPosition = RandomPointIn(playArea.rect);

            // random velocity
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(minSpeed, maxSpeed);
            Vector2 vel = dir * speed;

            icon.Init(this, playArea, sprite, isTarget, vel);
            _icons.Add(icon);
        }
    }

    public void OnHeadClicked(HeadIcon clicked)
    {
        if (clicked.IsTarget)
        {
            _score++;
            PlayCorrectFx(clicked.Rect);

            // ramp difficulty
            iconCount = Mathf.Min(iconCount + addPerWin, 200);
            minSpeed += 10f;
            maxSpeed += 15f;

            StartRound();
        }
        else
        {
            _score = Mathf.Max(0, _score - 1);
        }

        UpdateUI();
    }

    void PlayCorrectFx(RectTransform targetUI)
    {
        if (correctFxPrefab == null || worldCamera == null) return;

        // convert UI position -> screen -> world
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, targetUI.position);
        Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, fxZ));

        var fx = Instantiate(correctFxPrefab, worldPos, Quaternion.identity);
        fx.Play();
        Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
    }

    void ClearIcons()
    {
        foreach (var icon in _icons)
            if (icon != null) Destroy(icon.gameObject);
        _icons.Clear();
    }

    Vector2 RandomPointIn(Rect rect)
    {
        return new Vector2(
            Random.Range(rect.xMin, rect.xMax),
            Random.Range(rect.yMin, rect.yMax)
        );
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {_score}";
    }
}
