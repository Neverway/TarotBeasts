using System;
using UnityEngine;

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
        GameFuncs.ResetRegisteredProcessing();
    }

    [ContextMenu("Add Reverse Rotate Rule")]
    public void AddReverseRotateRule() =>
        GameFuncs.RegisterInputProcessing<Quaternion, Quaternion>(nameof(GameFuncs.RotateThing), ReverseRotationRule);
    public Tuple<Quaternion, Quaternion> ReverseRotationRule(Quaternion oldQuaternion, Quaternion rotateBy)
    {
        rotateBy = Quaternion.Inverse(rotateBy);

        return new(oldQuaternion, rotateBy);
    }



    [ContextMenu("Add Double Rotate Rule")]
    public void AddDoubleRotateRule() =>
        GameFuncs.RegisterInputProcessing<Quaternion, Quaternion>(nameof(GameFuncs.RotateThing), DoubleRotationRule);
    public Tuple<Quaternion, Quaternion> DoubleRotationRule(Quaternion oldQuaternion, Quaternion rotateBy)
    {
        rotateBy *= rotateBy;

        return new(oldQuaternion, rotateBy);
    }

    [ContextMenu("Add Cancel Rotate Rule")]
    public void AddCancelRotateRule() =>
        GameFuncs.RegisterOutputProcessing<Quaternion, Quaternion, Quaternion>(nameof(GameFuncs.RotateThing), CancelRotationRule);
    public Quaternion CancelRotationRule(Quaternion oldQuaternion, Quaternion rotateBy, Quaternion output)
    {
        return oldQuaternion;
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