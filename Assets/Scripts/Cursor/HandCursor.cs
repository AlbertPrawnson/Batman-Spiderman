using UnityEngine;
using UnityEngine.InputSystem;

public class HandCursor : MonoBehaviour
{
    [Header("Cursor Images")]
    [Tooltip("The normal cursor sprite that will be displayed by default")]
    public Texture2D normalCursorImage;

    [Tooltip("The cursor sprite that will be displayed when clicking (leave empty if not ready yet)")]
    public Texture2D clickedCursorImage;

    [Header("Click Behavior Settings")]
    [Tooltip("Enable this when you have the clicked cursor image ready")]
    public bool hasClickedImage = false;

    [Tooltip("Color tint to apply to the cursor when clicking (only used if hasClickedImage is false)")]
    public Color clickTintColor = Color.gray;

    [Header("Cursor Visual Settings")]
    [Tooltip("Scale multiplier for the cursor size")]
    [Range(0.1f, 5f)]
    public float scaleMultiplier = 1f;

    [Tooltip("Rotation angle for the cursor in degrees")]
    [Range(-180f, 180f)]
    public float rotationAngle = 0f;

    [Header("Cursor Hotspot Settings")]
    [Tooltip("The pivot point of the cursor sprite (0,0 = bottom-left, 0.5,0.5 = center, 1,1 = top-right)")]
    public Vector2 cursorPivot = new Vector2(0.5f, 0.5f);

    [Tooltip("Additional offset from the pivot point in world units")]
    public Vector2 clickPointOffset = Vector2.zero;

    [Header("Detection Settings")]
    [Tooltip("Detection radius around the cursor point to make hovering easier (in world units)")]
    [Range(0f, 2f)]
    public float detectionRadius = 0.1f;

    [Header("Debug Visualization")]
    [Tooltip("Show a visual point where the cursor actually clicks/detects")]
    public bool showDebugPoint = true;

    [Tooltip("Color of the debug point")]
    public Color debugPointColor = Color.red;

    [Tooltip("Size of the debug point")]
    [Range(0.01f, 0.5f)]
    public float debugPointSize = 0.05f;

    [Header("Cursor Settings")]
    [Tooltip("The hotspot offset for the cursor (usually the point where it clicks)")]
    public Vector2 cursorHotspot = Vector2.zero;

    private SpriteRenderer cursorSpriteRenderer;
    private GameObject cursorObject;
    private GameObject debugPointObject;
    private SpriteRenderer debugPointRenderer;
    private Color originalColor = Color.white;
    private bool isClicking = false;
    private Camera mainCamera;

    void Start()
    {
        // Cache the main camera
        mainCamera = Camera.main;

        // Hide the default system cursor
        Cursor.visible = false;

        // Create a GameObject to display the custom cursor as a sprite
        cursorObject = new GameObject("CustomCursor");
        cursorSpriteRenderer = cursorObject.AddComponent<SpriteRenderer>();
        cursorSpriteRenderer.sortingOrder = 1000; // Make sure it's always on top

        // Convert the normal cursor texture to a sprite
        if (normalCursorImage != null)
        {
            Sprite cursorSprite = Sprite.Create(
                normalCursorImage,
                new Rect(0, 0, normalCursorImage.width, normalCursorImage.height),
                cursorPivot, // Use the adjustable pivot here!
                100f
            );
            cursorSpriteRenderer.sprite = cursorSprite;
            originalColor = Color.white;
            cursorSpriteRenderer.color = originalColor;
        }

        // Create debug point visualization
        CreateDebugPoint();

        // Apply initial scale and rotation
        UpdateCursorTransform();
    }

    void CreateDebugPoint()
    {
        debugPointObject = new GameObject("CursorDebugPoint");
        debugPointRenderer = debugPointObject.AddComponent<SpriteRenderer>();
        debugPointRenderer.sortingOrder = 1001; // Above the cursor

        // Create a simple circle sprite for the debug point
        Texture2D circleTexture = CreateCircleTexture(32);
        Sprite circleSprite = Sprite.Create(
            circleTexture,
            new Rect(0, 0, circleTexture.width, circleTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        debugPointRenderer.sprite = circleSprite;
        debugPointRenderer.color = debugPointColor;

        debugPointObject.SetActive(showDebugPoint);
    }

    Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    void Update()
    {
        // Update cursor position to follow the mouse using new Input System
        if (cursorObject != null && mainCamera != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));
            worldPosition.z = 0; // Keep it on the same Z plane
            cursorObject.transform.position = worldPosition;

            // Update debug point position with offset
            if (debugPointObject != null)
            {
                Vector3 clickPoint = worldPosition + (Vector3)clickPointOffset;
                debugPointObject.transform.position = clickPoint;
                debugPointObject.transform.localScale = Vector3.one * debugPointSize;
                debugPointObject.SetActive(showDebugPoint);
                debugPointRenderer.color = debugPointColor;
            }
        }

        // Update scale and rotation if changed in inspector
        UpdateCursorTransform();

        // Handle click behavior using new Input System
        if (Mouse.current.leftButton.wasPressedThisFrame && !isClicking)
        {
            OnCursorClick();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isClicking)
        {
            OnCursorRelease();
        }
    }

    private void UpdateCursorTransform()
    {
        if (cursorObject != null)
        {
            cursorObject.transform.localScale = Vector3.one * scaleMultiplier;
            cursorObject.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }
    }

    // Public method to get the actual click/detection point
    public Vector3 GetClickPoint()
    {
        if (cursorObject != null)
        {
            return cursorObject.transform.position + (Vector3)clickPointOffset;
        }
        return Vector3.zero;
    }

    // Public method to get the detection radius
    public float GetDetectionRadius()
    {
        return detectionRadius;
    }

    private void OnCursorClick()
    {
        isClicking = true;

        if (hasClickedImage && clickedCursorImage != null)
        {
            // Change to the clicked cursor image
            Sprite clickedSprite = Sprite.Create(
                clickedCursorImage,
                new Rect(0, 0, clickedCursorImage.width, clickedCursorImage.height),
                cursorPivot, // Use the adjustable pivot here too!
                100f
            );
            cursorSpriteRenderer.sprite = clickedSprite;
        }
        else
        {
            // Apply color tint instead
            cursorSpriteRenderer.color = clickTintColor;
        }
    }

    private void OnCursorRelease()
    {
        isClicking = false;

        if (hasClickedImage && clickedCursorImage != null)
        {
            // Change back to the normal cursor image
            Sprite normalSprite = Sprite.Create(
                normalCursorImage,
                new Rect(0, 0, normalCursorImage.width, normalCursorImage.height),
                cursorPivot, // Use the adjustable pivot here too!
                100f
            );
            cursorSpriteRenderer.sprite = normalSprite;
        }
        else
        {
            // Remove color tint
            cursorSpriteRenderer.color = originalColor;
        }
    }

    void OnDestroy()
    {
        // Clean up and restore the system cursor
        if (cursorObject != null)
        {
            Destroy(cursorObject);
        }
        if (debugPointObject != null)
        {
            Destroy(debugPointObject);
        }
        Cursor.visible = true;
    }
}