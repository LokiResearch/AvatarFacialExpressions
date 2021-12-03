using System.Collections.Generic;
using UnityEngine;
using VRInputsAPI;

public class EmoTouch_ExprSelection : MonoBehaviour
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

    /*private readonly Dictionary<Vector2, (Expression, float)> Expressions = new Dictionary<Vector2, (Expression, float)>
    {
        [PolarVector2(  0,    0.38f)] = (Expression.WORRIED, 1),
        [PolarVector2(  0,    0.80f)] = (Expression.ANXIOUS_WITH_SWEAT, 1),
        [PolarVector2( 22.5f, 0.80f)] = (Expression.SMIRKING, 1),                   // commun
        [PolarVector2( 45,    0.38f)] = (Expression.SLIGHTLY_SMILING, 1),           // expé
        [PolarVector2( 45,    0.80f)] = (Expression.SMILING_WITH_SMILING_EYES, 1),  // expé
        [PolarVector2( 67.5f, 0.80f)] = (Expression.SMILING_WITH_HEART_EYES, 1),    // expé
        [PolarVector2( 90,    0.38f)] = (Expression.GRINNING, 1),                   // expé
        [PolarVector2( 90,    0.80f)] = (Expression.WITH_TEARS_OF_JOY, 1),
        [PolarVector2(112.5f, 0.80f)] = (Expression.WINKING, 1),                    // commun
        [PolarVector2(135,    0.38f)] = (Expression.THINKING, 1),                   // expé
        [PolarVector2(135,    0.80f)] = (Expression.ZIPPER_MOUTH, 1),
        [PolarVector2(157.5f, 0.80f)] = (Expression.WITH_SYMBOLS_ON_MOUTH, 1),
        [PolarVector2(180,    0.38f)] = (Expression.EXPRESSIONLESS, 1),
        [PolarVector2(180,    0.80f)] = (Expression.ANGRY, 1),                      // expé
        [PolarVector2(202.5f, 0.80f)] = (Expression.UNAMUSED, 1),
        [PolarVector2(225,    0.38f)] = (Expression.PERSEVERING, 1),
        [PolarVector2(225,    0.80f)] = (Expression.NAUSEATED, 1),                  // commun
        [PolarVector2(247.5f, 0.80f)] = (Expression.DISAPPOINTED, 1),
        [PolarVector2(270,    0.38f)] = (Expression.FROWNING, 1),                   // expé
        [PolarVector2(270,    0.80f)] = (Expression.CRYING, 1),
        [PolarVector2(292.5f, 0.80f)] = (Expression.DOWNCAST_WITH_SWEAT, 1),        // commun
        [PolarVector2(315,    0.38f)] = (Expression.WITH_OPEN_MOUTH, 1),            // expé
        [PolarVector2(315,    0.80f)] = (Expression.ASTONISHED, 1),
        [PolarVector2(337.5f, 0.80f)] = (Expression.SCREAMING_IN_FEAR, 1),
    };*/

    private readonly Dictionary<Vector2, (Expression, float)> Expressions = new Dictionary<Vector2, (Expression, float)>
    {
        // [PolarVector2(0, 0)] = (Expression.NEUTRAL, 1),
        [PolarVector2(0, 0.29f)] = (Expression.WORRIED, 1),
        [PolarVector2(0, 0.57f)] = (Expression.FEARFULL, 1),
        [PolarVector2(0, 0.87f)] = (Expression.ANXIOUS_WITH_SWEAT, 1),
        [PolarVector2(22.5f, 0.87f)] = (Expression.SMIRKING, 1),
        [PolarVector2(45, 0.29f)] = (Expression.SLIGHTLY_SMILING, 1),
        [PolarVector2(45, 0.57f)] = (Expression.GRINNING, 1),
        [PolarVector2(45, 0.87f)] = (Expression.SMILING_WITH_SMILING_EYES, 1),
        [PolarVector2(67.5f, 0.87f)] = (Expression.SMILING_WITH_HEART_EYES, 1),
        [PolarVector2(90, 0.29f)] = (Expression.RELIEVED, 1),
        [PolarVector2(90, 0.57f)] = (Expression.BEAMING_WITH_SMILING_EYES, 1),
        [PolarVector2(90, 0.87f)] = (Expression.WITH_TEARS_OF_JOY, 1),
        [PolarVector2(112.5f, 0.87f)] = (Expression.WINKING, 1),
        [PolarVector2(135, 0.29f)] = (Expression.THINKING, 1),
        [PolarVector2(135, 0.57f)] = (Expression.WITH_RAISED_EYEBROW, 1),
        [PolarVector2(135, 0.87f)] = (Expression.ZIPPER_MOUTH, 1),
        [PolarVector2(157.5f, 0.87f)] = (Expression.WITH_SYMBOLS_ON_MOUTH, 1),
        [PolarVector2(180, 0.29f)] = (Expression.EXPRESSIONLESS, 1),
        [PolarVector2(180, 0.57f)] = (Expression.ANGRY, 1),
        [PolarVector2(180, 0.87f)] = (Expression.POUTING, 1),
        [PolarVector2(202.5f, 0.87f)] = (Expression.UNAMUSED, 1),
        [PolarVector2(225, 0.29f)] = (Expression.WITH_ROLLING_EYES, 1),
        [PolarVector2(225, 0.57f)] = (Expression.PERSEVERING, 1),
        [PolarVector2(225, 0.87f)] = (Expression.NAUSEATED, 1),
        [PolarVector2(247.5f, 0.87f)] = (Expression.DISAPPOINTED, 1),
        [PolarVector2(270, 0.29f)] = (Expression.CONFUSED, 1),
        [PolarVector2(270, 0.57f)] = (Expression.FROWNING, 1),
        [PolarVector2(270, 0.87f)] = (Expression.CRYING, 1),
        [PolarVector2(292.5f, 0.87f)] = (Expression.DOWNCAST_WITH_SWEAT, 1),
        [PolarVector2(315, 0.29f)] = (Expression.HUSHED, 1),
        [PolarVector2(315, 0.57f)] = (Expression.WITH_OPEN_MOUTH, 1),
        [PolarVector2(315, 0.87f)] = (Expression.ASTONISHED, 1),
        [PolarVector2(337.5f, 0.87f)] = (Expression.SCREAMING_IN_FEAR, 1),
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
