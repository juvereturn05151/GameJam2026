using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeadIcon : MonoBehaviour, IPointerClickHandler
{
    public bool IsTarget { get; private set; }

    private RectTransform rt;
    public RectTransform Rect => rt;
    private RectTransform bounds;
    private Vector2 velocity;
    private GameManager manager;

    void Awake()
    {
        rt = (RectTransform)transform;
    }

    public void Init(GameManager manager, RectTransform bounds, Sprite sprite, bool isTarget, Vector2 velocity)
    {
        this.manager = manager;
        this.bounds = bounds;
        IsTarget = isTarget;
        this.velocity = velocity;

        GetComponent<Image>().sprite = sprite;
    }

    void Update()
    {
        if (bounds == null) return;

        rt.anchoredPosition += velocity * Time.deltaTime;

        Vector2 pos = rt.anchoredPosition;
        Vector2 half = rt.rect.size * 0.5f;
        Rect b = bounds.rect;

        float minX = b.xMin + half.x;
        float maxX = b.xMax - half.x;
        float minY = b.yMin + half.y;
        float maxY = b.yMax - half.y;

        if (pos.x < minX) { pos.x = minX; velocity.x *= -1f; }
        if (pos.x > maxX) { pos.x = maxX; velocity.x *= -1f; }
        if (pos.y < minY) { pos.y = minY; velocity.y *= -1f; }
        if (pos.y > maxY) { pos.y = maxY; velocity.y *= -1f; }

        rt.anchoredPosition = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.OnHeadClicked(this);
    }
}
