using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlFunctions : MonoBehaviour
{
    public Vector2 CameraSpeed;
    public float CameraWaitDuration;

    public void SetCameraMovement(Camera c)
    {
        CameraMovement.instance.SetCameraBehaviour(CameraMovement.instance.MoveConstant(CameraSpeed));
    }

    public void SetCameraWaitThenMove(Camera c)
    {
        CameraMovement.instance.SetCameraBehaviour(CameraMovement.instance.WaitThenDo(CameraWaitDuration, () => CameraMovement.instance.MoveConstant(CameraSpeed)));
    }
}