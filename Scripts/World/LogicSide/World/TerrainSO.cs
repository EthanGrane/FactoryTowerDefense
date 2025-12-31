using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTerrain", menuName = "FACTORY/Terrain")]
public class TerrainSO : ScriptableObject
{
    public string terrainName;
    public bool solid;
    public TileBase sprite;
    [Header("Pathfinding")]
    public int movementCost = 1;

    [Header("3D Visual")] 
    public GameObject terrainTile;
    public GameObject terrainOrnament;
}