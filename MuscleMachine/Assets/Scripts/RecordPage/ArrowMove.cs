using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMove : MonoBehaviour
{
    private float R_Ceiling = 3.6f;
    private float R_Floor = -2.4f;

    private void Start()
    {
        R_Ceiling = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height - 100)).y;
        R_Floor = Camera.main.ScreenToWorldPoint(new Vector3(0, 100)).y;
    }
    private void OnMouseDrag()
    {
        //Debug.Log("拖拽:" + Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(pos.y>= R_Floor && pos.y <= R_Ceiling)
        {
            transform.parent.GetComponentInChildren<AudioVisualization>().OffsetY = pos.y;
            transform.position = new Vector3(transform.position.x, pos.y, 0);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        }
    }
}
