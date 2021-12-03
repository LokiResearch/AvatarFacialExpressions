using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using utils;

public class ShapeKeysFace : Face
{
    private const int NB_BLEND_SHAPES = 23;

    public Transform face;

    public SkinnedMeshRenderer SkinnedMeshRenderer;
    public MeshRenderer hairsH, hairsF;
    public GameObject leftEye, rightEye;

    public GameObject heartPrefab;
    public GameObject waterDropPrefab;

    private List<Accessory> activeAccessories = new List<Accessory>();

    private bool hairsWoman = false;

    private Expression currentExpression = Expression.NEUTRAL;
    private float currentIntensity = 0;

    public override void SetWomanHair()
    {
        hairsWoman = true;
        hairsF.enabled = visible;
        hairsH.enabled = false;
    }
    public override void SetManHair()
    {
        hairsWoman = false;
        hairsF.enabled = false;
        hairsH.enabled = visible;
    }

    public override bool isWoman()
    {
        return hairsWoman;
    }

    public override void SetExpression(Expression expression, float intensity)
    {
        this.expression = expression;
        this.intensity = intensity;
    }

    public override Expression GetCurrentExpression()
    {
        return expression;
    }

    bool wasVisibleBeforeDisabled = true;

    private void OnDisable()
    {
        wasVisibleBeforeDisabled = visible;
        if (visible)
        {
            SetVisible(false);
        }
    }

    private void OnEnable()
    {
        if (wasVisibleBeforeDisabled)
        {
            SetVisible(true);
        }
    }

    private bool visible = true;
    public override void SetVisible(bool v)
    {
        visible = v;
        if (v)
        {
            if (ExpressionMapping.ContainsKey(currentExpression))
                ExpressionMapping[currentExpression].Accessories.ForEach(acc => acc.Update(this, intensity));
            SkinnedMeshRenderer.enabled = true;
            hairsH.enabled = !hairsWoman;
            hairsF.enabled = hairsWoman;
            leftEye.SetActive(true);
            rightEye.SetActive(true);
        }
        else
        {
            new List<Accessory>(activeAccessories).ForEach(acc => acc.HideInstantly(this));

            SkinnedMeshRenderer.enabled = false;
            hairsH.enabled = false;
            hairsF.enabled = false;
            leftEye.SetActive(false);
            rightEye.SetActive(false);
        }
    }

    public Expression expression;
    public float intensity;


    private float[] TargetValues = new float[NB_BLEND_SHAPES];
    private Color TargetSkinTint = Color.white;
    
    private float currentExpressionStartTime = 0;

    public void LateUpdate()
    {
        if (expression != currentExpression || intensity == 0)
        {
            currentExpressionStartTime = Time.time;

            if (ExpressionMapping.ContainsKey(currentExpression))
                ExpressionMapping[currentExpression].Accessories.ForEach(acc => acc.Hide(this)); 
        }

        currentExpression = expression;
        currentIntensity = intensity;

        ExpressionConfiguration expressionConfig = ExpressionMapping.ContainsKey(currentExpression) ? ExpressionMapping[currentExpression] : ExpressionMapping[Expression.NEUTRAL];

        uint currentTimer = (uint)((Time.time - currentExpressionStartTime) * 1000);
        

        // update target values according to selected expression and animation progress
        for (int i = 0; i < NB_BLEND_SHAPES; i++)
        {
            TargetValues[i] = expressionConfig[currentTimer, (Shape)i] * intensity;
        }
        TargetSkinTint = Color.Lerp(Color.white, expressionConfig.SkinTint, intensity);


        // update actual values on face to animate towards target values

        // update shape keys
        for (int i = 0; i < NB_BLEND_SHAPES; i++)
        {
            float currV = SkinnedMeshRenderer.GetBlendShapeWeight(i);
            float targetV = TargetValues[i];

            if (currV == targetV)
                continue;

            if (Mathf.Abs(currV - targetV) < 0.01)
                currV = targetV;
            else
                currV = currV + SmoothAnimationCoefficient[(Shape)i] * (targetV - currV);
            SkinnedMeshRenderer.SetBlendShapeWeight(i, currV);
        }

        // update skin tint
        {
            Color currC = SkinnedMeshRenderer.materials[1].color;
            Color targetC = TargetSkinTint;

            if (currC != targetC)
            {
                if (Vector4.Distance(currC, targetC) < 0.01)
                    currC = targetC;
                else
                    currC = currC + SkinTintSmoothAnimationCoefficient * (targetC - currC);

                SkinnedMeshRenderer.materials[1].color = currC;
                SkinnedMeshRenderer.materials[2].color = currC;
            }
        }

        // update accessories
        if (visible)
            expressionConfig.Accessories.ForEach(acc => acc.Update(this, intensity));
    }



