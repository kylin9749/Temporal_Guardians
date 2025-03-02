using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class VirturalCameraControl : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    private float targetFiledView = 50; //默认相机缩放
    private float fieldOfViewMax = 100; //最大相机缩放
    private float fieldOfViewMin = 10;  //最小相机缩放
    private bool dragPanActive = false; //是否正在使用右键拖拽
    private Vector2 lastDragPosition;
    
    // Update is called once per frame
    void Update()
    {
        HandleCameraMovementDragPan();
        HandleCamreaZoom();

    }

    private void HandleCameraMovementDragPan()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetMouseButtonDown(1))
        {
            dragPanActive = true;
            lastDragPosition = Input.mousePosition;
            Debug.Log("dragPanActive" + lastDragPosition);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            dragPanActive = false;
        }

        if (dragPanActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastDragPosition;

            float dragPanSpeed = 2f;
            inputDir.x = mouseMovementDelta.x * dragPanSpeed;
            inputDir.y = mouseMovementDelta.y * dragPanSpeed;

            lastDragPosition = Input.mousePosition;
        }

        Vector3 moveDir = transform.forward * inputDir.y + transform.right * inputDir.x;
        float moveSpeed = 10f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleCamreaZoom()
    {
        if (Input.mouseScrollDelta.y > 0) 
        {
            targetFiledView -= 5;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            targetFiledView += 5;
        }

        targetFiledView = Mathf.Clamp(targetFiledView, fieldOfViewMin, fieldOfViewMax);

        float zoomSpeed = 10f;
        cinemachineVirtualCamera.m_Lens.OrthographicSize =
            Mathf.Lerp(cinemachineVirtualCamera.m_Lens.FieldOfView, targetFiledView, Time.deltaTime + zoomSpeed);
    }
}
