using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public Sprite[] backgrounds;  // Array to hold background sprites
    private SpriteRenderer spriteRenderer;
    private Sprite Background1;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Background1 = spriteRenderer.sprite;  // Store the original background
    }
    public void ChangeBackground(int index)
    {
        if (index >= 0 && index < backgrounds.Length)
        {
            spriteRenderer.sprite = backgrounds[index];  // Change the background
            StartCoroutine(ResetBackground(4f));  // Call to reset background after 4 seconds
        }
    }

    private System.Collections.IEnumerator ResetBackground(float delay)
    {
        yield return new WaitForSeconds(delay);  // Wait for the specified time
        spriteRenderer.sprite = Background1;  // Reset to the original background
    }
}


