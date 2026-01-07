using UnityEngine;

public enum ItemType { None, Pasta, TomatoSauce, Pot, PastaSpoon, Salt, Plate, Stir }

public class InteractableItem : MonoBehaviour
{
    public ItemType itemType = ItemType.None;

    private void OnMouseDown()
    {
        // Send click to the PotManager
        PotManager.Instance.HandleItemClicked(itemType, gameObject);
    }
}

