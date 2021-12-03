using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Face : MonoBehaviour
{
    public abstract void SetExpression(Expression expression, float intensity);

    public abstract Expression GetCurrentExpression();

    public abstract void SetVisible(bool v);

    public abstract void SetWomanHair();
    public abstract void SetManHair();

    public abstract bool isWoman();

    public void ReplicateGenderFrom(Face source)
    {
        if (isWoman() != source.isWoman())
        {
            if (source.isWoman())
                SetWomanHair();
            else
                SetManHair();
        }
    }

}
