using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LimitTile : MonoBehaviour
{
    public bool hideVisual = true;

    private void Awake()
    {
        ApplySettings();
    }

    private void Reset()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        MatchColliderToVisual();
        SetVisualState();
    }

    void MatchColliderToVisual()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Transform spriteTransform = spriteRenderer.transform;
        Bounds spriteBounds = spriteRenderer.bounds;
        Vector3 localCenter = transform.InverseTransformPoint(spriteBounds.center);
        Vector3 lossyScale = transform.lossyScale;

        float sizeX = spriteBounds.size.x;
        float sizeY = spriteBounds.size.y;

        if (Mathf.Abs(lossyScale.x) > 0.001f)
        {
            sizeX /= Mathf.Abs(lossyScale.x);
        }

        if (Mathf.Abs(lossyScale.y) > 0.001f)
        {
            sizeY /= Mathf.Abs(lossyScale.y);
        }

        boxCollider.offset = new Vector2(localCenter.x, localCenter.y);
        boxCollider.size = new Vector2(sizeX, sizeY);
        boxCollider.isTrigger = false;
    }

    void SetVisualState()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = !hideVisual;
        }
    }
}
