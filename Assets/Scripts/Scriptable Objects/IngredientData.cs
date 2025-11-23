using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Batman-Spiderman/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("Ingredient Info")]
    public string ingredientName;
    public int ingredientID;

    [Header("Visual")]
    public Sprite ingredientSprite;
    public Sprite ingredientHoverSprite; // For the hoverable version

    [Header("Recipe Data (For Future)")]
    [Tooltip("Tags that define what recipes this ingredient can be used in")]
    public string[] recipeTags;

    [TextArea(3, 5)]
    public string description;
}