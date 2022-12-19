using Godot;
using System;

public static class StatUtility 
{
    public static float GetPseudoAsymptotic(float inValue, float halfInput, float maxOutput)
    {
        var slope = .5f / halfInput;
        int iter = 0;
        float outValue = 0f;
        while (inValue > halfInput)
        {
            iter++;
            outValue += .5f / iter;
            inValue -= halfInput;
        }

        iter++;
        outValue += .5f * (inValue / halfInput) / iter;
        return outValue;
    }
}
