using UnityEngine;

public class ComputeCaller : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;

    [ContextMenu("Dispatch")]
    public void DispatchShader()
    {
        computeShader.Dispatch(0, 8, 8, 8);
    }
}
