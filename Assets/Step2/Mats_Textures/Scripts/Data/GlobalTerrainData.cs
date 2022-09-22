using UnityEngine;

[CreateAssetMenu(fileName = "Terrain Data", menuName = "Data/TerrainData", order = 3)]
public class GlobalTerrainData : UpdatableData
{
    public float uniformScale = 1f;
    public bool useFlatShading;
    public float waterLevel;
}
