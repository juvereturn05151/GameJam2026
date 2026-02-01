using UnityEngine;

[CreateAssetMenu(menuName = "Halloween/Mask Data")]
public class MaskData : ScriptableObject
{
    public string maskName;         
    public Sprite maskSprite;

    // optional hint text
    [TextArea] public string description;

    // Per-mask alignment tweaks
    public Vector2 localOffset = Vector2.zero;
    public float localScale = 1f;
}
