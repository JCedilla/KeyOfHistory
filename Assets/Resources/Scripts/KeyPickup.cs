using UnityEngine;
using KeyOfHistory.Manager;

public class KeyPickup : InteractableObject
{
    public override string GetPromptText()
    {
        return "[E] Pick Up Key";
    }
    
    public override void Interact()
    {
        Debug.Log("Key picked up!");
        
        // Trigger portal spawn
        PortalSpawner spawner = FindFirstObjectByType<PortalSpawner>();
        if (spawner != null)
        {
            spawner.SpawnPortal();
        }
        
        // TODO: Add to inventory later
        
        Destroy(gameObject);
    }
}