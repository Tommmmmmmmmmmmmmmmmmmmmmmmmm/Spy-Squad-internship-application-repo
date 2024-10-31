using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public abstract class SpringBehaviourHandler<T, U> : MonoBehaviour where T : ExposedSpring<U>, new()
{
    #region Serialised values

    [OnValueChanged("RealignValues")]
    [MinValue(1)]
    [SerializeField] private float damping = 20;
    // I have these getters so that, in the inspector, damping, mass, and strength feel more balanced
    protected float Damping => damping * .5f;

    [OnValueChanged("RealignValues")]
    [MinValue(1)]
    [SerializeField] private float strength = 30;
    // I have these getters so that, in the inspector, damping, mass, and strength feel more balanced
    protected float Strength => strength * 5;

    [SerializeField] private bool updateSimulationSpeedAfterEachSpring;

    [OnValueChanged("RealignValues")]
    [SerializeField] private Vector2 springSimulationSpeedRandomness = new(1, 0);
    [ShowNonSerializedField]
    private float simulationSpeedMultiplier;
    private float SimulationDelta => Time.deltaTime * simulationSpeedMultiplier;

    /*
    [OnValueChanged("RealignValues")]
    [MinValue(0.1f)]
    [SerializeField] private float mass = 1;
    // I have these getters so that, in the inspector, damping, mass, and strength feel more balanced
    protected float Mass => mass;
    */

    //todo - auto collect components inherititing from processor abstract class and add to output list
    [SerializeField] private bool autoCollectOutputProcessors;

    [HideIf("autoCollectOutputProcessors")]
    [SerializeField] protected SpringOutputProcessor[] output;

    protected void RealignValues()
    {
        if (Application.isPlaying)
        {
            AToBSpring.Damping = Damping;
            AToBSpring.Strength = Strength;

            ContinuousSpring.Damping = Damping;
            ContinuousSpring.Strength = Strength;

            simulationSpeedMultiplier = springSimulationSpeedRandomness.x + Random.Range(-springSimulationSpeedRandomness.y, springSimulationSpeedRandomness.y);
        }
    }

    #endregion

    #region Non-serialised values

    protected T AToBSpring;
    protected T ContinuousSpring;

    [ShowNonSerializedField]
    protected bool isNudging;
    [ShowNonSerializedField]
    protected bool isAToBSpringing;
    [ShowNonSerializedField]
    protected bool isSpringingContinuously;

    public delegate U SpringTargetDelegate();

    // Can be used to enable or disable printing in different child classes - very useful for debugging specific ones
    protected abstract bool IShouldPrintDebugStatements { get; }
    #endregion

    #region Setup

    private void Awake()
    {
        AToBSpring = new T
        {
            Damping = Damping,
            Strength = Strength,
        };

        AToBSpring.Setup(SpringMode.AToB);

        ContinuousSpring = new T
        {
            Damping = Damping,
            Strength = Strength,
        };

        ContinuousSpring.Setup(SpringMode.Continuous);
    }

    private void Start()
    {
        // Make sure all recivers are at their rest values. We do this in start to ensure that we dont enter any race conditions with recievers that set default values in awake
        AToBSpring.EndValue = ValueToTarget;
        ContinuousSpring.EndValue = ValueToTarget;

        CalculateOutput(AToBSpring);
        CalculateOutput(ContinuousSpring);
    }

    #endregion

    #region Utility functions for external triggering

    public void InturruptSpring()
    {
        isAToBSpringing = false;
        isNudging = false;
        isSpringingContinuously = false;
    }

    public void TriggerSpringOfType(SpringMode springMode, U value)
    {
        if (springMode == SpringMode.AToB)       TriggerSpringTo               (value);
        if (springMode == SpringMode.Continuous) StartSpringToContinuous(value);
    }

    public U GetSpringStartValue  (SpringMode springMode) => springMode == SpringMode.AToB ? AToBSpring.StartValue   : ContinuousSpring.StartValue;
    public U GetSpringCurrentValue(SpringMode springMode) => springMode == SpringMode.AToB ? AToBSpring.CurrentValue : ContinuousSpring.CurrentValue;
    public U GetSpringEndValue    (SpringMode springMode) => springMode == SpringMode.AToB ? AToBSpring.EndValue     : ContinuousSpring.EndValue;

    #endregion

    #region Springing stuff

    protected abstract U ValueToTarget { get; set; }

    private readonly CoroutineControl internalShouldStopAllCoroutines = CoroutineControl.StopAllCoroutines;

    #region Nudging

    //TODO: add support for nudging continuous springs. The continuous spring solver that I'm currently using bugs out if you try to nudge it, and the 
    // AToB solver cant be used for continuous springs, which means ill probably need to store the continuous spring velocity, cancel it,
    // add the nudge value to it, then restart the spring with that modified velocity

    public void TriggerNudge(U nudgeAmount)
    {
        HandleSpringTriggering(internalShouldStopAllCoroutines);

        if (isAToBSpringing || isNudging) HandleAToBSpringSetupForNudgeWhenIsNudgingOrSpringing(nudgeAmount);
        else
        {
            AToBSpring.Reset();
            HandleAToBSpringSetupForNudgeWhenNotNudgingOrSpringing(nudgeAmount);
        }

        StartCoroutine(HandleNudge());
    }

    protected virtual void HandleAToBSpringSetupForNudgeWhenIsNudgingOrSpringing(U nudgeAmount)
    {
        AToBSpring.InitialVelocity = nudgeAmount;
        AToBSpring.UpdateEndValue(AToBSpring.EndValue, nudgeAmount);
    }

    protected void HandleAToBSpringSetupForNudgeWhenNotNudgingOrSpringing(U nudgeAmount)
    {
        AToBSpring.StartValue = ValueToTarget;
        AToBSpring.EndValue = ValueToTarget;
        AToBSpring.InitialVelocity = nudgeAmount;
    }

    private IEnumerator HandleNudge()
    {
        HandleSpringEvaluationUltilisation(SpringMode.AToB);
        isNudging = true;

        while (IShouldContinueSpringing)
        {
            if (!isNudging) yield break;
            HandleSpringEvaluationUltilisation(SpringMode.AToB);
            yield return null;
        }

        isNudging = false;
        HandleSpringReachingItsDestination();
    }

    #endregion

    #region AToB Springing

    public void TriggerSpringTo(U targetValue)
    {
        HandleSpringTriggering(internalShouldStopAllCoroutines);

        if (isAToBSpringing || isNudging) HandleAToBSpringSetupForSpringingWhenIsNudgingOrSpringing(targetValue);
        else
        {
            AToBSpring.Reset();
            HandleAToBSpringSetupForSpringingWhenNotNudgingOrSpringing(targetValue);
        }

        StartCoroutine(DoSpringTo());
    }

    protected virtual void HandleAToBSpringSetupForSpringingWhenIsNudgingOrSpringing(U test)
    {
        AToBSpring.UpdateEndValue(test, AToBSpring.CurrentVelocity);
    }

    protected void HandleAToBSpringSetupForSpringingWhenNotNudgingOrSpringing(U targetValue)
    {
        AToBSpring.StartValue = ValueToTarget;
        AToBSpring.EndValue = targetValue;
    }

    private IEnumerator DoSpringTo()
    {
        isAToBSpringing = true;

        while (IShouldContinueSpringing)
        {
            if (!isAToBSpringing) yield break;

            HandleSpringEvaluationUltilisation(SpringMode.AToB);

            yield return null;
        }

        isAToBSpringing = false;
        HandleSpringReachingItsDestination();
    }

    #endregion

    #region Continuous Springing

    protected abstract bool IShouldContinueSpringing { get; }

    public void DoSpringToContinuous(U target)
    {
        HandleSpringTriggering(internalShouldStopAllCoroutines);
        ContinuousSpring.EndValue = target;
        HandleSpringEvaluationUltilisation(SpringMode.Continuous);
    }

    public void StartSpringToContinuous(U targetValue)
    {
        U ContinuousTarget() => targetValue;
        StartSpringToContinuous(ContinuousTarget);
    }

    public void StartSpringToContinuous(SpringTargetDelegate target)
    {
        HandleSpringTriggering(internalShouldStopAllCoroutines);
        StartCoroutine(HandleContinuousSpring(target));
    }

    private IEnumerator HandleContinuousSpring(SpringTargetDelegate target)
    {
        isSpringingContinuously = true;

        while (isSpringingContinuously)
        {
            ContinuousSpring.EndValue = target();
            HandleSpringEvaluationUltilisation(SpringMode.Continuous);
            yield return null;
        }
    }

    public void StopSpringToContinuous()
    {
        isSpringingContinuously = false;
    }

    #endregion

    #region Misc springing functions 

    protected virtual void HandleSpringEvaluationUltilisation(SpringMode springMode) => ValueToTarget = springMode == SpringMode.AToB ? EvaluateAToBSpring : EvaluateContinuousSpring;

    protected U EvaluateContinuousSpring
    {
        get
        {
            CalculateOutput(ContinuousSpring);
            return ContinuousSpring.Evaluate(SimulationDelta);
        }
    }

    protected U EvaluateAToBSpring
    {
        get
        {
            CalculateOutput(AToBSpring);
            return AToBSpring.Evaluate(SimulationDelta);
        }
    }

    // Is used by both AToB springing and nudging
    private void HandleSpringReachingItsDestination()
    {
        // We set the value to the end value as the springs almost always end when the values aren't exactly the same 
        ValueToTarget = AToBSpring.EndValue;
        AToBSpring.Reset();
        CalculateOutput(AToBSpring);
    }

    protected abstract void CalculateOutput(T mySpring);

    protected void OutputData(float value)
    {
        if (output.Length == 0) return;

        for (int i = 0; i < output.Length; i++)
        {
            output[i].RecieveInput(value);
        }
    }


    // Useful for debugging

    private void Print(object whatToPrint)
    {
        if (IShouldPrintDebugStatements) print(whatToPrint);
    }

    private void Print()
    {
        // Can be useful for printing the same value out in a bunch of different areas
        if (IShouldPrintDebugStatements) print(AToBSpring.CurrentValue);
    }

    #endregion

    private enum CoroutineControl { StopAllCoroutines, DontStopAllCoroutines }

    private void HandleSpringTriggering(CoroutineControl shouldStopAllCoroutines)
    {
        RealignValues();

        // Havent fully tested when coroutines need to be canceled
        if (shouldStopAllCoroutines == CoroutineControl.StopAllCoroutines) StopAllCoroutines();

        if (updateSimulationSpeedAfterEachSpring) simulationSpeedMultiplier = springSimulationSpeedRandomness.x + Random.Range(-springSimulationSpeedRandomness.y, springSimulationSpeedRandomness.y);
    }

    #endregion
}

public abstract class ArbitraryValueSpringHandler<T, U> : SpringBehaviourHandler<T, U> where T : ExposedSpring<U>, new()
{
    [SerializeField] protected UnityEvent<U> OnSpringUpdate;

    [ShowNonSerializedField]
    protected U myValue;
}