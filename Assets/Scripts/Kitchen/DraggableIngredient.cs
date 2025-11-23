using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DraggableIngredient : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed at which the ingredient returns to parent")]
    [Range(1f, 20f)]
    public float returnSpeed = 8f;

    [Tooltip("Distance threshold to consider 'returned' to parent")]
    public float returnThreshold = 0.05f;

    [Header("Visual Feedback")]
    [Tooltip("Scale multiplier while being dragged")]
    public float dragScale = 1.1f;

    [Tooltip("Color tint while being dragged (optional)")]
    public Color dragTint = Color.white;

    // References
    private IngredientData ingredientData;
    private HoverableObject parentHoverable;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private HandCursor handCursor;

    // State
    private bool isDragging = false;
    private bool isReturning = false;
    private Vector3 originalScale;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        mainCamera = Camera.main;
        handCursor = FindFirstObjectByType<HandCursor>();

        originalScale = transform.localScale;
    }

    public void Initialize(IngredientData data, HoverableObject parent)
    {
        ingredientData = data;
        parentHoverable = parent;

        // Set up the visual
        if (ingredientData != null && ingredientData.ingredientSprite != null)
        {
            spriteRenderer.sprite = ingredientData.ingredientSprite;
        }

        spriteRenderer.sortingOrder = 100; // Above other objects
        originalColor = spriteRenderer.color;

        // Start dragging immediately
        StartDragging();
    }

    private void StartDragging()
    {
        isDragging = true;
        transform.localScale = originalScale * dragScale;
        spriteRenderer.color = dragTint;
    }

    void Update()
    {
        if (isDragging)
        {
            // Follow the cursor
            if (handCursor != null)
            {
                transform.position = handCursor.GetClickPoint();
            }
            else if (mainCamera != null && Mouse.current != null)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));
                worldPosition.z = 0;
                transform.position = worldPosition;
            }

            // Check if mouse button is released
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                OnReleased();
            }
        }
        else if (isReturning)
        {
            ReturnToParent();
        }
    }

    private void OnReleased()
    {
        isDragging = false;

        // TODO: Check if dropped on a recipe area (implement later)
        // For now, just return to parent

        // Reset visual
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;

        // Start returning to parent
        StartReturning();
    }

    private void StartReturning()
    {
        if (parentHoverable == null)
        {
            Destroy(gameObject);
            return;
        }

        isReturning = true;
    }

    private void ReturnToParent()
    {
        if (parentHoverable == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = parentHoverable.GetReturnPosition();

        // Move towards parent
        transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);

        // Check if close enough
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < returnThreshold)
        {
            // Arrived! Notify parent and destroy
            parentHoverable.OnDraggableReturned();
            Destroy(gameObject);
        }
    }

    // Public getter for ingredient data (for recipe checking later)
    public IngredientData GetIngredientData()
    {
        return ingredientData;
    }

    void OnDestroy()
    {
        // Make sure to notify parent when destroyed
        if (parentHoverable != null && (isDragging || isReturning))
        {
            parentHoverable.OnDraggableReturned();
        }
    }
}