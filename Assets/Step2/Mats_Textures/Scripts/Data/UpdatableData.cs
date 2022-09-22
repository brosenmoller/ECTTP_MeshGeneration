using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    private void OnValidate()
    {
        if (autoUpdate)
        {
            NotifyOfUpdatedValues();
        }
    }

    public void NotifyOfUpdatedValues()
    {
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }
}
