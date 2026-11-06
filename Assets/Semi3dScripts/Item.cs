using UnityEngine;

public class Item : MonoBehaviour
{
        [Header("Obstacle Type")]
    public bool jumpable = true;
    public float clearHeight = 1f;
    public float yOffset = 0f;

    [HideInInspector] public Vector3 itemPosition;
    [HideInInspector] public bool hasHit;
    [HideInInspector] public int poolIndex;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float perspective = CameraComponent.focalLength / (CameraComponent.focalLength + itemPosition.z);
        transform.localScale = baseScale * perspective;
        transform.position = new Vector2(itemPosition.x, itemPosition.y + yOffset) * perspective;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-itemPosition.z);
    }
}
