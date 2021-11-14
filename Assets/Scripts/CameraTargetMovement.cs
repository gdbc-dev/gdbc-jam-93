using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTargetMovement : MonoBehaviour
{
    [SerializeField] private float keyScrollSpeed;
    [SerializeField] private float keyTurnSpeed;
    [SerializeField] private float zoomSpeed;
    
    Transform cam;
    CinemachineVirtualCamera actionVcam;
    CinemachineOrbitalTransposer actionTransposer;

    // Start is called before the first frame update
    void Awake()
    {
        cam = GameObject.Find("Action Camera").transform;
        actionVcam = cam.transform.Find("Action Virtual Camera").
            GetComponent<CinemachineVirtualCamera>();
        actionTransposer = actionVcam.
            GetCinemachineComponent<CinemachineOrbitalTransposer>();
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
        var adjustment = -1 * mouseScroll * zoomSpeed * Time.deltaTime;
        actionTransposer.m_FollowOffset.y += adjustment;
    }
}
