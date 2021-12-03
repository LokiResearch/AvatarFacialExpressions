using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRInputsAPI
{

    public abstract class IVRInput
    {
        internal abstract void UpdateState();
    }

    public class VRInput<T> : IVRInput
    {
        public delegate bool InputTryGetter<O>(out O output);

        public readonly string name;
        public readonly VRInputsManager inputs;
        protected InputTryGetter<T> inputGetter;

        /// <summary>
        /// True if the value is updated for the current frame, false otherwise
        /// </summary>
        public bool Available { get; protected set; }

        /// <summary>
        /// Value of this input in the previous frame
        /// </summary>
        public T Previous { get; protected set; }

        /// <summary>
        /// Value of this input in the current frame
        /// </summary>
        public T Value { get; protected set; }

        public float PreviousTime { get; protected set; }
        public float CurrentTime { get; protected set; }

        public float DeltaTime => CurrentTime - PreviousTime;

        internal VRInput(VRInputsManager inputs, string name, InputTryGetter<T> currentStateGetter)
        {
            this.inputs = inputs;
            this.name = name;
            inputGetter = currentStateGetter;
            if (inputGetter(out T v))
            {
                Previous = Value = v;
                Available = true;
            }
            else
            {
                Available = false;
            }
            PreviousTime = CurrentTime = Time.time;
        }

        internal override void UpdateState()
        {
            if (inputGetter(out T newV))
            {   // is available
                if (Available)
                {   // was already available in the previous frame
                    Previous = Value;
                    PreviousTime = CurrentTime;
                    Value = newV;
                    CurrentTime = Time.time;
                }
                else
                {   // was not available in the previous frame
                    Previous = Value = newV;
                    PreviousTime = CurrentTime = Time.time;
                    Available = true;
                }
            }
            else
            {   // is not available
                if (Available)
                {   // was available before
                    Previous = Value;
                    PreviousTime = CurrentTime;
                    Available = false;
                }
            }
            if (Available)
            {
                //LogChange();
            }
        }

        protected virtual void LogChange()
        {
            //Debug.Log("Input " + name + ", value = " + Value);
        }

        public static implicit operator T(VRInput<T> i) => i.Value;
    }







    public class VRBooleanInput : VRInput<bool>
    {
        internal VRBooleanInput(VRInputsManager inputs, string name, InputTryGetter<bool> currentStateGetter)
            : base(inputs, name, currentStateGetter) { }

        /// <summary>
        /// True if the state of this boolean input just changed to true, false otherwise.
        /// </summary>
        public bool Down
        {
            get => Value && !Previous;
        }

        /// <summary>
        /// True if the state of this boolean input just changed to false, false otherwise.
        /// </summary>
        public bool Up
        {
            get => !Value && Previous;
        }

        protected override void LogChange()
        {
            if (Previous != Value)
                Debug.Log("BooleanInput " + name + " changed: " + Previous + " -> " + Value);
        }
    }







    public class VR1DAxisInput : VRInput<float>
    {
        internal VR1DAxisInput(VRInputsManager inputs, string name, InputTryGetter<float> currentStateGetter)
            : base(inputs, name, currentStateGetter) { }

        /// <summary>
        /// Delta between the previous value and the current value (Value - Previous).
        /// </summary>
        public float Delta
        {
            get
            {
                if (float.IsInfinity(Value) || float.IsInfinity(Previous)
                || float.IsNaN(Value) || float.IsNaN(Previous))
                    return 0;
                return Value - Previous;
            }
        }

        /// <summary>
        /// Speed of which the value is changing over time, in units/seconds: Delta / DeltaTime.
        /// </summary>
        public float Speed => DeltaTime == 0 ? 0 : (Delta / DeltaTime);

        private Dictionary<Tuple<float, float>, VRThresholdedInput> thresholdedInputs = new Dictionary<Tuple<float, float>, VRThresholdedInput>();

        /// <summary>
        /// Returns the thresholded input for the specified threshhold interval, creating a new instance if necessary.
        /// A thresholded input represent a boolean which the value is based on the state of a 1 axis continuous input. The boolean value
        /// is determined according to the changes of the source value around the threshold interval.
        /// </summary>
        /// <param name="thUp">The lower value of the threshold interval. We consider "up" when the input is released by the user.</param>
        /// <param name="thDown">The upper value of the threshold interval. We consider "down" when the input is activated by the user.</param>
        /// <returns>An instance of VRThresholdedInput</returns>
        public VRThresholdedInput GetThresholdedInput(float thUp, float thDown)
        {
            if (thUp > thDown)
                throw new Exception("thUp must be less than or equal to thDown");

            Tuple<float, float> tKey = new Tuple<float, float>(thUp, thDown);
            return thresholdedInputs.ContainsKey(tKey) ? thresholdedInputs[tKey] : (thresholdedInputs[tKey] = new VRThresholdedInput(this, thUp, thDown));
        }

        internal override void UpdateState()
        {
            base.UpdateState();
            foreach (VRBooleanInput thInput in thresholdedInputs.Values)
                thInput.UpdateState();
        }

        protected override void LogChange()
        {
            if (Mathf.Abs(Delta) >= 0.2)
                Debug.Log("1DInput " + name + " changed: " + Previous + " -> " + Value);
        }
    }







    public class VRThresholdedInput : VRBooleanInput
    {
        public readonly VR1DAxisInput BaseInput;
        internal VRThresholdedInput(VR1DAxisInput axis, float thUp, float thDown)
            : base(axis.inputs, "thresholded input of " + axis.name, (out bool value) => { value = false; return false; })
        {
            BaseInput = axis;
            inputGetter = (out bool value) =>
            {
                if (!axis.Available) { value = false; return false; }
                bool p = Value; // previous value (not stored in Previous yet)
                bool d = axis > thDown, u = axis < thUp;
                value = (p && (d || !u)) || (d && !u);
                return true;
            };
        }
    }




    public class VRMultiThresholdedInput : VRBooleanInput
    {
        internal VRMultiThresholdedInput(VRThresholdedInput first, params VRThresholdedInput[] others)
            : base(first.inputs, "multi thresholded input of " + first.BaseInput.name + " and others", (out bool value) => { value = false; return false; })
        {
            inputGetter = (out bool value) =>
            {
                bool p = Value; // previous value (not stored in Previous yet)

                value = false;
                if (first.Available)
                {
                    if (first.Down) { value = true; return true; }
                    if (first.Up) { value = false; return true; }
                }
                else
                    return false;
                foreach (VRThresholdedInput curr in others)
                {
                    if (curr.Available)
                    {
                        if (curr.Down) { value = true; return true; }
                        if (curr.Up) { value = false; return true; }
                    }
                    else
                        return false;
                }
                value = p; // the value does not change
                return true;
            };
        }
    }







    public class VR2DAxisInput : VRInput<Vector2>
    {
        public readonly VR1DAxisInput x;
        public readonly VR1DAxisInput y;
        internal VR2DAxisInput(VRInputsManager inputs, string name, InputTryGetter<Vector2> currentStateGetter)
            : base(inputs, name, currentStateGetter)
        {
            x = new VR1DAxisInput(inputs, name + ".x", (out float v) =>
            {
                if (Available) { v = Value.x; return true; }
                else { v = 0; return false; }
            });
            y = new VR1DAxisInput(inputs, name + ".y", (out float v) =>
            {
                if (Available) { v = Value.y; return true; }
                else { v = 0; return false; }
            });
        }

        internal override void UpdateState()
        {
            base.UpdateState();
            x.UpdateState();
            y.UpdateState();
        }

        protected override void LogChange()
        {
            if (Delta.magnitude >= 0.2)
                Debug.Log("2DInput " + name + " changed: " + Previous + " -> " + Value);
        }


        public Vector2 Delta => new Vector2(x.Delta, y.Delta);

        /// <summary>
        /// Speed of which the value is changing over time, in units/seconds: Delta / DeltaTime.
        /// </summary>
        public Vector2 Speed => DeltaTime == 0 ? Vector2.zero : (Delta / DeltaTime);

        /// <summary>
        /// Retourne l'angle (atan2) d'orientation du vecteur ayant comme point d'origine center
        /// et comme point de destination la valeur de l'input courant.
        /// </summary>
        /// <param name="center">Le centre de rotation</param>
        /// <returns>L'angle calculé en radian, entre -PI et PI</returns>
        public float GetAngle(Vector2 center) => GetAngle(center, Value);


        /// <summary>
        /// Retourne l'angle (atan2) d'orientation du vecteur ayant comme point d'origine (0; 0)
        /// et comme point de destination la valeur de l'input courant.
        /// </summary>
        /// <returns>L'angle calculé en radian, entre -PI et PI</returns>
        public float GetAngle() => GetAngle(Vector2.zero);



        /// <summary>
        /// Retourne le delta d'angle d'orientation du vecteur ayant comme point d'origine center
        /// et comme point de destination la valeur de l'input courant.
        /// Une valeur positive va dans le sens des aiguilles d'une montre.
        /// </summary>
        /// <param name="center">Le centre de rotation</param>
        /// <returns>Un delta d'angle calculé en radian</returns>
        public float GetDeltaOfAngle(Vector2 center) => GetDeltaOfAngle(center, Previous, Value);



        /// <summary>
        /// Retourne le delta d'angle d'orientation du vecteur ayant comme point d'origine (0; 0)
        /// et comme point de destination la valeur de l'input courant.
        /// Une valeur positive va dans le sens des aiguilles d'une montre.
        /// </summary>
        /// <returns>Un delta d'angle calculé en radian</returns>
        public float GetDeltaOfAngle() => GetDeltaOfAngle(Vector2.zero);




        public float GetAngleOfDelta() => GetAngle(Vector2.zero, Delta);








        /// <summary>
        /// Retourne l'angle (atan2) d'orientation du vecteur ayant comme point d'origine center
        /// et comme point de destination current.
        /// </summary>
        /// <param name="center">Le centre de rotation</param>
        /// <param name="current">Le point de destination</param>
        /// <returns>L'angle calculé en radian, entre -PI et PI</returns>
        public static float GetAngle(Vector2 center, Vector2 current)
        {
            if (float.IsInfinity(center.x) || float.IsInfinity(center.y)
                || float.IsNaN(center.x) || float.IsNaN(center.y))
                throw new ArgumentException("Les coordonnées center ne peuvent être infini ou NaN");

            if (float.IsNaN(current.x) || float.IsNaN(current.y))
                return float.NaN;

            if (float.IsInfinity(current.x) && float.IsInfinity(current.y))
                return float.NaN;
            if (float.IsPositiveInfinity(current.x))
                return 0;
            if (float.IsPositiveInfinity(current.y))
                return Mathf.PI / 2;
            if (float.IsNegativeInfinity(current.x))
                return Mathf.PI;
            if (float.IsNegativeInfinity(current.y))
                return -Mathf.PI / 2;

            return Mathf.Atan2(current.y - center.y, current.x - center.x);
        }



        /// <summary>
        /// Retourne le delta d'angle d'orientation du vecteur ayant comme point d'origine center
        /// et comme point de destination la valeur de l'input courant.
        /// Une valeur positive va dans le sens des aiguilles d'une montre.
        /// </summary>
        /// <param name="center">Le centre de rotation</param>
        /// <param name="previous">Le point de destination à l'état précédent</param>
        /// <param name="current">Le point de destination à l'état courant</param>
        /// <returns>Un delta d'angle calculé en radian</returns>
        public static float GetDeltaOfAngle(Vector2 center, Vector2 previous, Vector2 current)
        {
            float aPrev = GetAngle(center, previous);
            float aCurr = GetAngle(center, current);

            if (float.IsNaN(aPrev) || float.IsNaN(aCurr))
                return 0;

            float aDiff = aPrev - aCurr;
            if (aDiff < -Mathf.PI)
                aDiff += 2 * Mathf.PI;
            if (aDiff > Mathf.PI)
                aDiff -= 2 * Mathf.PI;

            return aDiff;

        }

    }





    public class VRCircularTouchpad : VR2DAxisInput
    {
        /// <summary>
        /// Physical radius of the eliptical touchpad, in meter.
        /// </summary>
        public Vector2 Radius { get; protected set; }

        internal VRCircularTouchpad(VR2DAxisInput input, Func<bool> isTouching, Vector2 radius)
            : base(input.inputs, "circular touchpad " + input.name, (out Vector2 v) =>
            {
                if (input.Available && (isTouching == null || isTouching.Invoke())) { v = input.Value; return true; }
                v = Vector2.zero; return false;
            })
        {
            Radius = radius;
        }

        internal VRCircularTouchpad(VR2DAxisInput input, Func<bool> isTouching, float radius)
            : this(input, isTouching, new Vector2(radius, radius))
        {

        }



        /// <summary>
        /// Position of the finger relative to the center of the touchpad. The unit of the coordinate are in meter, based on the value of Radius.
        /// It assumes the raw input being in [-1; 1].
        /// </summary>
        public Vector2 ValueInMeter => Value * Radius;



        public Vector2 DeltaInMeterPerSecond => Delta * Radius / Time.deltaTime;


    }







    public class VR3DAxisInput : VRInput<Vector3>
    {
        public readonly VR1DAxisInput x;
        public readonly VR1DAxisInput y;
        public readonly VR1DAxisInput z;
        internal VR3DAxisInput(VRInputsManager inputs, string name, InputTryGetter<Vector3> currentStateGetter)
            : base(inputs, name, currentStateGetter)
        {
            x = new VR1DAxisInput(inputs, name + ".x", (out float v) =>
            {
                if (Available) { v = Value.x; return true; }
                else { v = 0; return false; }
            });
            y = new VR1DAxisInput(inputs, name + ".y", (out float v) =>
            {
                if (Available) { v = Value.y; return true; }
                else { v = 0; return false; }
            });
            z = new VR1DAxisInput(inputs, name + ".z", (out float v) =>
            {
                if (Available) { v = Value.z; return true; }
                else { v = 0; return false; }
            });
        }

        internal override void UpdateState()
        {
            base.UpdateState();
            x.UpdateState();
            y.UpdateState();
            z.UpdateState();
        }

        protected override void LogChange()
        {
            if (Previous != Value)
                Debug.Log("3DInput " + name + " changed: " + Previous + " -> " + Value);
        }


        public Vector3 Delta => new Vector3(x.Delta, y.Delta, z.Delta);

        /// <summary>
        /// Speed of which the value is changing over time, in units/seconds: Delta / DeltaTime.
        /// </summary>
        public Vector2 Speed => DeltaTime == 0 ? Vector3.zero : (Delta / DeltaTime);

    }

    public class VRQuaternionInput : VRInput<Quaternion>
    {
        public readonly VR1DAxisInput x;
        public readonly VR1DAxisInput y;
        public readonly VR1DAxisInput z;
        public readonly VR1DAxisInput w;
        internal VRQuaternionInput(VRInputsManager inputs, string name, InputTryGetter<Quaternion> currentStateGetter)
            : base(inputs, name, currentStateGetter)
        {
            x = new VR1DAxisInput(inputs, name + ".x", (out float v) =>
            {
                if (Available) { v = Value.x; return true; }
                else { v = 0; return false; }
            });
            y = new VR1DAxisInput(inputs, name + ".y", (out float v) =>
            {
                if (Available) { v = Value.y; return true; }
                else { v = 0; return false; }
            });
            z = new VR1DAxisInput(inputs, name + ".z", (out float v) =>
            {
                if (Available) { v = Value.z; return true; }
                else { v = 0; return false; }
            });
            w = new VR1DAxisInput(inputs, name + ".w", (out float v) =>
            {
                if (Available) { v = Value.w; return true; }
                else { v = 0; return false; }
            });
        }

        internal override void UpdateState()
        {
            base.UpdateState();
            x.UpdateState();
            y.UpdateState();
            z.UpdateState();
            w.UpdateState();
        }

        protected override void LogChange()
        {
            if (Previous != Value)
                Debug.Log("QuaternionInput " + name + " changed: " + Previous + " -> " + Value);
        }


        public Quaternion Delta => new Quaternion(x.Delta, y.Delta, z.Delta, w.Delta);

    }
}
