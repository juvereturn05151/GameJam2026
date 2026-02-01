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

    readonly List<PartyPerson> _people = new();
    MaskData _targetMask;
    float _timeLeft;
    int _score;
    float _speedScale = 1f;

    Rect _bounds;

    void Start()
    {
        _bounds = Rect.MinMaxRect(worldMin.x, worldMin.y, worldMax.x, worldMax.y);
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

    public void StartRound()
    {
        ClearCrowd();
        SpawnCrowd();

        _targetMask = PickTargetMaskFromCrowd();
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
        // Build a pool for uniqueness
        List<MaskData> pool = new List<MaskData>(masks);

        for (int i = 0; i < crowdSize; i++)
        {
            PartyPerson p = Instantiate(personPrefab);

            // Spawn in bounds
            p.transform.position = new Vector2(
                Random.Range(_bounds.xMin, _bounds.xMax),
                Random.Range(_bounds.yMin, _bounds.yMax)
            );

            p.SetBounds(_bounds);

            // Choose body
            Sprite body = bodySprites[Random.Range(0, bodySprites.Length)];

            // Choose mask
            MaskData mask;
            if (uniqueMasks)
            {
                if (pool.Count == 0) pool = new List<MaskData>(masks);
                int idx = Random.Range(0, pool.Count);
                mask = pool[idx];
                pool.RemoveAt(idx);
            }
            else
            {
                mask = masks[Random.Range(0, masks.Length)];
            }

            // Choose behavior
            PartyState st = RandomState();
            p.Init(body, mask, st, _speedScale);

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
