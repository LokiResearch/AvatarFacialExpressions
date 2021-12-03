using System.Collections.Generic;
using UnityEngine;
using VRInputsAPI;

public class EmoTouch_ExprSelection_8 : MonoBehaviour
{
    public static string lastConfirmButtonUsed = "ButtonNone";

    public GameObject cursor;

    public GameObject menu;

    public Face mainFace;

    public Face feedbackFace;


    // Start is called before the first frame update
    void Start()
    {
        
    }


    void OnEnable()
    {
        Debug.Log("EmoTouch_ExprSelection_PadPress.OnEnable()");
    }

    void OnDisable()
    {
        Debug.Log("EmoTouch_ExprSelection_PadPress.OnDisable()");
        hovered = (Expression.NEUTRAL, 0);
        select = false;
        mainFace.SetExpression(Expression.NEUTRAL, 0);
        feedbackFace.SetExpression(Expression.NEUTRAL, 0);
    }


    
    private (Expression, float) hovered = (Expression.NEUTRAL, 0);

    private bool select = false;

    // Update is called once per frame
    void Update()
    {
        feedbackFace.ReplicateGenderFrom(mainFace);

        VRController ctrl = VRInputsManager.GetMainHandController();

        if (!ctrl.Available || !ctrl.HTCVive_PadTouch.Available)
        {
            cursor.SetActive(false);
            hovered = (Expression.NEUTRAL, 0);
            select = false;
            feedbackFace.SetExpression(hovered.Item1, hovered.Item2);
            mainFace.SetExpression(hovered.Item1, hovered.Item2);
            return;
        }

        VRBooleanInput padPressed = ctrl.HTCVive_PadPress;
        VRBooleanInput triggerPressed = ctrl.HTCVive_TriggerPress;

        // selection confirmation
        if (padPressed.Down || triggerPressed.Down)
        {
            select = true;
            cursor.SetActive(false);
            menu.SetActive(false);

            lastConfirmButtonUsed = padPressed.Down ? "ButtonPadPress" : "ButtonTriggerPress";
            mainFace.SetExpression(hovered.Item1, hovered.Item2);
            feedbackFace.SetExpression(hovered.Item1, hovered.Item2);
            //if (ModesManager.instance != null)
            //    ModesManager.instance.OnExpressionSet((hovered.Item1, hovered.Item2));
            //TechniquesManager.EnableTechnique(null);
            return;
        }

        // navigation
        if (!select)
        {
            if (!menu.activeSelf)
                menu.SetActive(true);
            cursor.SetActive(ctrl.HTCVive_PadTouch);
            if (ctrl.HTCVive_PadTouch)
            {
                cursor.transform.localPosition = ctrl.Compat_circularTouchpad.Value;
                hovered = GetClosest(ctrl.Compat_circularTouchpad);
            }
            else
            {
                hovered = (Expression.NEUTRAL, 0);
            }
            feedbackFace.SetExpression(hovered.Item1, hovered.Item2);

            return;
        }

        // selection release
        if (select && padPressed.Up || triggerPressed.Up)
        {
            gameObject.SetActive(false);
        }


    }




    private static Vector2 PolarVector2(float degrees, float magnitude) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad) * magnitude, Mathf.Sin(degrees * Mathf.Deg2Rad) * magnitude);

    private readonly Dictionary<Vector2, (Expression, float)> Expressions = new Dictionary<Vector2, (Expression, float)>
    {
        [PolarVector2(  0, 0.6f)] = (Expression.SMILING_WITH_HEART_EYES, 1), // keep
        [PolarVector2( 45, 0.6f)] = (Expression.SLIGHTLY_SMILING, 1), // keep
        [PolarVector2( 90, 0.6f)] = (Expression.GRINNING, 1), // keep
        [PolarVector2(135, 0.6f)] = (Expression.THINKING, 1), // keep
        [PolarVector2(180, 0.6f)] = (Expression.ANGRY, 1), // keep
        [PolarVector2(225, 0.6f)] = (Expression.FROWNING, 1), // keep
        [PolarVector2(270, 0.6f)] = (Expression.WITH_OPEN_MOUTH, 1), // keep
        [PolarVector2(315, 0.6f)] = (Expression.SMILING_WITH_SMILING_EYES, 1), // keep
    };
    

    private (Expression, float) GetClosest(Vector2 pos)
    {
        (Expression, float) closest = (Expression.NEUTRAL, 1);
        float closestSqrDist = float.PositiveInfinity;
        foreach (Vector2 uiPos in Expressions.Keys)
        {
            float sqrDist = (pos - uiPos).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closest = Expressions[uiPos];
                closestSqrDist = sqrDist;
            }
        }

        return closest;
    }


}
