using UnityEngine;

public enum PartyState { Idle, Walk, Dance }

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PartyPerson : MonoBehaviour
{
    public MaskData Mask { get; private set; }
    public PartyState State { get; private set; }

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer maskRenderer;

    [Header("Optional Animator (recommended if you have clips)")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float changeDirInterval = 1.4f;

    [Tooltip("If true, agent moves along iso grid directions (NE/NW/SE/SW).")]
    [SerializeField] private bool isoFourDirections = true;

    [Header("Dance (fallback if no animator)")]
    [SerializeField] private float danceBobSpeed = 10f;
    [SerializeField] private float danceBobAmount = 0.08f;

    Rigidbody2D _rb;
    Vector2 _moveDir;
    float _nextDirTime;

    Rect _bounds;
    bool _useBounds;

    Vector3 _baseLocalPos;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _baseLocalPos = transform.localPosition;
    }

    public void SetBounds(Rect worldBounds)
    {
        _bounds = worldBounds;
        _useBounds = true;
    }

    public void Init(Sprite bodySprite, MaskData mask, PartyState startState, float speedScale = 1f)
    {
        if (bodyRenderer) bodyRenderer.sprite = bodySprite;

        Mask = mask;
        if (maskRenderer && mask != null)
        {
            maskRenderer.sprite = mask.maskSprite;
            maskRenderer.transform.localPosition = mask.localOffset;
            maskRenderer.transform.localScale = Vector3.one * mask.localScale;
        }

        walkSpeed *= speedScale;
        SetState(startState);

        UpdateIsoSorting();
    }

    public void SetState(PartyState newState)
    {
        State = newState;

        if (animator)
        {
            // Requires Animator states named: Idle, Walk, Dance
            animator.Play(newState.ToString());
        }

        if (State == PartyState.Walk)
        {
            PickNewDirection();
            _nextDirTime = Time.time + changeDirInterval * Random.Range(0.75f, 1.25f);
            animator.SetBool("Walk", true);
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            if (State == PartyState.Dance)
            {
                animator.SetBool("Dance", true);
            }
            else if (State == PartyState.Idle) 
            {
                animator.SetBool("Idle", true);
            }
        }
    }

    void Update()
    {
        if (State == PartyState.Walk)
        {
            if (Time.time >= _nextDirTime)
            {
                PickNewDirection();
                _nextDirTime = Time.time + changeDirInterval * Random.Range(0.75f, 1.25f);
            }

            UpdateFacing();
        }
        else if (State == PartyState.Dance && animator == null)
        {
            // Simple bobbing "dance"
            float t = Time.time * danceBobSpeed;
            transform.localPosition = transform.localPosition + new Vector3(0, Mathf.Sin(t) * danceBobAmount, 0);
        }
        else
        {
            //transform.localPosition = _baseLocalPos;
        }

        UpdateIsoSorting();
    }

    void FixedUpdate()
    {
        if (State == PartyState.Walk)
            _rb.linearVelocity = _moveDir * walkSpeed;

        if (_useBounds)
        {
            Vector2 p = _rb.position;
            p.x = Mathf.Clamp(p.x, _bounds.xMin, _bounds.xMax);
            p.y = Mathf.Clamp(p.y, _bounds.yMin, _bounds.yMax);
            _rb.position = p;
        }
    }

    void UpdateFacing()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (_moveDir.x > 0 ? -1f : 1f);
        transform.localScale = scale;
    }

    void PickNewDirection()
    {
        if (isoFourDirections)
        {
            // Diamond iso directions: NE, NW, SE, SW
            // In world XY, these are diagonals.
            // (You can tweak if your art uses different projection.)
            Vector2[] dirs = new Vector2[]
            {
                new Vector2( 1,  1), // NE
                new Vector2(-1,  1), // NW
                new Vector2( 1, -1), // SE
                new Vector2(-1, -1), // SW
            };
            _moveDir = dirs[Random.Range(0, dirs.Length)].normalized;
        }
        else
        {
            _moveDir = Random.insideUnitCircle.normalized;
            if (_moveDir.sqrMagnitude < 0.01f) _moveDir = Vector2.right;
        }
    }

    void UpdateIsoSorting()
    {
        // Isometric depth sorting: lower Y draws on top
        int baseOrder = Mathf.RoundToInt(-transform.position.y * 100f);

        if (bodyRenderer) bodyRenderer.sortingOrder = baseOrder;
        if (maskRenderer) maskRenderer.sortingOrder = baseOrder + 1;
    }
}
