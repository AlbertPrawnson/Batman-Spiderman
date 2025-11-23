using UnityEngine;
using UnityEngine.InputSystem;

public class HoverableObject : MonoBehaviour
{
    [Header("Image Settings (Future)")]
    [Tooltip("Enable this when you have the hover sprite images ready")]
    public bool hasHoverImages = false;

    [Tooltip("The sprite to show when NOT hovered (leave empty if not ready yet)")]
    public Sprite unhoveredSprite;

    [Tooltip("The sprite to show when hovered (leave empty if not ready yet)")]
    public Sprite hoveredSprite;

    [Header("Color Settings (Testing)")]
    [Tooltip("Color when the object is NOT hovered")]
    public Color unhoveredColor = Color.white;

    [Tooltip("Color when the object IS hovered")]
    public Color hoveredColor = Color.yellow;

    [Header("Detection Settings")]
    [Tooltip("Use the cursor's detection radius for more forgiving hover detection")]
    public bool useDetectionRadius = true;

    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;
    private bool isCurrentlyHovered = false;
    private Camera mainCamera;
    private HandCursor handCursor;

    void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"HoverableObject on {gameObject.name} requires a SpriteRenderer component!");
            enabled = false;
            return;
        }

        // Get any type of 2D collider attached to this object
        objectCollider = GetComponent<Collider2D>();

        if (objectCollider == null)
        {
            Debug.LogError($"HoverableObject on {gameObject.name} requires a Collider2D component (BoxCollider2D, CircleCollider2D, PolygonCollider2D, etc.)!");
            enabled = false;
            return;
        }

        // Cache the main camera
        mainCamera = Camera.main;

        // Find the HandCursor in the scene
        handCursor = FindFirstObjectByType<HandCursor>();

        // Set initial color
        if (!hasHoverImages)
        {
            spriteRenderer.color = unhoveredColor;
        }
    }

    void Update()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector3 checkPoint;
        bool isHovering;

        // Use cursor's click point if HandCursor exists
        if (handCursor != null)
        {
            checkPoint = handCursor.GetClickPoint();

            if (useDetectionRadius)
            {
                // Check with radius for more forgiving detection
                float radius = handCursor.GetDetectionRadius();
                isHovering = IsPointNearCollider(checkPoint, radius);
            }
            else
            {
                // Precise point detection
                isHovering = objectCollider.OverlapPoint(checkPoint);
            }
        }
        else
        {
            // Fallback to raw mouse position
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            checkPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));
            isHovering = objectCollider.OverlapPoint(checkPoint);
        }

        // If hover state changed, update the visual
        if (isHovering && !isCurrentlyHovered)
        {
            OnHoverEnter();
        }
        else if (!isHovering && isCurrentlyHovered)
        {
            OnHoverExit();
        }
    }

    private bool IsPointNearCollider(Vector3 point, float radius)
    {
        // Check if point is directly in collider
        if (objectCollider.OverlapPoint(point))
            return true;

        // Check if point is within radius of collider
        Vector2 closestPoint = objectCollider.ClosestPoint(point);
        float distance = Vector2.Distance(point, closestPoint);

        return distance <= radius;
    }

    private void OnHoverEnter()
    {
        isCurrentlyHovered = true;

        if (hasHoverImages && hoveredSprite != null)
        {
            // Change to hovered sprite
            spriteRenderer.sprite = hoveredSprite;
        }
        else
        {
            // Change to hovered color
            spriteRenderer.color = hoveredColor;
        }
    }

    private void OnHoverExit()
    {
        isCurrentlyHovered = false;

        if (hasHoverImages && unhoveredSprite != null)
        {
            // Change back to unhovered sprite
            spriteRenderer.sprite = unhoveredSprite;
        }
        else
        {
            // Change back to unhovered color
            spriteRenderer.color = unhoveredColor;
        }
    }

    // Optional: Visualize the collider in the editor
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}