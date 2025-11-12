using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 100f;

    [Tooltip("Rotation axis (X=1 for x-axis, Y=1 for y-axis, etc.)")]
    public Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
