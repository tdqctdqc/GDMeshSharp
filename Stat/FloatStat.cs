using Godot;
using System;

public abstract class FloatStat
{
    public float InValue { get; private set; }
        = 0f;
    public float OutValue { get; private set; }
        = 0f;
    protected abstract float GetOutValue(float inValue);

    public void AddIn(float inValueIncrement)
    {
        InValue += inValueIncrement;
        OutValue = GetOutValue(InValue);
    }
}
