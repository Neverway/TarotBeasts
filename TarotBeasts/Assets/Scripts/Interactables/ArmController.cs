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
    [SerializeField] private bool isCPUControlled = false;
    [SerializeField] private float CPUMoveSpeed = 8f;
    [SerializeField] private Vector2 minXZ;
    [SerializeField] private Vector2 maxXZ;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private float startY;
    private Vector3 cpuTargetPos;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Camera viewCamera;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        startY = transform.position.y;
        if (!isCPUControlled) Cursor.visible = false;
    }

    private void Update()
    {
        if (isCPUControlled)
        {
            Vector3 target = new Vector3(cpuTargetPos.x, transform.position.y, cpuTargetPos.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * CPUMoveSpeed);
            return;
        }
        
        Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            transform.position = new Vector3(Mathf.Clamp(worldPos.x, minXZ.x, maxXZ.x), transform.position.y, Mathf.Clamp(worldPos.z, minXZ.y, maxXZ.y));
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

    private IEnumerator TravelThenPunch()
    {
        while (true)
        {
            Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatTarget = new Vector3(cpuTargetPos.x, 0, cpuTargetPos.z);
            if (Vector3.Distance(flatCurrent, flatTarget) < 0.05f) break;
            yield return null;
        }
        PunchDown();
        yield return new WaitForSeconds(0.125f);
        PunchUp();
        yield return new WaitForSeconds(clickUpDuration);
        cpuTargetPos = Vector3.zero;
        while (true)
        {
            Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatTarget = new Vector3(cpuTargetPos.x, 0, cpuTargetPos.z);
            if (Vector3.Distance(flatCurrent, flatTarget) < 0.05f) break;
            yield return null;
        }
        gameObject.SetActive(false);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void MoveCPUToTarget(Vector3 worldPosition)
    {
        cpuTargetPos = worldPosition;
        StopAllCoroutines();
        StartCoroutine(TravelThenPunch());
    }


    #endregion
}