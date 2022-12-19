using Godot;
using System;

public class TriRoughnessStat : FloatStat
{
    protected override float GetOutValue(float inValue)
    {
        if (inValue < 0f) inValue = 0f;
        return StatUtility.GetPseudoAsymptotic(inValue, .5f, 1f);
    }
}
