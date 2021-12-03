using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace utils
{

    public delegate Vector3 PositionProducer();
    public delegate float RadiusProducer();
    public delegate bool PositionValidator(Vector3 pos, float radius);
    public delegate String StringProducer();
    public delegate float FloatProducer();
    public delegate void FloatConsumer(float v);
    public delegate float FloatBinaryOp(float deltaAngle, float oldValue);

    public delegate T UnaryOperator<T>(T input);

    public delegate T[] ArrayProducer<T>();



    public delegate void Runnable();
    public static class RunnableMethods
    {
        public static Runnable Then(this Runnable current, Runnable next)
        {
            return () => { current(); next(); };
        }
    }
    
}