    public enum Shape
    {
        AU_L1_L4_browinnerL,
        AU_R1_R4_browinnerR,
        AU_L2_browouterL,
        AU_R2_browouterR,
        AU_L5_L41_L42_L43_L44_L45_eyelid_topL,
        AU_R5_R41_R42_R43_R44_R45_eyelid_topR,
        AU_L6_squintL,
        AU_R6_squintR,
        AU_L7_L44_eyelid_bottomL,
        AU_R7_R44_eyelid_bottomR,
        AU_10_toplipUp,
        AU_L10_snarlL,
        AU_R10_snarlR,
        AU_L12_smileL,
        AU_R12_smileR,
        AU_L15_frownL,
        AU_R15_frownR,
        AU_23_lipspursed,
        AU_25_bottomlipDown,
        AU_26_jawdown,
        AU_29_jawjutt,
        AU_L30_jawleft,
        AU_R30_jawright
    }



    public static Dictionary<Shape, float> SmoothAnimationCoefficient = new Dictionary<Shape, float>
    {
        [Shape.AU_L1_L4_browinnerL] = 0.2f,
        [Shape.AU_R1_R4_browinnerR] = 0.2f,
        [Shape.AU_L2_browouterL] = 0.1f,
        [Shape.AU_R2_browouterR] = 0.1f,
        [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 0.5f,
        [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 0.5f,
        [Shape.AU_L6_squintL] = 0.2f,
        [Shape.AU_R6_squintR] = 0.2f,
        [Shape.AU_L7_L44_eyelid_bottomL] = 0.4f,
        [Shape.AU_R7_R44_eyelid_bottomR] = 0.4f,
        [Shape.AU_10_toplipUp] = 0.2f,
        [Shape.AU_L10_snarlL] = 0.2f,
        [Shape.AU_R10_snarlR] = 0.2f,
        [Shape.AU_L12_smileL] = 0.2f,
        [Shape.AU_R12_smileR] = 0.2f,
        [Shape.AU_L15_frownL] = 0.15f,
        [Shape.AU_R15_frownR] = 0.15f,
        [Shape.AU_23_lipspursed] = 0.2f,
        [Shape.AU_25_bottomlipDown] = 0.2f,
        [Shape.AU_26_jawdown] = 0.3f,
        [Shape.AU_29_jawjutt] = 0.1f,
        [Shape.AU_L30_jawleft] = 0.3f,
        [Shape.AU_R30_jawright] = 0.3f
    };
    public static float SkinTintSmoothAnimationCoefficient = 0.1f;



    public Dictionary<Expression, ExpressionConfiguration> ExpressionMapping = new Dictionary<Expression, ExpressionConfiguration>
    {
        [Expression.NEUTRAL] = new ExpressionConfiguration
        {
            // none
        },
        [Expression.GRINNING] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 10,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 10,
            [Shape.AU_L6_squintL] = 70,
            [Shape.AU_R6_squintR] = 70,
            [Shape.AU_L7_L44_eyelid_bottomL] = 40,
            [Shape.AU_R7_R44_eyelid_bottomR] = 40,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,
        },
        [Expression.GRINNIG_WITH_BIG_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -20,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = -10,
            [Shape.AU_R7_R44_eyelid_bottomR] = -10,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,
        },
        [Expression.GRINNING_WITH_SMILING_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,
        },
        [Expression.BEAMING_WITH_SMILING_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
        },
        [Expression.GRINNING_SQUINTING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 30,
            [Shape.AU_R1_R4_browinnerR] = 30,
            [Shape.AU_L2_browouterL] = -30,
            [Shape.AU_R2_browouterR] = -30,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 70,
        },
        [Expression.GRINNING_WITH_SWEAT] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3(-0.048f, 0.18f, 0.0965f), Quaternion.Euler(-15, 5,  0), new Vector3(0.015f, 0.015f, 0.01f), 0.5f),
            },
        },
        [Expression.ROLLING_ON_THE_FLOOR_LAUGHING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 30,
            [Shape.AU_R1_R4_browinnerR] = 30,
            [Shape.AU_L2_browouterL] = -30,
            [Shape.AU_R2_browouterR] = -30,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 80,
            [Shape.AU_L12_smileL] = 100,
            [Shape.AU_R12_smileR] = 100,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,

            [110, Shape.AU_L1_L4_browinnerL] = 30,
            [110, Shape.AU_R1_R4_browinnerR] = 30,
            [110, Shape.AU_L2_browouterL] = -30,
            [110, Shape.AU_R2_browouterR] = -30,
            [110, Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [110, Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [110, Shape.AU_L6_squintL] = 100,
            [110, Shape.AU_R6_squintR] = 100,
            [110, Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [110, Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [110, Shape.AU_10_toplipUp] = 80,
            [110, Shape.AU_L12_smileL] = 100,
            [110, Shape.AU_R12_smileR] = 100,
            [110, Shape.AU_25_bottomlipDown] = 100,
            [110, Shape.AU_26_jawdown] = 50,

            LoopDuration = 220,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3( 0.0662f, 0.079f, 0.087f), Quaternion.Euler(0, 0,  65), Vector3.one * 0.01f, 0.8f),
                new Accessory(AccessoryType.WATER_DROP, new Vector3(-0.0662f, 0.079f, 0.087f), Quaternion.Euler(0, 0, -65), Vector3.one * 0.01f, 0.8f),
            },
        },
        [Expression.WITH_TEARS_OF_JOY] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3( 0.0662f, 0.079f, 0.087f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.01f, 0.8f),
                new Accessory(AccessoryType.WATER_DROP, new Vector3(-0.0662f, 0.079f, 0.087f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.01f, 0.8f),
            },
        },
        [Expression.SLIGHTLY_SMILING] = new ExpressionConfiguration
        {
            [Shape.AU_L6_squintL] = 30,
            [Shape.AU_R6_squintR] = 30,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
        },
        [Expression.WINKING] = new ExpressionConfiguration
        {
            [Shape.AU_R2_browouterR] = -20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 30,
            [Shape.AU_R6_squintR] = 50,
            [Shape.AU_L7_L44_eyelid_bottomL] = 20,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 100,
            // TODO add choice of side
        },
        [Expression.SMILING_WITH_SMILING_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_L12_smileL] = 100,
            [Shape.AU_R12_smileR] = 100,
        },
        [Expression.SMILING_WITH_HALO] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_L12_smileL] = 100,
            [Shape.AU_R12_smileR] = 100,
            // TODO add halo
        },
        [Expression.SMILING_WITH_HEARTS] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_L12_smileL] = 100,
            [Shape.AU_R12_smileR] = 100,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.LOVE_HEART, new Vector3(-0.0717f, 0.0201f, 0.0951f), Quaternion.Euler(0, -52.5f, 0), Vector3.one * 0.015f, 0.3f),
                new Accessory(AccessoryType.LOVE_HEART, new Vector3(-0.0240f, 0.1260f, 0.1246f), Quaternion.Euler(0,  -8.4f, 0), Vector3.one * 0.020f, 0.5f),
                new Accessory(AccessoryType.LOVE_HEART, new Vector3( 0.0726f, 0.0210f, 0.0974f), Quaternion.Euler(0,  41.7f, 0), Vector3.one * 0.025f, 0.7f),
            }
        },
        [Expression.SMILING_WITH_HEART_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -20,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = -10,
            [Shape.AU_R7_R44_eyelid_bottomR] = -10,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.LOVE_HEART, new Vector3( 0.044f, 0.0588f, 0.1082f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.025f, 0.3f),
                new Accessory(AccessoryType.LOVE_HEART, new Vector3(-0.044f, 0.0588f, 0.1082f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.025f, 0.3f),
            }
        },
        [Expression.STAR_STRUCK] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -20,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = -10,
            [Shape.AU_R7_R44_eyelid_bottomR] = -10,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,
            // TODO add stars on the eyes
        },
        [Expression.BLOWING_A_KISS] = new ExpressionConfiguration
        {
            [Shape.AU_R2_browouterR] = -20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 30,
            [Shape.AU_R6_squintR] = 50,
            [Shape.AU_L7_L44_eyelid_bottomL] = 20,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_23_lipspursed] = 100,
            // TODO heart coming out of the mouth
        },
        [Expression.KISSING] = new ExpressionConfiguration
        {
            [Shape.AU_L6_squintL] = 20,
            [Shape.AU_R6_squintR] = 20,
            [Shape.AU_L7_L44_eyelid_bottomL] = 20,
            [Shape.AU_R7_R44_eyelid_bottomR] = 20,
            [Shape.AU_23_lipspursed] = 100,
        },
        [Expression.SMILING] = new ExpressionConfiguration
        {
            [Shape.AU_R2_browouterR] = -20,
            [Shape.AU_L2_browouterL] = -20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 50,
            [Shape.AU_R6_squintR] = 50,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_L12_smileL] = 100,
            [Shape.AU_R12_smileR] = 100,
        },
        [Expression.KISSING_WITH_CLOSED_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_R2_browouterR] = -20,
            [Shape.AU_L2_browouterL] = -20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_23_lipspursed] = 100,
        },
        [Expression.KISSING_WITH_SMILING_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_R2_browouterR] = 20,
            [Shape.AU_L2_browouterL] = 20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 95,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 95,
            [Shape.AU_L7_L44_eyelid_bottomL] = 100,
            [Shape.AU_R7_R44_eyelid_bottomR] = 100,
            [Shape.AU_23_lipspursed] = 100,
        },
        [Expression.SAVORING_FOOD] = new ExpressionConfiguration
        {
            // needs tongue shapes
        },
        [Expression.WITH_TONGUE] = new ExpressionConfiguration
        {
            // needs tongue shapes
        },
        [Expression.WINKING_WITH_TONGUE] = new ExpressionConfiguration
        {
            // needs tongue shapes
        },
        [Expression.ZANY] = new ExpressionConfiguration // crazy face
        {
            // needs tongue shapes
        },
        [Expression.SQUINTING_WITH_TONGUE] = new ExpressionConfiguration
        {
            // needs tongue shapes
        },
        [Expression.MONEY_MOUTH] = new ExpressionConfiguration
        {
            // needs tongue shapes
            // TODO money textures on tongue
            // TODO money eyes
        },
        [Expression.THINKING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 30,
            [Shape.AU_R1_R4_browinnerR] = -10,
            [Shape.AU_L2_browouterL] = -30,
            [Shape.AU_R2_browouterR] = 60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_L7_L44_eyelid_bottomL] = 30,
            [Shape.AU_R7_R44_eyelid_bottomR] = 30,
            [Shape.AU_L12_smileL] = 30,
            [Shape.AU_L15_frownL] = 70,
            [Shape.AU_23_lipspursed] = -20,
        },
        [Expression.ZIPPER_MOUTH] = new ExpressionConfiguration
        {
            [Shape.AU_23_lipspursed] = -60,
            // TODO zipped mouth
        },
        [Expression.WITH_RAISED_EYEBROW] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = -20,
            [Shape.AU_L2_browouterL] = 60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -15,
            [Shape.AU_23_lipspursed] = -60,
        },
        [Expression.EXPRESSIONLESS] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_23_lipspursed] = -60,
        },
        [Expression.WITHOUT_MOUTH] = new ExpressionConfiguration
        {
            // needs no mouth (how to do this ?)
        },
        [Expression.SMIRKING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = -20,
            [Shape.AU_R1_R4_browinnerR] = -20,
            [Shape.AU_L2_browouterL] = 40,
            [Shape.AU_R2_browouterR] = 40,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L7_L44_eyelid_bottomL] = 85,
            [Shape.AU_R7_R44_eyelid_bottomR] = 85,
            [Shape.AU_10_toplipUp] = 45,
            [Shape.AU_L12_smileL] = 100,
            [Shape.AU_23_lipspursed] = -60,

            [125, Shape.AU_L1_L4_browinnerL] = 0,
            [125, Shape.AU_R1_R4_browinnerR] = 0,
            [125, Shape.AU_L2_browouterL] = 0,
            [125, Shape.AU_R2_browouterR] = 0,
            [125, Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 45,
            [125, Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 45,
            [125, Shape.AU_L7_L44_eyelid_bottomL] = 85,
            [125, Shape.AU_R7_R44_eyelid_bottomR] = 85,
            [125, Shape.AU_10_toplipUp] = 45,
            [125, Shape.AU_L12_smileL] = 100,
            [125, Shape.AU_23_lipspursed] = -60,

            [250, Shape.AU_L1_L4_browinnerL] = -20,
            [250, Shape.AU_R1_R4_browinnerR] = -20,
            [250, Shape.AU_L2_browouterL] = 40,
            [250, Shape.AU_R2_browouterR] = 40,
            [250, Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [250, Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [250, Shape.AU_L7_L44_eyelid_bottomL] = 85,
            [250, Shape.AU_R7_R44_eyelid_bottomR] = 85,
            [250, Shape.AU_10_toplipUp] = 45,
            [250, Shape.AU_L12_smileL] = 100,
            [250, Shape.AU_23_lipspursed] = -60,

            [375, Shape.AU_L1_L4_browinnerL] = 0,
            [375, Shape.AU_R1_R4_browinnerR] = 0,
            [375, Shape.AU_L2_browouterL] = 0,
            [375, Shape.AU_R2_browouterR] = 0,
            [375, Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 45,
            [375, Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 45,
            [375, Shape.AU_L7_L44_eyelid_bottomL] = 85,
            [375, Shape.AU_R7_R44_eyelid_bottomR] = 85,
            [375, Shape.AU_10_toplipUp] = 45,
            [375, Shape.AU_L12_smileL] = 100,
            [375, Shape.AU_23_lipspursed] = -60,

            LoopDuration = 1500,
        },
        [Expression.UNAMUSED] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 60,
            [Shape.AU_R1_R4_browinnerR] = 60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 60,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 60,
            [Shape.AU_L15_frownL] = 65,
            [Shape.AU_R15_frownR] = 65,
        },
        [Expression.WITH_ROLLING_EYES] = new ExpressionConfiguration
        {
            [Shape.AU_R15_frownR] = 100,
            // make eyes rolling
        },
        [Expression.GRIMACING] = new ExpressionConfiguration
        {
            [Shape.AU_10_toplipUp] = 90,
            [Shape.AU_L10_snarlL] = 25,
            [Shape.AU_R10_snarlR] = 25,
            [Shape.AU_L12_smileL] = 70,
            [Shape.AU_R12_smileR] = 70,
            [Shape.AU_L15_frownL] = 70,
            [Shape.AU_R15_frownR] = 70,
            [Shape.AU_25_bottomlipDown] = 100,
        },
        [Expression.LYING_FACE] = new ExpressionConfiguration
        {
            [Shape.AU_L15_frownL] = 40,
            [Shape.AU_R15_frownR] = 80,
            // TODO long nose
        },
        [Expression.RELIEVED] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_L12_smileL] = 70,
            [Shape.AU_R12_smileR] = 70,
        },
        [Expression.PENSIVE] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_L15_frownL] = 50,
            [Shape.AU_R15_frownR] = 50,
        },
        [Expression.SLEEPY] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_25_bottomlipDown] = 70,
            // TODO bubble (curved droplet)
        },
        [Expression.DROOLING] = new ExpressionConfiguration
        {
            [Shape.AU_R2_browouterR] = 20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 70,
            [Shape.AU_R6_squintR] = 70,
            [Shape.AU_L7_L44_eyelid_bottomL] = 93,
            [Shape.AU_R7_R44_eyelid_bottomR] = 93,
            [Shape.AU_25_bottomlipDown] = 70,
            // TODO saliva dropping
        },
        [Expression.SLEEPING] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_25_bottomlipDown] = 70,
            // TODO zzzzzzzzzzz
        },
        [Expression.WITH_MEDICAL_MASK] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 55,
            [Shape.AU_R1_R4_browinnerR] = 55,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            // TODO medical mask
        },
        [Expression.WITH_THERMOMETER] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 40,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 40,
            [Shape.AU_L7_L44_eyelid_bottomL] = 40,
            [Shape.AU_R7_R44_eyelid_bottomR] = 40,
            [Shape.AU_L15_frownL] = 25,
            [Shape.AU_R15_frownR] = 25,
            // TODO thermometer
        },
        [Expression.WITH_HEAD_BANDAGE] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 40,
            [Shape.AU_R1_R4_browinnerR] = 40,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_L15_frownL] = 50,
            [Shape.AU_R15_frownR] = 50,
            // TODO bandage
        },
        [Expression.NAUSEATED] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 60,
            [Shape.AU_R1_R4_browinnerR] = 60,
            [Shape.AU_L2_browouterL] = -60,
            [Shape.AU_R2_browouterR] = -60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 50,
            [Shape.AU_R6_squintR] = 50,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 20,
            [Shape.AU_L10_snarlL] = 40,
            [Shape.AU_R10_snarlR] = 40,
            [Shape.AU_L15_frownL] = 50,
            [Shape.AU_R15_frownR] = 50,
            [Shape.AU_25_bottomlipDown] = 30,

            SkinTint = new Color(.8f, 1, .6f),
        },
        [Expression.VOMITING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 60,
            [Shape.AU_R1_R4_browinnerR] = 60,
            [Shape.AU_L2_browouterL] = -60,
            [Shape.AU_R2_browouterR] = -60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 50,
            [Shape.AU_R6_squintR] = 50,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 40,
            [Shape.AU_L10_snarlL] = 40,
            [Shape.AU_R10_snarlR] = 40,
            [Shape.AU_L15_frownL] = 50,
            [Shape.AU_R15_frownR] = 50,
            [Shape.AU_25_bottomlipDown] = 75,
            [Shape.AU_26_jawdown] = 50,
            // TODO vomit ?
        },
        [Expression.HOT] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = -20,
            [Shape.AU_R1_R4_browinnerR] = -20,
            [Shape.AU_L2_browouterL] = -60,
            [Shape.AU_R2_browouterR] = -60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_10_toplipUp] = 20,
            [Shape.AU_L15_frownL] = 30,
            [Shape.AU_R15_frownR] = 30,
            [Shape.AU_25_bottomlipDown] = 50,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3(-0.048f, 0.18f,  0.0965f), Quaternion.Euler(-15, 5,  0), new Vector3(0.015f, 0.015f, 0.01f), 0.4f),
                new Accessory(AccessoryType.WATER_DROP, new Vector3( 0.048f, 0.154f, 0.1045f), Quaternion.Euler(-15, 15,  0), new Vector3(0.01f, 0.01f, 0.007f), 0.6f)
            },

            SkinTint = new Color(1, .75f, .75f),
        },
        [Expression.COLD] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = -60,
            [Shape.AU_R2_browouterR] = -60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L7_L44_eyelid_bottomL] = 50,
            [Shape.AU_R7_R44_eyelid_bottomR] = 50,
            [Shape.AU_10_toplipUp] = 90,
            [Shape.AU_L10_snarlL] = 25,
            [Shape.AU_R10_snarlR] = 25,
            [Shape.AU_L12_smileL] = 70,
            [Shape.AU_R12_smileR] = 70,
            [Shape.AU_L15_frownL] = 70,
            [Shape.AU_R15_frownR] = 70,
            [Shape.AU_25_bottomlipDown] = 100,


            [75, Shape.AU_L2_browouterL] = -60,
            [75, Shape.AU_R2_browouterR] = -60,
            [75, Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [75, Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [75, Shape.AU_L7_L44_eyelid_bottomL] = 50,
            [75, Shape.AU_R7_R44_eyelid_bottomR] = 50,
            [75, Shape.AU_10_toplipUp] = 90,
            [75, Shape.AU_L10_snarlL] = 25,
            [75, Shape.AU_R10_snarlR] = 25,
            [75, Shape.AU_L12_smileL] = 70,
            [75, Shape.AU_R12_smileR] = 70,
            [75, Shape.AU_L15_frownL] = 70,
            [75, Shape.AU_R15_frownR] = 70,
            [75, Shape.AU_25_bottomlipDown] = 100,
            [75, Shape.AU_26_jawdown] = 30,

            LoopDuration = 150,

            SkinTint = new Color(.75f, .75f, 1),
        },
        [Expression.WOOZY] = new ExpressionConfiguration
        {
            [Shape.AU_R1_R4_browinnerR] = -20,
            [Shape.AU_R2_browouterR] = 20,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 55,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 15,
            [Shape.AU_L7_L44_eyelid_bottomL] = 45,
            [Shape.AU_R7_R44_eyelid_bottomR] = 100,
            [Shape.AU_L10_snarlL] = 75,
            [Shape.AU_R10_snarlR] = 40,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_25_bottomlipDown] = 60,
        },
        [Expression.DIZZY] = new ExpressionConfiguration
        {
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_23_lipspursed] = 100,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,
            // TODO dead eyes
        },
        [Expression.EXPLODING_HEAD] = new ExpressionConfiguration
        {
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_23_lipspursed] = 100,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,
            // TODO exploding head
        },
        [Expression.COWBOY_HAT] = new ExpressionConfiguration
        {
            [Shape.AU_L6_squintL] = 30,
            [Shape.AU_R6_squintR] = 30,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            // TODO cowboy hat
        },
        [Expression.SMILING_WITH_SUNGLASSES] = new ExpressionConfiguration
        {
            [Shape.AU_L6_squintL] = 30,
            [Shape.AU_R6_squintR] = 30,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            // TODO sunglasses
        },
        [Expression.NERD] = new ExpressionConfiguration
        {
            [Shape.AU_L6_squintL] = 30,
            [Shape.AU_R6_squintR] = 30,
            [Shape.AU_L12_smileL] = 60,
            [Shape.AU_R12_smileR] = 60,
            // TODO glasses + teeth
        },
        [Expression.WITH_MONOCLE] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 70,
            [Shape.AU_R1_R4_browinnerR] = 30,
            [Shape.AU_L2_browouterL] = -40,
            [Shape.AU_L15_frownL] = 60,
            [Shape.AU_R15_frownR] = 60,
            // TODO monocle on left eye
        },
        [Expression.CONFUSED] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = -30,
            [Shape.AU_R2_browouterR] = -30,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 10,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 10,
            [Shape.AU_R15_frownR] = 50,
        },
        [Expression.WORRIED] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = -20,
            [Shape.AU_R1_R4_browinnerR] = -20,
            [Shape.AU_L2_browouterL] = -60,
            [Shape.AU_R2_browouterR] = -60,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_10_toplipUp] = 20,
            [Shape.AU_L15_frownL] = 30,
            [Shape.AU_R15_frownR] = 30,
            [Shape.AU_25_bottomlipDown] = 50,
        },
        [Expression.SLIGHTLY_FROWNING] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 60,
            [Shape.AU_R15_frownR] = 60,
        },
        [Expression.FROWNING] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
        },
        [Expression.WITH_OPEN_MOUTH] = new ExpressionConfiguration
        {
            [Shape.AU_10_toplipUp] = 80,
            [Shape.AU_23_lipspursed] = 50,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,
        },
        [Expression.HUSHED] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 50,
            [Shape.AU_R2_browouterR] = 50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -10,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -10,
            [Shape.AU_10_toplipUp] = 60,
            [Shape.AU_23_lipspursed] = 100,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 60,
        },
        [Expression.ASTONISHED] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 50,
            [Shape.AU_R2_browouterR] = 50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -10,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -10,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L10_snarlL] = 30,
            [Shape.AU_R10_snarlR] = 30,
            [Shape.AU_23_lipspursed] = 50,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,
        },
        [Expression.FLUSHED] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 70,
            [Shape.AU_R2_browouterR] = 70,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -10,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -10,
            [Shape.AU_L15_frownL] = 20,
            [Shape.AU_R15_frownR] = 20,
            // TODO reddish chicks (like an ashamed or embarrased person)
        },
        [Expression.PLEADING] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = -80,
            [Shape.AU_R2_browouterR] = -80,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 10,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 10,
            [Shape.AU_L15_frownL] = 60,
            [Shape.AU_R15_frownR] = 60,
        },
        [Expression.FROWNING_WITH_OPEN_MOUTH] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,
        },
        [Expression.ANGUISHED] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 50,
            [Shape.AU_R2_browouterR] = 50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,
        },
        [Expression.FEARFULL] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 50,
            [Shape.AU_R2_browouterR] = 50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,
            // TODO blueish forehead
        },
        [Expression.ANXIOUS_WITH_SWEAT] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = -50,
            [Shape.AU_R2_browouterR] = -50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,
            // TODO blueish forehead

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3(0.0727f, 0.0471f, 0.0742f), Quaternion.Euler(8, 80,  0), new Vector3(0.012f, 0.012f, 0.009f), 0.6f),
            }
        },
        [Expression.SAD_BUT_RELIEVED] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = -50,
            [Shape.AU_R2_browouterR] = -50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3(0.0727f, 0.0471f, 0.0742f), Quaternion.Euler(8, 80,  0), new Vector3(0.012f, 0.012f, 0.009f), 0.6f),
            }
        },
        [Expression.CRYING] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 50,
            [Shape.AU_R2_browouterR] = 50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 20,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 20,
            [Shape.AU_L7_L44_eyelid_bottomL] = 50,
            [Shape.AU_R7_R44_eyelid_bottomR] = 50,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3(-0.043f, 0.072f, 0.107f), Quaternion.Euler(0, 15,  0), new Vector3(0.015f, 0.015f, 0.01f), 0.4f),
            },
        },
        [Expression.LOUDLY_CRYING] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 50,
            [Shape.AU_R2_browouterR] = 50,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_L15_frownL] = 110,
            [Shape.AU_R15_frownR] = 110,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 80,
            // TODO tears flooding
        },
        [Expression.SCREAMING_IN_FEAR] = new ExpressionConfiguration
        {
            [Shape.AU_L2_browouterL] = 75,
            [Shape.AU_R2_browouterR] = 75,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = -15,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = -15,
            [Shape.AU_10_toplipUp] = 125,
            [Shape.AU_25_bottomlipDown] = 160,
            [Shape.AU_26_jawdown] = 150,
            // TODO blueish forehead
        },
        [Expression.CONFOUNDED] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 80,
            [Shape.AU_R1_R4_browinnerR] = 80,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_10_toplipUp] = -30,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 50,
        },
        [Expression.PERSEVERING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 80,
            [Shape.AU_R1_R4_browinnerR] = 80,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_10_toplipUp] = 50,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 50,
        },
        [Expression.DISAPPOINTED] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 50,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 50,
        },
        [Expression.DOWNCAST_WITH_SWEAT] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 50,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            [Shape.AU_25_bottomlipDown] = 50,
            [Shape.AU_26_jawdown] = 50,

            Accessories = new List<Accessory>
            {
                new Accessory(AccessoryType.WATER_DROP, new Vector3(0.048f, 0.18f, 0.0965f), Quaternion.Euler(-15, -5,  0), new Vector3(0.015f, 0.015f, 0.01f), 0.5f),
            },
        }, // downcast = abattu
        [Expression.WEARY] = new ExpressionConfiguration
        {
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 95,
            [Shape.AU_R7_R44_eyelid_bottomR] = 95,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,
        },
        [Expression.TIRED] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 80,
            [Shape.AU_R1_R4_browinnerR] = 80,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_10_toplipUp] = 100,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            [Shape.AU_25_bottomlipDown] = 100,
            [Shape.AU_26_jawdown] = 100,
        },
        [Expression.YAWNING] = new ExpressionConfiguration
        {
            // TODO later
        },
        [Expression.WITH_STEAM_FROM_NOSE] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 70,
            [Shape.AU_R1_R4_browinnerR] = 70,
            [Shape.AU_L5_L41_L42_L43_L44_L45_eyelid_topL] = 100,
            [Shape.AU_R5_R41_R42_R43_R44_R45_eyelid_topR] = 100,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 90,
            [Shape.AU_R7_R44_eyelid_bottomR] = 90,
            [Shape.AU_L10_snarlL] = 30,
            [Shape.AU_R10_snarlR] = 30,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            // TODO steam from nose
        },
        [Expression.POUTING] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 70,
            [Shape.AU_R1_R4_browinnerR] = 70,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 30,
            [Shape.AU_R7_R44_eyelid_bottomR] = 30,
            [Shape.AU_L10_snarlL] = 30,
            [Shape.AU_R10_snarlR] = 30,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,

            SkinTint = new Color(1, .4f, .4f),
        },
        [Expression.ANGRY] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 70,
            [Shape.AU_R1_R4_browinnerR] = 70,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 30,
            [Shape.AU_R7_R44_eyelid_bottomR] = 30,
            [Shape.AU_L10_snarlL] = 30,
            [Shape.AU_R10_snarlR] = 30,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
        },
        [Expression.WITH_SYMBOLS_ON_MOUTH] = new ExpressionConfiguration
        {
            [Shape.AU_L1_L4_browinnerL] = 70,
            [Shape.AU_R1_R4_browinnerR] = 70,
            [Shape.AU_L6_squintL] = 100,
            [Shape.AU_R6_squintR] = 100,
            [Shape.AU_L7_L44_eyelid_bottomL] = 30,
            [Shape.AU_R7_R44_eyelid_bottomR] = 30,
            [Shape.AU_L10_snarlL] = 30,
            [Shape.AU_R10_snarlR] = 30,
            [Shape.AU_L15_frownL] = 100,
            [Shape.AU_R15_frownR] = 100,
            
            [150, Shape.AU_L1_L4_browinnerL] = 70,
            [150, Shape.AU_R1_R4_browinnerR] = 70,
            [150, Shape.AU_L6_squintL] = 100,
            [150, Shape.AU_R6_squintR] = 100,
            [150, Shape.AU_L7_L44_eyelid_bottomL] = 30,
            [150, Shape.AU_R7_R44_eyelid_bottomR] = 30,
            [150, Shape.AU_10_toplipUp] = 30,
            [150, Shape.AU_L10_snarlL] = 30,
            [150, Shape.AU_R10_snarlR] = 30,
            [150, Shape.AU_L15_frownL] = 100,
            [150, Shape.AU_R15_frownR] = 100,
            [150, Shape.AU_25_bottomlipDown] = 50,
            [150, Shape.AU_26_jawdown] = 50,
            // TODO animated symbols on mouth

            LoopDuration = 300,

            SkinTint = new Color(1, .4f, .4f),
        },
    };





    public class ExpressionConfiguration
    {
        private Dictionary<uint, Dictionary<Shape, float>> poses = new Dictionary<uint, Dictionary<Shape, float>>();

        public uint LoopDuration { get; set; }

        public Color SkinTint { get; set; }

        public List<Accessory> Accessories = new List<Accessory>();

        public float this[Shape shape]
        {
            get => this[0, shape];
            set => this[0, shape] = value;
        }
        public float this[uint timer, Shape shape]
        {
            get {
                if (timer >= LoopDuration)
                    timer %= LoopDuration;
                while (!poses.ContainsKey(timer) && timer > 0)
                    timer--;
                return (poses.ContainsKey(timer) && poses[timer].ContainsKey(shape)) ? poses[timer][shape] : 0;
            }
            set {
                if (!poses.ContainsKey(timer))
                    poses[timer] = new Dictionary<Shape, float>();
                poses[timer][shape] = value;
            }
        }

        public ExpressionConfiguration()
        {
            LoopDuration = 1;
            SkinTint = Color.white;
        }

    }


    public enum AccessoryType
    {
        WATER_DROP, LOVE_HEART
    }

    public class Accessory
    {
        public AccessoryType Type { get; set; }
        public Vector3 Pos { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public float Threshold { get; set; }
        public float SmoothAnimationCoefficient { get; set; }

        public Accessory(AccessoryType t, Vector3 p, Quaternion r, Vector3 s, float th)
            : this(t, p, r, s, th, 0.3f) { }
        public Accessory(AccessoryType t, Vector3 p, Quaternion r, Vector3 s, float th, float sm)
        {
            Type = t;
            Pos = p;
            Rotation = r;
            Scale = s;
            Threshold = th;
            SmoothAnimationCoefficient = sm;
        }

        private GameObject instance = null;
        private int state = 0; // 0 = not displayed; 1 = is appearing; 2 = displayed; 3 = is disappearing

        public void Update(ShapeKeysFace parent, float intensity)
        {
            if ((state == 0 || state == 3) && (intensity >= Threshold + 0.02 || intensity >= 0.99))
                Display(parent);
            else if ((state == 1 || state == 2) && (intensity <= Threshold - 0.02 || intensity <= 0.01))
                Hide(parent);
        }

        public void Display(ShapeKeysFace parent)
        {
            if (state == 1 || state == 2)
                return;
            if (state == 3)
            {
                state = 1;
                return;
            }
            GameObject prefab = null;
            switch (Type)
            {
                case AccessoryType.WATER_DROP: prefab = parent.waterDropPrefab; break;
                case AccessoryType.LOVE_HEART: prefab = parent.heartPrefab; break;
            }
            instance = Instantiate(prefab);
            instance.transform.parent = parent.face;
            instance.transform.localPosition = Pos;
            instance.transform.localRotation = Rotation;
            instance.transform.localScale = Vector3.zero;
            parent.activeAccessories.Add(this);
            state = 1;
            parent.StartCoroutine(Fade(parent));
        }

        public void HideInstantly(ShapeKeysFace parent)
        {
            if (state == 0)
                return;
            GameObject tmpInstance = instance;
            Scheduler.RunSync(() => Destroy(tmpInstance));

            instance = null;
            parent.activeAccessories.Remove(this);
            state = 0;
        }

        public void Hide(ShapeKeysFace parent)
        {
            if (state == 0 || state == 3)
                return;
            if (state == 1)
            {
                state = 3;
                return;
            }
            state = 3;
            parent.StartCoroutine(Fade(parent));
        }

        private IEnumerator Fade(ShapeKeysFace parent)
        {
            for(;;)
            {
                if (state == 0 || state == 2)
                    yield break;

                Vector3 currC = instance.transform.localScale;
                Vector3 targetC = (state == 1) ? Scale : Vector3.zero;

                if (currC == targetC)
                {
                    if (state == 1)
                    {
                        state = 2;
                    }
                    else // state == 3
                    {
                        state = 0;
                        Destroy(instance);
                        instance = null;
                        parent.activeAccessories.Remove(this);
                    }
                    yield break;
                }
                
                if (Vector3.Distance(currC, targetC) < 0.001)
                    currC = targetC;
                else
                    currC = currC + SmoothAnimationCoefficient * (targetC - currC);

                instance.transform.localScale = currC;

                yield return null;
            }
        }
    }



}