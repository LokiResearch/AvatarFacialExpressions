using System.Collections.Generic;
using UnityEngine;
using VRInputsAPI;

public class EmoRay_Trigger : MonoBehaviour
{
    public static float MENU_DISTANCE = 0.6f;

    public GameObject menuElement;
    public GameObject baseElement;

    public RayCursor.RayCursor rayCursor;

    public Face mainFace;
    public Face feedbackFace;

    private void Start()
    {

    }


    private static Vector2 PolarVector2(float degrees, float magnitude) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad) * magnitude, Mathf.Sin(degrees * Mathf.Deg2Rad) * magnitude);

    private readonly Dictionary<Vector2, (Expression, float)> Expressions = new Dictionary<Vector2, (Expression, float)>
    {
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


    void OnEnable()
    {
        if (baseElement != null) // should be null at the end of initializing
        {
            foreach (KeyValuePair<Vector2, (Expression, float)> kv in Expressions)
            {
                Expression e = kv.Value.Item1;
                if (e == Expression.NEUTRAL)
                    continue;
                GameObject go = Instantiate(baseElement);
                go.name = "Menu Item " + e.ToString();
                go.transform.parent = menuElement.transform;
                go.transform.localPosition = ((Vector3)kv.Key) / 2 + new Vector3(0, 0, -0.003f);
                go.transform.localScale = Vector3.one * 0.09f;
                go.transform.localRotation = Quaternion.identity;
                go.GetComponent<MeshRenderer>().material.mainTexture = e.LoadTexture();
                go.GetComponent<RayCursor.Selectable>().OnSelect += () =>
                {
                    selected = e;
                };
                go.GetComponent<RayCursor.Selectable>().OnHighlightIn += () =>
                {
                    feedbackFace.SetExpression(e, 1);
                };
            }

            Destroy(baseElement);
        }
        feedbackFace.gameObject.SetActive(true);
        menuElement.SetActive(true);

        Transform headTr = VRInputsManager.GetDevice(VRDeviceEnum.HMD).GetTransform();
        Transform ctrlTr = VRInputsManager.GetMainHandController().GetTransform();

        rayCursor.ray.Parent = VRInputsManager.GetMainHandController().GetGameObject();

        Vector3 ctrlFw = ctrlTr.forward;
        ctrlFw.y = 0;
        ctrlFw.Normalize();

        transform.position = ctrlTr.position + ctrlFw * MENU_DISTANCE;

        Vector3 menuFw = transform.position - headTr.position;
        menuFw.y = 0;
        menuFw.Normalize();

        transform.forward = menuFw;

        rayCursor.gameObject.SetActive(true);
        rayCursor.ray.gameObject.SetActive(true);

    }


    private void OnDisable()
    {
        rayCursor.gameObject.SetActive(false);
        rayCursor.ray.gameObject.SetActive(false);
        feedbackFace.gameObject.SetActive(false);
        menuHidden = false;
        selected = Expression.NEUTRAL;
        mainFace.SetExpression(Expression.NEUTRAL, 0);
        feedbackFace.SetExpression(Expression.NEUTRAL, 0);
        if (selectedIcon != null)
        {
            Destroy(selectedIcon);
            selectedIcon = null;
        }
    }



    Expression selected = Expression.NEUTRAL;
    private GameObject selectedIcon = null;

    bool menuHidden = false;
    float releaseTime = 0;

    private HapticSliderController hapticIntensity = new HapticSliderController(new (float, float)[]{
            (0.05f, 0.01f), (0.15f, 0.04f), (0.25f, 0.09f), (0.35f, 0.16f), (0.45f, 0.25f),
            (0.55f, 0.36f), (0.65f, 0.49f), (0.75f, 0.64f), (0.85f, 0.81f), (0.95f, 1f),
        }, 0.01f);

    // Update is called once per frame
    void Update()
    {
        feedbackFace.ReplicateGenderFrom(mainFace);

        if (selected == Expression.NEUTRAL)
            return;

        // hide the menu, except the selected element
        if (!menuHidden)
        {
            // grab icon from menu elements
            Transform found = menuElement.transform.Find("Menu Item " + selected.ToString());
            selectedIcon = Instantiate(found.gameObject, transform, true);

            menuElement.SetActive(false);
            

            menuHidden = true;

            hapticIntensity.Reset(0);

        }
        


        VRThresholdedInput triggerPressed = VRInputsManager.GetMainHandController().HTCVive_TriggerAxis.GetThresholdedInput(0.1f, 0.13f);

        if (triggerPressed.Up)
        {
            releaseTime = Time.time;
        }

        if (!triggerPressed && Time.time - releaseTime > 1)
        {
            gameObject.SetActive(false);
            return;
        }

        float intensity = VRInputsManager.GetMainHandController().HTCVive_TriggerAxis;

        mainFace.SetExpression(selected, intensity);
        feedbackFace.SetExpression(selected, intensity);

        hapticIntensity.Update(VRInputsManager.GetMainHandController(), intensity);


    }




    
}
