using System.Collections;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Assign backgrounds in inspector")]
    public GameObject background1; // (default)
    public GameObject background2;
    public GameObject background3;

    private Coroutine currentCoroutine;

    // Call this when TriggerZone1 is activated (show background2)
    public void TriggerBackground2()
    {
        // Show background2 and temporarily hide the others
        SetActiveBackground(background2);
    }

    // Call this when TriggerZone2 is activated (show background3)
    public void TriggerBackground3()
    {
        SetActiveBackground(background3);
    }

    private void SetActiveBackground(GameObject activeBg)
    {
        // Ensure default background1 remains visible only if it's the active one
        // Disable the two that are NOT activeBg
        GameObject[] all = new GameObject[] { background1, background2, background3 };

        // Stop previous coroutine so timer restarts on repeated triggers
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        // Activate the requested background immediately
        foreach (var bg in all)
            bg.SetActive(bg == activeBg);

        // Start coroutine to re-enable all backgrounds after 4 seconds
        currentCoroutine = StartCoroutine(ReenableOthersAfterDelay(4f, activeBg));
    }

    private IEnumerator ReenableOthersAfterDelay(float seconds, GameObject activeBg)
    {
        yield return new WaitForSeconds(seconds);

        // After delay, make all backgrounds visible again (or restore default behavior)
        background1.SetActive(true);
        background2.SetActive(true);
        background3.SetActive(true);

        currentCoroutine = null;
    }
}



