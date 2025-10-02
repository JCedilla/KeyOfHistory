using UnityEngine;

public class RotatingKey : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(25f, 25f, 25f); // degrees per second

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
