using UnityEngine;
using System.Collections;

public class ClickPicker : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private PartyManager manager;
    [SerializeField] private Transform pickerPeopleParent;

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

        person.transform.SetParent(pickerPeopleParent, true);
        person.MaskRenderer.sortingLayerName = "PickObject";
        person.BodyRenderer.sortingLayerName = "PickObject";

        manager.PreCheckRightWrong(person);

        manager.StartFadingToBlack();

        StartCoroutine(DelayedPick(person, 1f));
    }

    IEnumerator DelayedPick(PartyPerson person, float delay)
    {
        yield return new WaitForSeconds(delay);

        manager.OnPersonClicked(person);
    }
}
