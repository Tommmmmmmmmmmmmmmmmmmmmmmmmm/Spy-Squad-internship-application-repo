using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using System;

public class FloatSpringHandler : ArbitraryValueSpringHandler<FloatSpring, float>
{
    protected override float ValueToTarget { get { return myValue; }  set { myValue = value; } }
    protected override bool IShouldContinueSpringing => Mathf.Abs(myValue - AToBSpring.EndValue) > .005f || !AToBSpring.IsSpringVelocityNearZero();

    protected override bool IShouldPrintDebugStatements { get { return false; } }

    protected override void CalculateOutput(FloatSpring mySpring)
    {
        if (output == null) return;

        OnSpringUpdate?.Invoke(myValue);
        float distance = Mathf.Abs(ValueToTarget - mySpring.EndValue);
        OutputData(distance);
    }
}