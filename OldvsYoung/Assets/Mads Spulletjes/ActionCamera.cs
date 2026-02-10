using System.Collections.Generic;
using UnityEngine;

public class ActionCamera : MonoBehaviour
{
    public List<Transform> players = new List<Transform>();

    public float zoomMultiplier = 1f; // hoe sterk zoom reageert
    public float minZoom = 5f;
    public float maxZoom = 25f;
    public float smoothSpeed = 5f;

    private Camera cam;

    [Header("Camera Bounds")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;


    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (players.Count == 0) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 center = GetCenterPoint();

        Vector3 targetPos = new Vector3(
            center.x,
            center.y,
            transform.position.z
        );

        // Clamp camera inside boundaries
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothSpeed
        );
    }


    void ZoomCamera()
    {
        float distance = GetGreatestDistance();

        float targetZoom;

        if (distance < 0.5f) // spelers bijna tegen elkaar
            targetZoom = minZoom;
        else
            targetZoom = distance * zoomMultiplier;

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * smoothSpeed
        );
    }


    Vector3 GetCenterPoint()
    {
        Bounds bounds = new Bounds(players[0].position, Vector3.zero);

        foreach (Transform player in players)
            bounds.Encapsulate(player.position);

        return bounds.center;
    }

    float GetGreatestDistance()
    {
        if (players.Count < 2) return 0f;

        float maxDistance = 0f;

        for (int i = 0; i < players.Count; i++)
        {
            for (int j = i + 1; j < players.Count; j++)
            {
                float dist = Vector2.Distance(
                    players[i].position,
                    players[j].position
                );

                if (dist > maxDistance)
                    maxDistance = dist;
            }
        }

        return maxDistance;
    }
}
