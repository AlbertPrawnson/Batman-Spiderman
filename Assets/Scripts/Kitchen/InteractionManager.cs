using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InteractionManager : MonoBehaviour
{
    private GameObject firstClickedObject;  // Track the first clicked object
    private GameObject secondClickedObject; // Track the second clicked object

    // Possible pot states
    private enum PotState
    {
        Empty,
        RawPasta,
        SoggyPasta,
        SaucePasta,
        Stir
    }

    private PotState currentPotState = PotState.Empty; // Current state of the pot

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (firstClickedObject == null)
            {
                firstClickedObject = clickedObject; // First click
            }
            else if (secondClickedObject == null)
            {
                secondClickedObject = clickedObject; // Second click
                ProcessInteraction(firstClickedObject, secondClickedObject); // Process the interaction
                ResetClickStates(); // Reset for next interaction
            }
        }
    }

    private void ProcessInteraction(GameObject obj1, GameObject obj2)
    {
        if (IsValidInteraction(obj1, obj2))
        {
            Debug.Log($"Valid Interaction: {obj1.name} with {obj2.name}");
            UpdatePotState(obj1, obj2); // Update pot state based on the clicked objects

            // Check for the final interaction for winning
            if (IsWinningInteraction(obj1, obj2))
            {
                ShowWinningScreen();
            }
        }
        else
        {
            Debug.Log("Invalid Interaction - Nothing Happened.");
        }
    }

    private bool IsValidInteraction(GameObject obj1, GameObject obj2)
    {
        // Define valid interactions based on current pot state
        switch (currentPotState)
        {
            // Order is important: first click must match the left-hand item, second click the right-hand item
            // Order is important: first click must match the left-hand item, second click the right-hand item
            case PotState.Empty:
                // Click Pasta, then Pot -> add pasta to pot
                return obj1.name == "Pasta" && obj2.name == "Pot";
            case PotState.RawPasta:
                // RawPasta should automatically become SoggyPasta; no direct player interaction allowed
                return false;
            case PotState.SoggyPasta:
                // Click TomatoSauce, then SoggyPasta -> add sauce
                return obj1.name == "TomatoSauce" && obj2.name == "SoggyPasta";
            case PotState.SaucePasta:
                // Click PastaSpoon, then SaucePasta -> stir, becomes Stir state
                return obj1.name == "PastaSpoon" && obj2.name == "SaucePasta";
            case PotState.Stir:
                // Final interaction: click Stir (or PastaSpoon representing stirred pasta) then Plate -> win
                return (obj1.name == "Stir" && obj2.name == "Plate") ||
                       (obj1.name == "PastaSpoon" && obj2.name == "Plate");
            default:
                return false;
        }
    }

    private void UpdatePotState(GameObject obj1, GameObject obj2)
    {
        if (currentPotState == PotState.Empty && obj1.name == "Pasta" && obj2.name == "Pot")
        {
            currentPotState = PotState.RawPasta;
            Debug.Log("Pot State: RawPasta");
            StartCoroutine(ChangePotStateAfterDelay(PotState.SoggyPasta, 2f));
        }
        else if (currentPotState == PotState.SoggyPasta && obj1.name == "TomatoSauce" && obj2.name == "SoggyPasta")
        {
            currentPotState = PotState.SaucePasta;
            Debug.Log("Pot State: SaucePasta");
        }
        else if (currentPotState == PotState.SaucePasta && obj1.name == "PastaSpoon" && obj2.name == "SaucePasta")
        {
            currentPotState = PotState.Stir;
            Debug.Log("Pot State: Stir");
        }
    }

    private System.Collections.IEnumerator ChangePotStateAfterDelay(PotState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        currentPotState = newState;
        Debug.Log($"Pot State updated to: {currentPotState}");
    }

    private bool IsWinningInteraction(GameObject obj1, GameObject obj2)
    {
        // Only allow the ordered final interaction when pot state is Stir
        if (currentPotState != PotState.Stir)
            return false;

        return (obj1.name == "Stir" && obj2.name == "Plate") ||
               (obj1.name == "PastaSpoon" && obj2.name == "Plate");
    }

    private void ShowWinningScreen()
    {
        Debug.Log("Complete");
        // Here load a new scene that represents the winning screen or display a UI overlay
        SceneManager.LoadScene("Win"); 
    }

    private void ResetClickStates()
    {
        firstClickedObject = null; // Reset first click
        secondClickedObject = null; // Reset second click
    }
}

