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

    [Header("World Bounds (spawn + clamp)")]
    [SerializeField] private Vector2 worldWalkableMin = new Vector2(-7, -4);
    [SerializeField] private Vector2 worldWalkableMax = new Vector2(7, 4);

    [Header("Round Settings")]
    [SerializeField] private int crowdSize = 20;
    [SerializeField] private float roundTime = 10f;
    [SerializeField] private bool uniqueMasks = true;

    [Header("Behavior Mix")]
    [Range(0, 1)][SerializeField] private float idleChance = 0.35f;
    [Range(0, 1)][SerializeField] private float walkChance = 0.40f;
    // danceChance = 1 - idle - walk

    [Header("Difficulty Ramp")]
    [SerializeField] private int addCrowdPerWin = 2;
    [SerializeField] private float timeDecreasePerWin = 0.25f;
    [SerializeField] private float minRoundTime = 3.5f;
    [SerializeField] private float speedScalePerWin = 0.06f;

    [Header("UI")]
    [SerializeField] private PromptUI promptUI;

    [Header("Spawn Separation")]
    [SerializeField] private float minSpawnDistance = 0.75f;
    [SerializeField] private int maxSpawnAttemptsPerAgent = 40;

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
        StartRound();
    }

    void Update()
    {
        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            if (promptUI) promptUI.SetTimer(_timeLeft);
            FailRound();
            return;
        }

        if (promptUI) promptUI.SetTimer(_timeLeft);
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

        // Fallback: if crowded, return any point (or loosen distance)
        return new Vector2(
            Random.Range(_bounds.xMin, _bounds.xMax),
            Random.Range(_bounds.yMin, _bounds.yMax)
        );
    }


    public void StartRound()
    {
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

        // Decide the one-and-only target up front
        _targetIndex = Random.Range(0, crowdSize);
        _targetMask = masks[Random.Range(0, masks.Length)];

        // Track accepted positions
        List<Vector2> usedPositions = new List<Vector2>(crowdSize);

        // Build a pool for NON-target masks
        List<MaskData> nonTargetPool = new List<MaskData>(masks);
        nonTargetPool.RemoveAll(m => m == _targetMask);

        for (int i = 0; i < crowdSize; i++)
        {
            PartyPerson p = Instantiate(personPrefab);

            Vector2 spawnPos = FindNonOverlappingSpawnPoint(usedPositions);
            usedPositions.Add(spawnPos);

            p.transform.position = spawnPos;
            p.SetBounds(_walkableBounds);

            Sprite body = bodySprites[Random.Range(0, bodySprites.Length)];

            MaskData maskToUse;

            if (i == _targetIndex)
            {
                // The only target
                maskToUse = _targetMask;
            }
            else
            {
                // Everyone else must NOT be target
                if (uniqueMasks)
                {
                    // If we run out, we can either allow repeats or re-fill (still excluding target)
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
                    // repeats allowed, but never the target mask
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

    MaskData PickTargetMaskFromCrowd()
    {
        return _people[Random.Range(0, _people.Count)].Mask;
    }

    public void OnPersonClicked(PartyPerson person)
    {
        if (person.Mask == _targetMask)
        {
            _score++;
            if (promptUI) promptUI.SetScore(_score);

            // Ramp difficulty
            crowdSize = Mathf.Min(120, crowdSize + addCrowdPerWin);
            roundTime = Mathf.Max(minRoundTime, roundTime - timeDecreasePerWin);
            _speedScale *= (1f + speedScalePerWin);

            StartRound();
        }
        else
        {
            // Penalty: shave time
            _timeLeft = Mathf.Max(0f, _timeLeft - 1.0f);
            if (promptUI) promptUI.FlashWrong();
        }
    }

    void FailRound()
    {
        if (promptUI) promptUI.FlashFail();
        // Jam-friendly: restart
        StartRound();
    }

    void ClearCrowd()
    {
        foreach (var p in _people)
            if (p) Destroy(p.gameObject);
        _people.Clear();
    }
}
