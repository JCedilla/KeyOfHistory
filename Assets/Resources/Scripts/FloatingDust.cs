using UnityEngine;
using UnityEngine.UI;

public class FloatingDust : MonoBehaviour
{
    [Header("Dust Settings")]
    public float minSpeed = 10f;
    public float maxSpeed = 30f;
    public float minRotationSpeed = 5f;
    public float maxRotationSpeed = 20f;
    public float minFloatAmount = 20f;
    public float maxFloatAmount = 50f;
    
    [Header("Glow Effect")]
    public bool enableGlow = true;
    public float glowSpeed = 1f;
    public float minGlow = 0.3f;
    public float maxGlow = 0.8f;
    
    private Image[] dustImages;
    private Vector2[] startPositions;
    private float[] speeds;
    private float[] rotationSpeeds;
    private float[] floatOffsets;
    private float[] floatAmounts;
    private float[] glowOffsets;
    private Color[] originalColors;
    
    void Start()
    {
        // Get all dust images
        dustImages = GetComponentsInChildren<Image>();
        int count = dustImages.Length;
        
        startPositions = new Vector2[count];
        speeds = new float[count];
        rotationSpeeds = new float[count];
        floatOffsets = new float[count];
        floatAmounts = new float[count];
        glowOffsets = new float[count];
        originalColors = new Color[count];
        
        // Initialize each dust particle with random values
        for (int i = 0; i < count; i++)
        {
            RectTransform rt = dustImages[i].GetComponent<RectTransform>();
            startPositions[i] = rt.anchoredPosition;
            speeds[i] = Random.Range(minSpeed, maxSpeed);
            rotationSpeeds[i] = Random.Range(minRotationSpeed, maxRotationSpeed) * (Random.value > 0.5f ? 1 : -1);
            floatOffsets[i] = Random.Range(0f, 100f);
            floatAmounts[i] = Random.Range(minFloatAmount, maxFloatAmount);
            glowOffsets[i] = Random.Range(0f, 100f);
            originalColors[i] = dustImages[i].color;
        }
    }
    
    void Update()
    {
        for (int i = 0; i < dustImages.Length; i++)
        {
            RectTransform rt = dustImages[i].GetComponent<RectTransform>();
            
            // Floating motion (sine wave)
            float floatX = Mathf.Sin((Time.time + floatOffsets[i]) * speeds[i] * 0.1f) * floatAmounts[i];
            float floatY = Mathf.Cos((Time.time + floatOffsets[i]) * speeds[i] * 0.08f) * floatAmounts[i];
            
            // Slow upward drift
            float drift = Time.time * speeds[i] * 0.5f;
            
            rt.anchoredPosition = startPositions[i] + new Vector2(floatX, floatY + drift);
            
            // Rotation
            rt.Rotate(0, 0, rotationSpeeds[i] * Time.deltaTime);
            
            // Glow pulsing effect
            if (enableGlow)
            {
                float glow = Mathf.Lerp(minGlow, maxGlow, 
                    (Mathf.Sin((Time.time + glowOffsets[i]) * glowSpeed) + 1f) / 2f);
                
                Color glowColor = originalColors[i];
                glowColor.a = originalColors[i].a * glow;
                dustImages[i].color = glowColor;
            }
            
            // Reset position if drifted too far up
            if (rt.anchoredPosition.y > Screen.height / 2 + 100)
            {
                startPositions[i] = new Vector2(
                    Random.Range(-Screen.width / 2, Screen.width / 2),
                    -Screen.height / 2 - 100
                );
            }
        }
    }
}