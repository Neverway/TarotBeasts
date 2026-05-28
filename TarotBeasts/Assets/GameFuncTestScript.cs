using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameFuncTestScript : MonoBehaviour
{
    public Transform rotateRef;

    public void Awake()
    {
        ResetRules();
    }

    public void FixedUpdate()
    {
        transform.localRotation = GameFuncs.RotateThing(transform.localRotation, rotateRef.localRotation);
    }


    [ContextMenu("Reset")]
    public void ResetRules()
    {
        GameFuncs.ResetRegisteredProcessors();
    }

    [ContextMenu("Add Reverse Rotate Rule")]
    public void AddReverseRotateRule() =>
        GameFuncs.NewInputProcessor<Quaternion, Quaternion>(nameof(GameFuncs.RotateThing), ReverseRotationRule).Register();
    public void ReverseRotationRule(ref Quaternion oldQuaternion, ref Quaternion rotateBy)
    {
        rotateBy = Quaternion.Inverse(rotateBy);
    }

    [ContextMenu("Add Double Rotate Rule")]
    public void AddDoubleRotateRule() =>
        GameFuncs.NewInputProcessor<Quaternion, Quaternion>(nameof(GameFuncs.RotateThing), DoubleRotationRule).Register();
    public void DoubleRotationRule(ref Quaternion oldQuaternion, ref Quaternion rotateBy)
    {
        rotateBy *= rotateBy;
    }

    [ContextMenu("Add Cancel Rotate Rule")]
    public void AddCancelRotateRule() =>
        GameFuncs.NewOutputProcessor<Quaternion, Quaternion, Quaternion>(nameof(GameFuncs.RotateThing), CancelRotationRule).Register();
    public void CancelRotationRule(Quaternion oldQuaternion, Quaternion rotateBy, ref Quaternion output)
    {
        output = oldQuaternion;
    }
}




public static partial class GameFuncs
{
    public static Quaternion RotateThing(Quaternion oldQuaternion, Quaternion rotateBy)
    {
        ProcessInput(ref oldQuaternion, ref rotateBy);

        
        Quaternion newRotation = oldQuaternion * rotateBy;

        return ProcessOutput(oldQuaternion, rotateBy, newRotation);
    }
}