using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public float pointsPerUnit;
    public LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Update()
    {
        float distance = Vector2.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
        float magnitude = pointsPerUnit * distance;
        lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(Time.timeSinceLevelLoad * 4f, 0f));
        lineRenderer.material.SetTextureScale("_MainTex", new Vector2(magnitude, 1f));
    }
}