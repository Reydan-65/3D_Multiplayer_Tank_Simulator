using UnityEngine;

[System.Serializable]
public class ParameterCurve
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float duration = 1;

    private float expiredTime;

    public float MoveToward(float deltaTime)
    {
        expiredTime += deltaTime;

        return curve.Evaluate(expiredTime / duration);
    }

    public float Reset()
    {
        expiredTime = 0;

        return curve.Evaluate(0);
    }

    public float GetValueBetween(float startValue, float endValue, float currentValue)
    {
        if (curve.length == 0 || startValue == endValue) return 0;

        float startTime = curve.keys[0].time;
        float endTime = curve.keys[curve.length - 1].time;

        float currentTime = Mathf.Lerp(startTime, endTime, (currentValue - startValue) / (endValue - startValue));

        return curve.Evaluate(currentTime);
    }
}
