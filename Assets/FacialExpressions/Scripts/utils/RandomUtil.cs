using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils
{
    public class RandomUtil
    {
        private RandomUtil() { }


        public static System.Random rand = new System.Random();


        public static float RandomGaussian(float mu = 0, float sigma = 1)
        {
            return mu + sigma * (Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
        }



        public static Vector3 RandomUniformVector3(Vector3 pos1, Vector3 pos2)
        {
            return new Vector3(Random.Range(pos1.x, pos2.x), Random.Range(pos1.y, pos2.y), Random.Range(pos1.z, pos2.z));
        }
        public static Vector3 RandomGaussianVector3(Vector3 center, Vector3 sigma)
        {
            return new Vector3(RandomGaussian(center.x, sigma.x), RandomGaussian(center.y, sigma.y), RandomGaussian(center.z, sigma.z));
        }










    }
}
