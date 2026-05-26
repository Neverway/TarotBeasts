//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArmController : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private float clickDepth = -0.7f;
    [SerializeField] private float clickDownDuration = 0.1f;
    [SerializeField] private float clickUpDuration = 0.2f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private float startY;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Camera viewCamera;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        startY = transform.position.y;
        Cursor.visible = false;
    }

    private void Update()
    {
        /*Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = viewCamera.transform.position.y;
        Vector3 worldPos = viewCamera.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);*/
        
        Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
        }
        
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PunchDown();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            PunchUp();
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void PunchDown()
    {
        DOTween.Kill(transform);
        transform.DOMoveY(startY + clickDepth, clickDownDuration);
    }

    private void PunchUp()
    {
        DOTween.Kill(transform);
        transform.DOMoveY(startY, clickUpDuration);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}