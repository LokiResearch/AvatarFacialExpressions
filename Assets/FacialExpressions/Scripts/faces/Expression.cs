using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Expression
{
    /*
     * Naming convention :
     * Emojipedia name of the emoji, without "Face" word
     * Example : Slightly Smiling Face -> SLIGHTLY_SMILING
     */
    NEUTRAL,
    GRINNING,
    GRINNIG_WITH_BIG_EYES,
    GRINNING_WITH_SMILING_EYES,
    BEAMING_WITH_SMILING_EYES,
    GRINNING_SQUINTING,
    GRINNING_WITH_SWEAT,
    ROLLING_ON_THE_FLOOR_LAUGHING,
    WITH_TEARS_OF_JOY,
    SLIGHTLY_SMILING,
    WINKING,
    SMILING_WITH_SMILING_EYES,
    SMILING_WITH_HALO,
    SMILING_WITH_HEARTS,
    SMILING_WITH_HEART_EYES,
    STAR_STRUCK,
    BLOWING_A_KISS,
    KISSING,
    SMILING,
    KISSING_WITH_CLOSED_EYES,
    KISSING_WITH_SMILING_EYES,
    SAVORING_FOOD,
    WITH_TONGUE,
    WINKING_WITH_TONGUE,
    ZANY, // crazy face
    SQUINTING_WITH_TONGUE,
    MONEY_MOUTH,
    SHUSHING, // shhht (should be applied when the user put the virtual finger in front of the mouth)
    THINKING, // (should be applied when the user put the virtual hand with a specific position and finger pose in front of the mouth)
    ZIPPER_MOUTH,
    WITH_RAISED_EYEBROW,
    EXPRESSIONLESS,
    WITHOUT_MOUTH,
    SMIRKING, // see if we dont need specific eyes
    UNAMUSED, // see if we dont need specific eyes
    WITH_ROLLING_EYES, // see if we dont need specific eyes
    GRIMACING,
    LYING_FACE,
    RELIEVED,
    PENSIVE,
    SLEEPY,
    DROOLING,
    SLEEPING,
    WITH_MEDICAL_MASK,
    WITH_THERMOMETER,
    WITH_HEAD_BANDAGE,
    NAUSEATED,
    VOMITING,
    HOT,
    COLD,
    WOOZY,
    DIZZY, // chocked with dead eyes
    EXPLODING_HEAD,
    COWBOY_HAT,
    SMILING_WITH_SUNGLASSES,
    NERD,
    WITH_MONOCLE,
    CONFUSED,
    WORRIED,
    SLIGHTLY_FROWNING,
    FROWNING,
    WITH_OPEN_MOUTH,
    HUSHED,
    ASTONISHED,
    FLUSHED, // represent some sort of shame/shiness
    PLEADING, // see if we dont need specific eyes for animation
    FROWNING_WITH_OPEN_MOUTH,
    ANGUISHED,
    FEARFULL,
    ANXIOUS_WITH_SWEAT,
    SAD_BUT_RELIEVED,
    CRYING,
    LOUDLY_CRYING,
    SCREAMING_IN_FEAR,
    CONFOUNDED,
    PERSEVERING,
    DISAPPOINTED,
    DOWNCAST_WITH_SWEAT, // downcast = abattu // needs water drop
    WEARY,
    TIRED,
    YAWNING, // (should be applied when the user put the virtual hand opened and facing the mouth)
    WITH_STEAM_FROM_NOSE,
    POUTING, // very angry face
    ANGRY,
    WITH_SYMBOLS_ON_MOUTH,
}

public static class ExpressionMethods
{
    public static Texture LoadTexture(this Expression e)
    {
        return Resources.Load<Texture>("Emojis/" + e.ToString());
    }
    
    public static string ToDisplayString(this Expression e, bool english)
    {
        (string fr, string en) = displayStrings[e];
        return english ? en : fr;
    }



