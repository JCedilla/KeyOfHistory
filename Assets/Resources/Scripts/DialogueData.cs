using UnityEngine;

namespace KeyOfHistory.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("NPC Info")]
        public string NPCName = "Mysterious Figure";
        
        [Header("Dialogue Lines")]
        public DialogueLine[] Lines;
        
        [Header("Events (Optional)")]
        public bool SpawnObjectAfterDialogue = false;
        public GameObject ObjectToSpawn;
        public Vector3 SpawnOffset = Vector3.forward;
        
        [System.Serializable]
        public class DialogueLine
        {
            [TextArea(2, 4)]
            public string Text;
            public AudioClip VoiceClip;
        }
    }
}