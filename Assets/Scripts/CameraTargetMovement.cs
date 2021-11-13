using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTargetMovement : MonoBehaviour
{
    [SerializeField] private float keyScrollSpeed;
    [SerializeField] private float keyTurnSpeed;
    [SerializeField] private float mouseScrollSpeed;
    [SerializeField] private float zoomSpeed;
    
    Transform cam;
    CinemachineVirtualCamera planningVcam, actionVcam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        planningVcam = cam.transform.Find("Planning Virtual Camera").
            GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        // KEY SCROLLING
        var dir = cam.transform.forward;
        dir.y = 0;
        dir.Normalize();
        transform.position += dir * Input.GetAxis("Vertical") * keyScrollSpeed * Time.deltaTime;

        var leftRight = Input.GetAxis("Horizontal");
        transform.Translate(leftRight * Vector3.right * keyScrollSpeed * Time.deltaTime);

        // KEY TURNING
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(new Vector3(0, -1, 0) * keyTurnSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(new Vector3(0, 1, 0) * keyTurnSpeed * Time.deltaTime);
        }

        // MOUSE ZOOM
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        //vcam.
    }
}
