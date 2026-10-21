using UnityEngine;

public class Test_ARBH : MonoBehaviour
{
    public GameObject VisualizationObject;

   public void FoundByTarget()
    {
        VisualizationObject.SetActive(true);
    }

    public void LostByTarget()
    {
        VisualizationObject.SetActive(false);
    }
}
