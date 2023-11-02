using UnityEditor;
using UnityEngine;

namespace Assets.Scripts
{
    public class RopePoint
    {
        public Vector3 currentPosition;
        public Vector3 oldPosition;
        public float mass;

        public RopePoint(Vector3 position, float mass)
        {
            currentPosition = position;
            oldPosition = position;
            this.mass = mass;
        }
    }
}