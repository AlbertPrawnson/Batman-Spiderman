using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public BackgroundManager manager;
    public int zoneId = 1; // 1 => background2, 2 => background3

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (zoneId == 1) manager.TriggerBackground2();
        else if (zoneId == 2) manager.TriggerBackground3();
    }
}