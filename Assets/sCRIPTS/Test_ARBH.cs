using UnityEngine;

public class Test_ARBH : MonoBehaviour
{
    public GameObject VisualizationObject;

   public void FoundByTarget()
    {
        if (VisualizationObject != null)
            VisualizationObject.SetActive(true);
    }

    public void LostByTarget()
    {
        if (VisualizationObject != null)
            VisualizationObject.SetActive(false);
    }
}
