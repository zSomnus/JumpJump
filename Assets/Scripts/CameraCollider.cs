using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollider : MonoBehaviour
{
    GameObject cameraCollider;
    // Start is called before the first frame update
    void Start()
    {
        //SetCollider();
    }

    public void SetCollider()
    {
        cameraCollider = GameObject.FindGameObjectWithTag("CameraCollider");
        GetComponent<PolygonCollider2D>().offset = cameraCollider.GetComponent<PolygonCollider2D>().offset;
        GetComponent<PolygonCollider2D>().points = cameraCollider.GetComponent<PolygonCollider2D>().points;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
