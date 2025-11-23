using UnityEngine;
using UnityEngine.InputSystem;

public class HoverableObject : MonoBehaviour
{
    [Header("Ingredient Data")]
    [Tooltip("The ingredient data this hoverable object represents")]
    public IngredientData ingredientData;

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

    [Header("Draggable Settings")]
    [Tooltip("The prefab to instantiate when this object is clicked")]
    public GameObject draggableIngredientPrefab;

    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;
    private bool isCurrentlyHovered = false;
    private Camera mainCamera;
    private HandCursor handCursor;
    private bool isBeingDragged = false; // To prevent multiple instantiations

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
            Debug.LogError($"HoverableObject on {gameObject.name} requires a Collider2D component!");
            enabled = false;
            return;
        }

        // Cache the main camera
        mainCamera = Camera.main;

        // Find the HandCursor in the scene
        handCursor = FindFirstObjectByType<HandCursor>();

        // Set initial color or sprite
        if (hasHoverImages && unhoveredSprite != null)
        {
            spriteRenderer.sprite = unhoveredSprite;
        }
        else if (!hasHoverImages)
        {
            spriteRenderer.color = unhoveredColor;
        }

        // If ingredientData exists, use its sprites
        if (ingredientData != null)
        {
            if (ingredientData.ingredientSprite != null)
            {
                spriteRenderer.sprite = ingredientData.ingredientSprite;
                unhoveredSprite = ingredientData.ingredientSprite;
            }
            if (ingredientData.ingredientHoverSprite != null)
            {
                hoveredSprite = ingredientData.ingredientHoverSprite;
                hasHoverImages = true;
            }
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
                float radius = handCursor.GetDetectionRadius();
                isHovering = IsPointNearCollider(checkPoint, radius);
            }
            else
            {
                isHovering = objectCollider.OverlapPoint(checkPoint);
            }
        }
        else
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            checkPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));
            isHovering = objectCollider.OverlapPoint(checkPoint);
        }

        // Handle hover state changes
        if (isHovering && !isCurrentlyHovered)
        {
            OnHoverEnter();
        }
        else if (!isHovering && isCurrentlyHovered)
        {
            OnHoverExit();
        }

        // Handle click to instantiate draggable ingredient
        if (isHovering && Mouse.current.leftButton.wasPressedThisFrame && !isBeingDragged)
        {
            OnClicked();
        }
    }

    private bool IsPointNearCollider(Vector3 point, float radius)
    {
        if (objectCollider.OverlapPoint(point))
            return true;

        Vector2 closestPoint = objectCollider.ClosestPoint(point);
        float distance = Vector2.Distance(point, closestPoint);

        return distance <= radius;
    }

    private void OnHoverEnter()
    {
        isCurrentlyHovered = true;

        if (hasHoverImages && hoveredSprite != null)
        {
            spriteRenderer.sprite = hoveredSprite;
        }
        else
        {
            spriteRenderer.color = hoveredColor;
        }
    }

    private void OnHoverExit()
    {
        isCurrentlyHovered = false;

        if (hasHoverImages && unhoveredSprite != null)
        {
            spriteRenderer.sprite = unhoveredSprite;
        }
        else
        {
            spriteRenderer.color = unhoveredColor;
        }
    }

    private void OnClicked()
    {
        if (draggableIngredientPrefab == null)
        {
            Debug.LogWarning($"No draggable prefab assigned to {gameObject.name}!");
            return;
        }

        if (ingredientData == null)
        {
            Debug.LogWarning($"No ingredient data assigned to {gameObject.name}!");
            return;
        }

        // Instantiate the draggable ingredient at the current position
        GameObject draggableInstance = Instantiate(draggableIngredientPrefab, transform.position, Quaternion.identity);

        // Set up the draggable ingredient with the data and parent reference
        DraggableIngredient draggable = draggableInstance.GetComponent<DraggableIngredient>();
        if (draggable != null)
        {
            draggable.Initialize(ingredientData, this);
            isBeingDragged = true;
        }
    }

    // Called by DraggableIngredient when it's released or returns
    public void OnDraggableReturned()
    {
        isBeingDragged = false;
    }

    // Public getter for the parent position (for the draggable to return to)
    public Vector3 GetReturnPosition()
    {
        return transform.position;
    }

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