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
        var forwardBack = Input.GetAxis("Vertical");
        transform.Translate(forwardBack * Vector3.forward  * keyScrollSpeed *
            Time.unscaledDeltaTime);

        var leftRight = Input.GetAxis("Horizontal");
        transform.Translate(leftRight * Vector3.right * keyScrollSpeed *
            Time.unscaledDeltaTime);

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
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        var adjustment = -1 * mouseScroll * zoomSpeed * Time.unscaledDeltaTime;
        cam.orthographicSize += adjustment;
    }
}
