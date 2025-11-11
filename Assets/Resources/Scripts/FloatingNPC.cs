using UnityEngine;

public class FloatingNPC : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float FloatSpeed = 1f;
    [SerializeField] private float FloatHeight = 0.3f;
    [SerializeField] private float RotationSpeed = 20f;
    
    [Header("Pulse Settings")]
    [SerializeField] private Light[] PulseLights;
    [SerializeField] private float PulseSpeed = 2f;
    [SerializeField] private float MinIntensity = 2f;
    [SerializeField] private float MaxIntensity = 4f;
    
    private Vector3 _startPos;
    
    void Start()
    {
        _startPos = transform.position;
    }
    
    void Update()
    {
        // Gentle floating up and down
        float newY = _startPos.y + Mathf.Sin(Time.time * FloatSpeed) * FloatHeight;
        transform.position = new Vector3(_startPos.x, newY, _startPos.z);
        
        // Slow rotation
        transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        
        // Pulsing lights
        if (PulseLights.Length > 0)
        {
            float intensity = Mathf.Lerp(MinIntensity, MaxIntensity, (Mathf.Sin(Time.time * PulseSpeed) + 1f) / 2f);
            foreach (Light light in PulseLights)
            {
                if (light != null)
                    light.intensity = intensity;
            }
        }
    }
}