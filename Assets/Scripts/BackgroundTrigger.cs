using UnityEngine;

public class BackgroundTrigger : MonoBehaviour
{
    public BackgroundManager backgroundManager;  // Reference to the BackgroundManager
    public int backgroundIndex;  // Index of the background to change to

    private void OnMouseDown()
    {
        backgroundManager.ChangeBackground(backgroundIndex);  // Change the background on click
    }
}
