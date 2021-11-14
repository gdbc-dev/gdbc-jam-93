using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanningCameraMovement : MonoBehaviour
{
    [SerializeField] private float keyScrollSpeed;
    [SerializeField] private float zoomSpeed;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // KEY SCROLLING
        var forwardBack = Input.GetAxisRaw("Vertical");
        transform.Translate(forwardBack * Vector3.up  * keyScrollSpeed *
            Time.unscaledDeltaTime);

        if (forwardBack > 0)
        {
            Debug.Log(forwardBack);
        }

        var leftRight = Input.GetAxisRaw("Horizontal");
        transform.Translate(leftRight * Vector3.right * keyScrollSpeed *
            Time.unscaledDeltaTime);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -10, 150), transform.position.y, Mathf.Clamp(transform.position.z, -10, 150));

        // KEY TURNING
        /*
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(new Vector3(0, -1, 0) * keyTurnSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(new Vector3(0, 1, 0) * keyTurnSpeed * Time.deltaTime);
        }
        */

        // MOUSE ZOOM
        var mouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        var adjustment = -1 * mouseScroll * zoomSpeed * Time.unscaledDeltaTime;
        cam.orthographicSize += adjustment;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 10, 110);
    }
}
