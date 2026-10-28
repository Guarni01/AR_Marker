using UnityEngine;

public class Test_RayFromCam : MonoBehaviour
{
    public Camera ArCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))

        {
            RaycastHit hit;
            Ray fingerRay = ArCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(fingerRay, out hit))
            {
                if(hit.collider.gameObject == gameObject)
                {
                    GetComponent<Renderer>().material.color = Random.ColorHSV();
                }
            }

        }

    }
}
