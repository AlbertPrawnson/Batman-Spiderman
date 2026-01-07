using System.Collections;
using UnityEngine;

public enum PotState { Empty, RawPasta, SoggyPasta, SaucePasta, Stir }

public class PotManager : MonoBehaviour
{
    public static PotManager Instance { get; private set; }

    [Header("Pot state GameObjects (assign in inspector)")]
    public GameObject potEmpty;      // optional
    public GameObject rawPastaGO;    // RawPasta visual
    public GameObject soggyPastaGO;  // SoggyPasta visual
    public GameObject saucePastaGO;  // SaucePasta visual
    public GameObject stirGO;     // Stir visual

    [Header("Game UI")]
    public GameObject winScreen;     // assign your winning UI panel

    private PotState currentState = PotState.Empty;
    private Coroutine soggyCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetState(PotState.Empty);
        if (winScreen != null) winScreen.SetActive(false);
    }

    public void HandleItemClicked(ItemType item, GameObject clickedObject)
    {
        // Ignore clicks that are not relevant
        switch (currentState)
        {
            case PotState.Empty:
                // Only Pasta should create RawPasta when clicked on Pasta then pot
                if (item == ItemType.Pasta)
                {
                    // Optionally check that the player clicked the pot after selecting pasta.
                    // For simplicity: clicking Pasta directly starts RawPasta.
                    SetState(PotState.RawPasta);
                    // Start timed transition to SoggyPasta
                    if (soggyCoroutine != null) StopCoroutine(soggyCoroutine);
                    soggyCoroutine = StartCoroutine(RawToSoggyDelay(2f));
                }
                break;

            case PotState.RawPasta:
                // become SoggyPasta
                break;

            case PotState.SoggyPasta:
                // Only TomatoSauce should change SoggyPasta -> SaucePasta
                if (item == ItemType.TomatoSauce)
                {
                    SetState(PotState.SaucePasta);
                }
                break;

            case PotState.SaucePasta:
                // Only PastaSpoon should change SaucePasta -> Stirred
                if (item == ItemType.PastaSpoon)
                {
                    SetState(PotState.Stir);
                }
                break;

            case PotState.Stir:
                // Only Plate interacting with Stirred triggers win
                if (item == ItemType.Plate)
                {
                    OnWin();
                }
                break;
        }

        // If click doesn't match any allowed transition, do nothing.
    }

    private IEnumerator RawToSoggyDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // Only transition if still in RawPasta
        if (currentState == PotState.RawPasta)
            SetState(PotState.SoggyPasta);
        soggyCoroutine = null;
    }

    private void SetState(PotState newState)
    {
        currentState = newState;
        // Hide all pot visuals, then enable only the one for current state
        potEmpty?.SetActive(false);
        rawPastaGO?.SetActive(false);
        soggyPastaGO?.SetActive(false);
        saucePastaGO?.SetActive(false);
        stirGO?.SetActive(false);

        switch (newState)
        {
            case PotState.Empty:
                potEmpty?.SetActive(true);
                break;
            case PotState.RawPasta:
                rawPastaGO?.SetActive(true);
                break;
            case PotState.SoggyPasta:
                soggyPastaGO?.SetActive(true);
                break;
            case PotState.SaucePasta:
                saucePastaGO?.SetActive(true);
                break;
            case PotState.Stir:
                stirGO?.SetActive(true);
                break;
        }
    }

    private void OnWin()
    {
        // Show win UI and optionally stop further interactions
        if (winScreen != null) winScreen.SetActive(true);
        // Optionally disable all interactables
        var all = FindObjectsOfType<InteractableItem>();
        foreach (var it in all) it.enabled = false;
        // Additional game over logic here
    }
}