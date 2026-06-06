using System;
using UnityEngine;

namespace Modular2DCharacterController.Data
{
    [CreateAssetMenu(
        fileName = "HorizontalMovementProfile",
        menuName = "Modular 2D Character Controller/Horizontal Movement Profile")]
    public class HorizontalMovementProfile : FeatureProfile
    {
        [Header("Movement Settings")]
        [Min(0)]
        public float maxSpeed;

        [Min(0)]
        public float acceleration;

        [Min(0)]
        public float deceleration;

        [Min(0)]
        public float turnAcceleration;
    }
}