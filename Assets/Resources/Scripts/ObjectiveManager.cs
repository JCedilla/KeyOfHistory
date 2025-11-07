using UnityEngine;
using TMPro;

namespace KeyOfHistory.UI
{
    public class ObjectiveManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject ObjectivePanel;
        [SerializeField] private TextMeshProUGUI ObjectiveText;
        
        [Header("Settings")]
        [SerializeField] private string ObjectivePrefix = "📋 Objective: ";
        
        private void Start()
        {
            // Start with objective hidden or with default objective
            ObjectivePanel.SetActive(false);
        }
        
        public void UpdateObjective(string newObjective)
        {
            if (ObjectiveText != null)
            {
                ObjectiveText.text = ObjectivePrefix + newObjective;
                ObjectivePanel.SetActive(true);
            }
        }
        
        public void ClearObjective()
        {
            ObjectivePanel.SetActive(false);
        }
        
        public void CompleteObjective()
        {
            // Optional: Show completion feedback
            if (ObjectiveText != null)
            {
                ObjectiveText.text = "✓ Objective Complete!";
                Invoke(nameof(ClearObjective), 2f);
            }
        }
    }
}