    private static Dictionary<Expression, (string, string)> displayStrings = new Dictionary<Expression, (string, string)>()
    {
        [Expression.NEUTRAL] = ("", ""),
        [Expression.GRINNING] = ("Souriant", "Grinning"),
        [Expression.GRINNIG_WITH_BIG_EYES] = ("", ""),
        [Expression.GRINNING_WITH_SMILING_EYES] = ("", ""),
        [Expression.BEAMING_WITH_SMILING_EYES] = ("Rayonnant avec des yeux souriants", "Beaming with smiling eyes"),
        [Expression.GRINNING_SQUINTING] = ("", ""),
        [Expression.GRINNING_WITH_SWEAT] = ("", ""),
        [Expression.ROLLING_ON_THE_FLOOR_LAUGHING] = ("", ""),
        [Expression.WITH_TEARS_OF_JOY] = ("", ""),
        [Expression.SLIGHTLY_SMILING] = ("Souriant légèrement", "Slightly smiling"),
        [Expression.WINKING] = ("", ""),
        [Expression.SMILING_WITH_SMILING_EYES] = ("", ""),
        [Expression.SMILING_WITH_HALO] = ("", ""),
        [Expression.SMILING_WITH_HEARTS] = ("Souriant avec des cœurs", "Smiling with hearts"),
        [Expression.SMILING_WITH_HEART_EYES] = ("", ""),
        [Expression.STAR_STRUCK] = ("", ""),
        [Expression.BLOWING_A_KISS] = ("", ""),
        [Expression.KISSING] = ("", ""),
        [Expression.SMILING] = ("", ""),
        [Expression.KISSING_WITH_CLOSED_EYES] = ("", ""),
        [Expression.KISSING_WITH_SMILING_EYES] = ("", ""),
        [Expression.SAVORING_FOOD] = ("", ""),
        [Expression.WITH_TONGUE] = ("", ""),
        [Expression.WINKING_WITH_TONGUE] = ("", ""),
        [Expression.ZANY] = ("", ""),
        [Expression.SQUINTING_WITH_TONGUE] = ("", ""),
        [Expression.MONEY_MOUTH] = ("", ""),
        [Expression.SHUSHING] = ("", ""),
        [Expression.THINKING] = ("En réflexion", "Thinking"),
        [Expression.ZIPPER_MOUTH] = ("", ""),
        [Expression.WITH_RAISED_EYEBROW] = ("", ""),
        [Expression.EXPRESSIONLESS] = ("", ""),
        [Expression.WITHOUT_MOUTH] = ("", ""),
        [Expression.SMIRKING] = ("", ""),
        [Expression.UNAMUSED] = ("", ""),
        [Expression.WITH_ROLLING_EYES] = ("", ""),
        [Expression.GRIMACING] = ("", ""),
        [Expression.LYING_FACE] = ("", ""),
        [Expression.RELIEVED] = ("", ""),
        [Expression.PENSIVE] = ("", ""),
        [Expression.SLEEPY] = ("", ""),
        [Expression.DROOLING] = ("", ""),
        [Expression.SLEEPING] = ("", ""),
        [Expression.WITH_MEDICAL_MASK] = ("", ""),
        [Expression.WITH_THERMOMETER] = ("", ""),
        [Expression.WITH_HEAD_BANDAGE] = ("", ""),
        [Expression.NAUSEATED] = ("", ""),
        [Expression.VOMITING] = ("", ""),
        [Expression.HOT] = ("", ""),
        [Expression.COLD] = ("", ""),
        [Expression.WOOZY] = ("", ""),
        [Expression.DIZZY] = ("", ""),
        [Expression.EXPLODING_HEAD] = ("", ""),
        [Expression.COWBOY_HAT] = ("", ""),
        [Expression.SMILING_WITH_SUNGLASSES] = ("", ""),
        [Expression.NERD] = ("", ""),
        [Expression.WITH_MONOCLE] = ("", ""),
        [Expression.CONFUSED] = ("", ""),
        [Expression.WORRIED] = ("", ""),
        [Expression.SLIGHTLY_FROWNING] = ("", ""),
        [Expression.FROWNING] = ("Pas content", "Frowning"), // trouver meilleure trad en fr
        [Expression.WITH_OPEN_MOUTH] = ("Bouche ouverte", "Open mouth"),
        [Expression.HUSHED] = ("", ""),
        [Expression.ASTONISHED] = ("", ""),
        [Expression.FLUSHED] = ("", ""),
        [Expression.PLEADING] = ("", ""),
        [Expression.FROWNING_WITH_OPEN_MOUTH] = ("", ""),
        [Expression.ANGUISHED] = ("", ""),
        [Expression.FEARFULL] = ("", ""),
        [Expression.ANXIOUS_WITH_SWEAT] = ("", ""),
        [Expression.SAD_BUT_RELIEVED] = ("", ""),
        [Expression.CRYING] = ("", ""),
        [Expression.LOUDLY_CRYING] = ("", ""),
        [Expression.SCREAMING_IN_FEAR] = ("", ""),
        [Expression.CONFOUNDED] = ("", ""),
        [Expression.PERSEVERING] = ("", ""),
        [Expression.DISAPPOINTED] = ("", ""),
        [Expression.DOWNCAST_WITH_SWEAT] = ("", ""),
        [Expression.WEARY] = ("", ""),
        [Expression.TIRED] = ("", ""),
        [Expression.YAWNING] = ("", ""),
        [Expression.WITH_STEAM_FROM_NOSE] = ("", ""),
        [Expression.POUTING] = ("", ""),
        [Expression.ANGRY] = ("En colère", "Angry"),
        [Expression.WITH_SYMBOLS_ON_MOUTH] = ("", ""),
    };
}