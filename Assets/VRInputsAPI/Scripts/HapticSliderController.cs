using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRInputsAPI;

public class HapticSliderController
{
    private float oldValue = 0;
    private float threshold;
    private (float, float)[] triggers;

    public HapticSliderController((float, float)[] triggersIntensityMap, float threshold)
    {
        this.threshold = threshold;
        triggers = triggersIntensityMap;
    }
    

    public void Reset(float newValue)
    {
        oldValue = newValue;
    }


    public void Update(VRController ctrl, float newValue)
    {
        float minV = Mathf.Min(oldValue, newValue);
        float maxV = Mathf.Max(oldValue, newValue);
        foreach ((float trigger, float intensity) in triggers)
        {
            if (minV < trigger - threshold && maxV > trigger + threshold)
            {
                ctrl.TriggerHapticImpulse(0.01f, intensity);
                oldValue = newValue;
            }
        }
    }

}
