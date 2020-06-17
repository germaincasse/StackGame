using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetChild(0);
    }


    private void Update() {
        if (cam.transform.localPosition.y > 0)
            cam.transform.localPosition = Vector3.zero;
        else
            cam.transform.Translate(0, (transform.position.y - cam.transform.position.y) / 2, 0, Space.World);
    }

    public void Move() {
        transform.Translate(new Vector3(0, 1, 0), Space.World);
        cam.transform.Translate(new Vector3(0, -1, 0), Space.World);
    }
}
