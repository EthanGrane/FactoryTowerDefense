using System.Collections.Generic;
using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{
    [Header("Pool")]
    public GameObject projectilePrefab;
    public int poolSize = 500;

    private List<Transform> projectilePool;
    private Stack<Transform> freePool;

    private Dictionary<Projectile, Transform> activeVisuals;

    private void Awake()
    {
        projectilePool = new List<Transform>(poolSize);
        freePool = new Stack<Transform>(poolSize);
        activeVisuals = new Dictionary<Projectile, Transform>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            Transform t = Instantiate(projectilePrefab, transform).transform;
            t.gameObject.SetActive(false);

            projectilePool.Add(t);
            freePool.Push(t);
        }
    }

    public void RegisterProjectile(Projectile projectile)
    {
        if (activeVisuals.ContainsKey(projectile))
            return;

        if (freePool.Count == 0)
        {
            Debug.LogWarning("ProjectileVisual pool exhausted");
            return;
        }

        Transform visual = freePool.Pop();
        visual.gameObject.SetActive(true);
        visual.position = projectile.position;

        activeVisuals.Add(projectile, visual);
    }

    public void UnregisterProjectile(Projectile projectile)
    {
        if (!activeVisuals.TryGetValue(projectile, out Transform visual))
            return;

        visual.gameObject.SetActive(false);
        freePool.Push(visual);

        activeVisuals.Remove(projectile);
    }
    
    private void Update()
    {
        foreach (var kvp in activeVisuals)
        {
            Projectile projectile = kvp.Key;
            Transform visual = kvp.Value;

            visual.position = projectile.position;
        }
    }
}
