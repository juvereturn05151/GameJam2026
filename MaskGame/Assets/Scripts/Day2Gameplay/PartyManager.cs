using UnityEngine;
using System.Collections.Generic;

public class PartyManager : MonoBehaviour
{
    [Header("Prefabs & Content")]
    [SerializeField] private PartyPerson personPrefab;
    [SerializeField] private Sprite[] bodySprites;
    [SerializeField] private MaskData[] masks;

    [Header("World Bounds (spawn + clamp)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-7, -4);
    [SerializeField] private Vector2 worldMax = new Vector2(7, 4);

    [Header("World Bounds (walkable)")]
    [SerializeField] private Vector2 worldWalkableMin = new Vector2(-7, -4);
    [SerializeField] private Vector2 worldWalkableMax = new Vector2(7, 4);

    [Header("Round Settings")]
    [SerializeField] private int crowdSize = 20;
    [SerializeField] private float roundTime = 10f;
    [SerializeField] private bool uniqueMasks = true;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    int _lives;
    bool _gameOver;

    [Header("Behavior Mix")]
    [Range(0, 1)][SerializeField] private float idleChance = 0.35f;
    [Range(0, 1)][SerializeField] private float walkChance = 0.40f;

    [Header("Difficulty Ramp")]
    [SerializeField] private int addCrowdPerWin = 2;
    [SerializeField] private float timeDecreasePerWin = 0.25f;
    [SerializeField] private float minRoundTime = 3.5f;
    [SerializeField] private float speedScalePerWin = 0.06f;

    [Header("UI")]
    [SerializeField] private PromptUI promptUI;
    [SerializeField] private GameOverUI gameOverUI; // <-- add this

    [Header("Spawn Separation")]
    [SerializeField] private float minSpawnDistance = 0.75f;
    [SerializeField] private int maxSpawnAttemptsPerAgent = 40;

    [SerializeField] private Transform crowdParent;

    [SerializeField] private SpriteRenderer fadeBlackBG;
    bool isFadingToBlack = false;

    readonly List<PartyPerson> _people = new();
    MaskData _targetMask;
    float _timeLeft;
    int _score;
    float _speedScale = 1f;

    Rect _bounds;
    Rect _walkableBounds;

    int _targetIndex = -1;

    void Start()
    {
        _bounds = Rect.MinMaxRect(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
        _walkableBounds = Rect.MinMaxRect(worldWalkableMin.x, worldWalkableMin.y, worldWalkableMax.x, worldWalkableMax.y);

        RestartGame();
    }

    void Update()
    {
        if (_gameOver) return;

        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            if (promptUI) promptUI.SetTimer(_timeLeft);

            LoseLife(); // time over costs 1 life
            return;
        }

        if (isFadingToBlack)
        {
            fadeBlackBG.color = Color.Lerp(fadeBlackBG.color, Color.black, 4.0f * Time.deltaTime);
        }
        else
        {
            fadeBlackBG.color = new Color(0, 0, 0, 0);
        }

        if (promptUI) promptUI.SetTimer(_timeLeft);
    }

    // --- NEW: central life loss handler ---
    void LoseLife()
    {
        if (_gameOver) return;

        _lives = Mathf.Max(0, _lives - 1);

        // If you have a lives UI method, call it here
         if (promptUI) promptUI.SetLives(_lives);

        if (_lives <= 0)
        {
            GameOver();
        }
        else
        {
            // Still alive: restart round
            FailRound(); // this calls StartRound()
        }
    }

    void GameOver()
    {
        _gameOver = true;
        _timeLeft = 0f;

        if (promptUI) promptUI.SetTimer(_timeLeft);

        // Optional: clear crowd so it looks "stopped"
        ClearCrowd();

        if (gameOverUI)
        {
            gameOverUI.Show(() =>
            {
                RestartGame();
            });
        }
        else
        {
            Debug.LogWarning("GameOverUI not assigned. Restarting immediately.");
            RestartGame();
        }
    }

    public void RestartGame()
    {
        _gameOver = false;
        _score = 0;
        _speedScale = 1f;

        // reset difficulty too (optional but recommended)
        // crowdSize = 20;
        // roundTime = 10f;

        _lives = maxLives;

        if (gameOverUI) gameOverUI.Hide();

        // if (promptUI) promptUI.SetLives(_lives);
        if (promptUI) promptUI.SetScore(_score);

        StartRound();
    }

    Vector2 FindNonOverlappingSpawnPoint(List<Vector2> usedPositions)
    {
        for (int attempt = 0; attempt < maxSpawnAttemptsPerAgent; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(_bounds.xMin, _bounds.xMax),
                Random.Range(_bounds.yMin, _bounds.yMax)
            );

            bool ok = true;
            for (int i = 0; i < usedPositions.Count; i++)
            {
                if ((candidate - usedPositions[i]).sqrMagnitude < minSpawnDistance * minSpawnDistance)
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
                return candidate;
        }

        return new Vector2(
            Random.Range(_bounds.xMin, _bounds.xMax),
            Random.Range(_bounds.yMin, _bounds.yMax)
        );
    }

    public void StartRound()
    {
        if (_gameOver) return;

        isFadingToBlack = false;

        ClearCrowd();
        SpawnCrowd();

        if (promptUI)
        {
            promptUI.ShowPrompt(_targetMask);
            promptUI.SetScore(_score);
        }

        _timeLeft = roundTime;
        if (promptUI) promptUI.SetTimer(_timeLeft);

    }

    void SpawnCrowd()
    {
        _people.Clear();

        _targetIndex = Random.Range(0, crowdSize);
        _targetMask = masks[Random.Range(0, masks.Length)];

        List<Vector2> usedPositions = new List<Vector2>(crowdSize);

        List<MaskData> nonTargetPool = new List<MaskData>(masks);
        nonTargetPool.RemoveAll(m => m == _targetMask);

        for (int i = 0; i < crowdSize; i++)
        {
            PartyPerson p = Instantiate(personPrefab, crowdParent);

            Vector2 spawnPos = FindNonOverlappingSpawnPoint(usedPositions);
            usedPositions.Add(spawnPos);

            p.transform.position = spawnPos;
            p.SetBounds(_walkableBounds);

            Sprite body = bodySprites[Random.Range(0, bodySprites.Length)];

            MaskData maskToUse;

            if (i == _targetIndex)
            {
                maskToUse = _targetMask;
            }
            else
            {
                if (uniqueMasks)
                {
                    if (nonTargetPool.Count == 0)
                    {
                        nonTargetPool = new List<MaskData>(masks);
                        nonTargetPool.RemoveAll(m => m == _targetMask);
                    }

                    int idx = Random.Range(0, nonTargetPool.Count);
                    maskToUse = nonTargetPool[idx];
                    nonTargetPool.RemoveAt(idx);
                }
                else
                {
                    maskToUse = nonTargetPool[Random.Range(0, nonTargetPool.Count)];
                }
            }

            PartyState st = RandomState();
            p.Init(body, maskToUse, st, _speedScale);

            _people.Add(p);
        }
    }

    PartyState RandomState()
    {
        float r = Random.value;
        if (r < idleChance) return PartyState.Idle;
        if (r < idleChance + walkChance) return PartyState.Walk;
        return PartyState.Dance;
    }

    public void StartFadingToBlack()
    {
        isFadingToBlack = true;
    }

    public void OnPersonClicked(PartyPerson person)
    {
        if (_gameOver) return;

        if (person.Mask == _targetMask)
        {
            _score++;
            if (promptUI) promptUI.SetScore(_score);

            crowdSize = Mathf.Min(120, crowdSize + addCrowdPerWin);
            roundTime = Mathf.Max(minRoundTime, roundTime - timeDecreasePerWin);
            _speedScale *= (1f + speedScalePerWin);

            StartRound();
        }
        else
        {
            if (promptUI) promptUI.FlashWrong();
            LoseLife();
        }
    }

    void FailRound()
    {
        if (promptUI) promptUI.FlashFail();
        StartRound();
    }

    void ClearCrowd()
    {
        foreach (var p in _people)
            if (p) Destroy(p.gameObject);
        _people.Clear();
    }
}
