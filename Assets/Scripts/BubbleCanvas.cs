using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BubbleCanvas : MonoBehaviour
{
    public TextMeshProUGUI text;
    public GameObject container;

    public float lifeTime;
    public Canvas myCanvas;
    private Camera _camera;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (container.activeSelf)
        {
            lifeTime -= Time.deltaTime;

            if (lifeTime <= 0)
            {
                container.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (myCanvas)
        {
            if (!_camera)
            {
                _camera = Camera.main;
                return;
            }
            myCanvas.transform.LookAt(myCanvas.transform.position +
                                      _camera.transform.rotation * Vector3.forward,
                _camera.transform.rotation * Vector3.up);
        }
    }

    public void setDialog(string text, float lifetime)
    {
        container.SetActive(true);
        this.lifeTime = lifetime;
        this.text.text = text;
    }
}