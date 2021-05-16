using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollider : MonoBehaviour
{
    GameObject colliderObj;
    // Start is called before the first frame update
    void Start()
    {
        colliderObj = GameObject.FindGameObjectWithTag("CameraCollider");
        GetComponent<PolygonCollider2D>().offset = colliderObj.GetComponent<PolygonCollider2D>().offset;
        GetComponent<PolygonCollider2D>().points = colliderObj.GetComponent<PolygonCollider2D>().points;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
