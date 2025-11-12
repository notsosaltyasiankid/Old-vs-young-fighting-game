using UnityEngine;

public class SimpleFalling : MonoBehaviour
{
    public float fallSpeed = 5f;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }
}
