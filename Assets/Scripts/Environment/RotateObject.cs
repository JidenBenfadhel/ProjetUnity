using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Configuration")]
    public float rotationSpeed = 50f;
    public Vector3 rotationAxis = Vector3.forward; 

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}