using UnityEngine;

public class ClickPicker : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private PartyManager manager;

    int _peopleMask;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        _peopleMask = LayerMask.GetMask("People");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryPick(Input.mousePosition);

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryPick(Input.GetTouch(0).position);
    }

    void TryPick(Vector2 screenPos)
    {
        Vector2 world = cam.ScreenToWorldPoint(screenPos);

        // Raycast zero-length to pick overlapping colliders properly
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0f, _peopleMask);
        if (!hit.collider) return;

        PartyPerson person = hit.collider.GetComponentInParent<PartyPerson>();
        if (!person) return;

        manager.OnPersonClicked(person);
    }
}
