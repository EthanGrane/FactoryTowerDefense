using System;
using UnityEngine;

public class PathfindingVisual : MonoBehaviour
{
    public LineRenderer lineRenderer;

    private void Start()
    {
        EnemyManager.Instance.OnPathUpdated += () =>
        {
            UpdatePathHint();
        };
    }

    void UpdatePathHint()
    {
        Vector2[] path = EnemyManager.Instance.GetPath().ToArray();
        lineRenderer.positionCount = path.Length;
        
        Vector3[] points = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
            points[i] = new Vector3(path[i].x, path[i].y, 0);
        
        lineRenderer.SetPositions(points);
    }
}
