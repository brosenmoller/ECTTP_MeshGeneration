using UnityEngine;

[CreateAssetMenu(fileName = "Noise Data", menuName = "Data/NoiseData", order = 3)]
public class NoiseData : UpdatableData
{
    [Header("Perlin Noise")]
    [Range(50, 500)] public float scale;
    [Range(1, 10)] public int octaves;
    [Range(0, 1f)] public float persistence;
    [Range(1, 10)] public float lacunarity;

    [Header("Height")]
    [Range(0, 100)] public float maxHeigth;
    public AnimationCurve heigthCurve;
}
