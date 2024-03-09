using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainUpdate : MonoBehaviour
{
    [SerializeField]
    DeviceContext dc;

    [SerializeField]
    GameLogic gameLogic;

    [SerializeField]
    float TickInterval = 1f;

    KeyCode[] keys = Enum.GetValues(typeof(KeyCode)).ConvertTo<KeyCode[]>();

    Coroutine tickRoutine;

    bool redraw = false;

    private void Awake()
    {
        StartTickRoutine();
    }

    void StartTickRoutine()
    {
        StopTickRoutine();
        tickRoutine = StartCoroutine(TickRoutine());
    }

    void StopTickRoutine()
    {
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }
    }

    IEnumerator TickRoutine()
    {
        float acc = TickInterval + 1f;
        while (true)
        {
            acc += Time.deltaTime;
            if (acc > TickInterval)
            {
                if (false == gameLogic.OnTick(acc, dc, ref redraw))
                {
                    GameOver();
                    yield return null;
                }
                acc = 0f;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void GameOver()
    {
        StopTickRoutine();
    }

    private void Update()
    {
        if (tickRoutine != null)
        {
            foreach (KeyCode key in keys)
            {
                if (Input.GetKeyDown(key))
                {
                    gameLogic.OnKeydown(key, dc, ref redraw);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (redraw)
        {
            dc.Redraw();
            redraw = false;
        }
    }
}
