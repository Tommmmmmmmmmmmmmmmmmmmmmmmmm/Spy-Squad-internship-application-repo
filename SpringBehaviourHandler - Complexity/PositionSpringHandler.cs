using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PositionSpringHandler : SpringBehaviourHandler<Vector2Spring, Vector2>
{
    protected override Vector2 ValueToTarget
    {
        get => TryGetComponent(out RectTransform GetRectTransform) ? GetRectTransform.anchoredPosition : transform.localPosition;
        set
        {
            if (TryGetComponent(out RectTransform SetRectTransform)) SetRectTransform.anchoredPosition = value;
            else transform.localPosition = value;
        }
    }
    protected override bool IShouldContinueSpringing => Vector2.Distance(ValueToTarget, AToBSpring.EndValue) > .001f || !AToBSpring.IsSpringVelocityNearZero();

    protected override bool IShouldPrintDebugStatements { get { return false; } }

    protected override void CalculateOutput(Vector2Spring mySpring)
    {
        if (output == null) return;

        float distance = Vector2.Distance(ValueToTarget, mySpring.EndValue);
        OutputData(distance);
    }
}