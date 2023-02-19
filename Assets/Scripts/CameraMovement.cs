using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Coroutine _currentRoutine;
    private Rigidbody2D _rb;

    public static CameraMovement instance;

    private bool _idlySpedUp = false;
    private static Vector2 _velocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = _velocity;
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
    }

    private void Update()
    {
        _rb.velocity = _velocity;

        if (_idlySpedUp)
        {
            _rb.velocity *= 4;
        }

        Manager.instance.SetBackgroundSpeed(_rb.velocity);
    }

    public void SlowDown()
    {
        if (_idlySpedUp)
        {
            _idlySpedUp = false;
        }
    }

    public void SpeedUp()
    {
        if (!_idlySpedUp)
        {
            _idlySpedUp = true;
        }
    }

    public IEnumerator MoveConstant(Vector2 speed)
    {
        SlowDown();
        Vector2 startVelocity = _velocity;
        float seconds = 0;
        while (seconds < 1)
        {
            seconds += Manager.deltaTime;
            _velocity = startVelocity + seconds * (speed - startVelocity);
            yield return null;
        }
        _velocity = speed;
    }

    public IEnumerator WaitThenDo(float waitDuration, Action postWaitAction)
    {
        SlowDown();
        _velocity = Vector2.zero;
        yield return new WaitForSeconds(waitDuration);
        postWaitAction();
    }

    public void SetCameraBehaviour(IEnumerator newBehaviour)
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
        }
        SlowDown();
        _currentRoutine = StartCoroutine(newBehaviour);
    }
}