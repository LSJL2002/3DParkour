using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public string chestName = "Chest";

    public string GetInteractPrompt()
    {
        return $"Press [E] to open {chestName}";
    }

    public void OnInteract()
    {
        Debug.Log($"{chestName} opened!");
    }
}
