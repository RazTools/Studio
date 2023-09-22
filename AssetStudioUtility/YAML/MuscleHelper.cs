using System;

namespace AssetStudio
{
    public enum HumanoidMuscleType
    {
        Motion = 0,
        Root = Motion + 7,
        Limbs = Root + 7,
        Muscles = Limbs + LimbType.Last * 7,
        Fingers = Muscles + MuscleType.Last,
        TDoFBones = Fingers + ArmType.Last * FingerType.Last * FingerDoFType.Last,

        Last = TDoFBones + TDoFBoneType.Last * 3,
    }

    public static class AnimationMuscleTypeExtensions
    {
        public static HumanoidMuscleType Update(this HumanoidMuscleType _this, int[] version)
        {
            if (_this < HumanoidMuscleType.Muscles)
            {
                return _this;
            }

            MuscleType muscle = (MuscleType)(_this - HumanoidMuscleType.Muscles);
            MuscleType fixedMuscle = muscle.Update(version);
            _this = HumanoidMuscleType.Muscles + (int)fixedMuscle;
            if (_this < HumanoidMuscleType.TDoFBones)
            {
                return _this;
            }

            TDoFBoneType tdof = (TDoFBoneType)(_this - HumanoidMuscleType.TDoFBones);
            TDoFBoneType fixedTdof = tdof.Update(version);
            _this = HumanoidMuscleType.TDoFBones + (int)fixedTdof;
            return _this;
        }

        public static string ToAttributeString(this HumanoidMuscleType _this)
        {
            if (_this < HumanoidMuscleType.Root)
            {
                int delta = _this - HumanoidMuscleType.Motion;
                return nameof(HumanoidMuscleType.Motion) + GetTransformPostfix(delta % 7);
            }
            if (_this < HumanoidMuscleType.Limbs)
            {
                int delta = _this - HumanoidMuscleType.Root;
                return nameof(HumanoidMuscleType.Root) + GetTransformPostfix(delta % 7);
            }
            if (_this < HumanoidMuscleType.Muscles)
            {
                int delta = _this - HumanoidMuscleType.Limbs;
                LimbType limb = (LimbType)(delta / 7);
                return limb.ToBoneType().ToAttributeString() + GetTransformPostfix(delta % 7);
            }
            if (_this < HumanoidMuscleType.Fingers)
            {
                int delta = _this - HumanoidMuscleType.Muscles;
                MuscleType muscle = (MuscleType)delta;
                return muscle.ToAttributeString();
            }
            if (_this < HumanoidMuscleType.TDoFBones)
            {
                const int armSize = (int)FingerType.Last * (int)FingerDoFType.Last;
                const int dofSize = (int)FingerDoFType.Last;
                int delta = _this - HumanoidMuscleType.Fingers;
                ArmType arm = (ArmType)(delta / armSize);
                delta = delta % armSize;
                FingerType finger = (FingerType)(delta / dofSize);
                delta = delta % dofSize;
                FingerDoFType dof = (FingerDoFType)delta;
                return $"{arm.ToBoneType().ToAttributeString()}.{finger.ToAttributeString()}.{dof.ToAttributeString()}";
            }
            if (_this < HumanoidMuscleType.Last)
            {
                int delta = _this - HumanoidMuscleType.TDoFBones;
                TDoFBoneType tdof = (TDoFBoneType)(delta / 3);
                return $"{tdof.ToBoneType().ToAttributeString()}{GetTDoFTransformPostfix(delta % 3)}";
            }
            throw new ArgumentException(_this.ToString());
        }

        private static string GetTransformPostfix(int index)
        {
            switch (index)
            {
                case 0:
                    return "T.x";
                case 1:
                    return "T.y";
                case 2:
                    return "T.z";

                case 3:
                    return "Q.x";
                case 4:
                    return "Q.y";
                case 5:
                    return "Q.z";
                case 6:
                    return "Q.w";

                default:
                    throw new ArgumentException(index.ToString());
            }
        }

        private static string GetTDoFTransformPostfix(int index)
        {
            switch (index)
            {
                case 0:
                    return "TDOF.x";
                case 1:
                    return "TDOF.y";
                case 2:
                    return "TDOF.z";

                default:
                    throw new ArgumentException(index.ToString());
            }
        }
    }

    public enum LimbType
    {
        LeftFoot = 0,
        RightFoot = 1,
        LeftHand = 2,
        RightHand = 3,

        Last,
    }

    public static class LimbTypeExtensions
    {
        public static BoneType ToBoneType(this LimbType _this)
        {
            switch (_this)
            {
                case LimbType.LeftFoot:
                    return BoneType.LeftFoot;
                case LimbType.RightFoot:
                    return BoneType.RightFoot;
                case LimbType.LeftHand:
                    return BoneType.LeftHand;
                case LimbType.RightHand:
                    return BoneType.RightHand;

                default:
                    throw new ArgumentException(_this.ToString());
            }
        }
    }

    public enum MuscleType
    {
        SpineFrontBack = 0,
        SpineLeftRight = 1,
        SpineTwistLeftRight = 2,
        ChestFrontBack = 3,
        ChestLeftRight = 4,
        ChestTwistLeftRight = 5,
        UpperchestFrontBack = 6,
        UpperchestLeftRight = 7,
        UpperchestTwisLeftRight = 8,
        NeckNodDownUp = 9,
        NeckTiltLeftRight = 10,
        NeckTurnLeftRight = 11,
        HeadNodDownUp = 12,
        HeadTiltLeftRight = 13,
        HeadTurnLeftRight = 14,
        LeftEyeDownUp = 15,
        LeftEyeInOut = 16,
        RightEyeDownUp = 17,
        RightEyeInOut = 18,
        JawClose = 19,
        JawLeftRight = 20,
        LeftUpperLegFrontBack = 21,
        LeftUpperLegInOut = 22,
        LeftUpperLegTwistInOut = 23,
        LeftLowerLegStretch = 24,
        LeftLowerLegTwistInOut = 25,
        LeftFootUpDown = 26,
        LeftFootTwistInOut = 27,
        LeftToesUpDown = 28,
        RightUpperLegFrontBack = 29,
        RightUpperLegInOut = 30,
        RightUpperLegTwistInOut = 31,
        RightLowerLegStretch = 32,
        RightLowerLegTwistInOut = 33,
        RightFootUpDown = 34,
        RightFootTwistInOut = 35,
        RightToesUpDown = 36,
        LeftShoulderDownUp = 37,
        LeftShoulderFrontBack = 38,
        LeftArmDownUp = 39,
        LeftArmFrontBack = 40,
        LeftArmTwistInOut = 41,
        LeftForearmStretch = 42,
        LeftForearmTwistInOut = 43,
        LeftHandDownUp = 44,
        LeftHandInOut = 45,
        RightShoulderDownUp = 46,
        RightShoulderFrontBack = 47,
        RightArmDownUp = 48,
        RightArmFrontBack = 49,
        RightArmTwistInOut = 50,
        RightForearmStretch = 51,
        RightForearmTwistInOut = 52,
        RightHandDownUp = 53,
        RightHandInOut = 54,

        Last,
    }

    public static class MuscleTypeExtensions
    {
        public static MuscleType Update(this MuscleType _this, int[] version)
        {
            if (!(version[0] > 5 || (version[0] == 5 && version[1] >= 6)))
            {
                if (_this >= MuscleType.UpperchestFrontBack)
                {
                    _this += 3;
                }
            }
            return _this;
        }

        public static string ToAttributeString(this MuscleType _this)
        {
            switch (_this)
            {
                case MuscleType.SpineFrontBack:
                    return "Spine Front-Back";
                case MuscleType.SpineLeftRight:
                    return "Spine Left-Right";
                case MuscleType.SpineTwistLeftRight:
                    return "Spine Twist Left-Right";
                case MuscleType.ChestFrontBack:
                    return "Chest Front-Back";
                case MuscleType.ChestLeftRight:
                    return "Chest Left-Right";
                case MuscleType.ChestTwistLeftRight:
                    return "Chest Twist Left-Right";
                case MuscleType.UpperchestFrontBack:
                    return "UpperChest Front-Back";
                case MuscleType.UpperchestLeftRight:
                    return "UpperChest Left-Right";
                case MuscleType.UpperchestTwisLeftRight:
                    return "UpperChest Twist Left-Right";
                case MuscleType.NeckNodDownUp:
                    return "Neck Nod Down-Up";
                case MuscleType.NeckTiltLeftRight:
                    return "Neck Tilt Left-Right";
                case MuscleType.NeckTurnLeftRight:
                    return "Neck Turn Left-Right";
                case MuscleType.HeadNodDownUp:
                    return "Head Nod Down-Up";
                case MuscleType.HeadTiltLeftRight:
                    return "Head Tilt Left-Right";
                case MuscleType.HeadTurnLeftRight:
                    return "Head Turn Left-Right";
                case MuscleType.LeftEyeDownUp:
                    return "Left Eye Down-Up";
                case MuscleType.LeftEyeInOut:
                    return "Left Eye In-Out";
                case MuscleType.RightEyeDownUp:
                    return "Right Eye Down-Up";
                case MuscleType.RightEyeInOut:
                    return "Right Eye In-Out";
                case MuscleType.JawClose:
                    return "Jaw Close";
                case MuscleType.JawLeftRight:
                    return "Jaw Left-Right";
                case MuscleType.LeftUpperLegFrontBack:
                    return "Left Upper Leg Front-Back";
                case MuscleType.LeftUpperLegInOut:
                    return "Left Upper Leg In-Out";
                case MuscleType.LeftUpperLegTwistInOut:
                    return "Left Upper Leg Twist In-Out";
                case MuscleType.LeftLowerLegStretch:
                    return "Left Lower Leg Stretch";
                case MuscleType.LeftLowerLegTwistInOut:
                    return "Left Lower Leg Twist In-Out";
                case MuscleType.LeftFootUpDown:
                    return "Left Foot Up-Down";
                case MuscleType.LeftFootTwistInOut:
                    return "Left Foot Twist In-Out";
                case MuscleType.LeftToesUpDown:
                    return "Left Toes Up-Down";
                case MuscleType.RightUpperLegFrontBack:
                    return "Right Upper Leg Front-Back";
                case MuscleType.RightUpperLegInOut:
                    return "Right Upper Leg In-Out";
                case MuscleType.RightUpperLegTwistInOut:
                    return "Right Upper Leg Twist In-Out";
                case MuscleType.RightLowerLegStretch:
                    return "Right Lower Leg Stretch";
                case MuscleType.RightLowerLegTwistInOut:
                    return "Right Lower Leg Twist In-Out";
                case MuscleType.RightFootUpDown:
                    return "Right Foot Up-Down";
                case MuscleType.RightFootTwistInOut:
                    return "Right Foot Twist In-Out";
                case MuscleType.RightToesUpDown:
                    return "Right Toes Up-Down";
                case MuscleType.LeftShoulderDownUp:
                    return "Left Shoulder Down-Up";
                case MuscleType.LeftShoulderFrontBack:
                    return "Left Shoulder Front-Back";
                case MuscleType.LeftArmDownUp:
                    return "Left Arm Down-Up";
                case MuscleType.LeftArmFrontBack:
                    return "Left Arm Front-Back";
                case MuscleType.LeftArmTwistInOut:
                    return "Left Arm Twist In-Out";
                case MuscleType.LeftForearmStretch:
                    return "Left Forearm Stretch";
                case MuscleType.LeftForearmTwistInOut:
                    return "Left Forearm Twist In-Out";
                case MuscleType.LeftHandDownUp:
                    return "Left Hand Down-Up";
                case MuscleType.LeftHandInOut:
                    return "Left Hand In-Out";
                case MuscleType.RightShoulderDownUp:
                    return "Right Shoulder Down-Up";
                case MuscleType.RightShoulderFrontBack:
                    return "Right Shoulder Front-Back";
                case MuscleType.RightArmDownUp:
                    return "Right Arm Down-Up";
                case MuscleType.RightArmFrontBack:
                    return "Right Arm Front-Back";
                case MuscleType.RightArmTwistInOut:
                    return "Right Arm Twist In-Out";
                case MuscleType.RightForearmStretch:
                    return "Right Forearm Stretch";
                case MuscleType.RightForearmTwistInOut:
                    return "Right Forearm Twist In-Out";
                case MuscleType.RightHandDownUp:
                    return "Right Hand Down-Up";
                case MuscleType.RightHandInOut:
                    return "Right Hand In-Out";

                default:
                    throw new ArgumentException(_this.ToString());
            }
        }
    }

    public enum BoneType
    {
        Hips = 0,
        LeftUpperLeg = 1,
        RightUpperLeg = 2,
        LeftLowerLeg = 3,
        RightLowerLeg = 4,
        LeftFoot = 5,
        RightFoot = 6,
        Spine = 7,
        Chest = 8,
        UpperChest = 9,
        Neck = 10,
        Head = 11,
        LeftShoulder = 12,
        RightShoulder = 13,
        LeftUpperArm = 14,
        RightUpperArm = 15,
        LeftLowerArm = 16,
        RightLowerArm = 17,
        LeftHand = 18,
        RightHand = 19,
        LeftToes = 20,
        RightToes = 21,
        LeftEye = 22,
        RightEye = 23,
        Jaw = 24,

        Last,
    }

    public static class BoneTypeExtensions
    {
        public static BoneType Update(this BoneType _this, int[] version)
        {
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6))
            {
                if (_this >= BoneType.UpperChest)
                {
                    _this++;
                }
            }
            return _this;
        }

        public static string ToAttributeString(this BoneType _this)
        {
            if (_this < BoneType.Last)
            {
                return _this.ToString();
            }
            throw new ArgumentException(_this.ToString());
        }
    }

    public enum TransformType
    {
        Translation = 1,
        Rotation = 2,
        Scaling = 3,
        EulerRotation = 4,
    }

    public static class BindingTypeExtensions
    {
        public static bool IsValid(this TransformType _this)
        {
            return _this >= TransformType.Translation && _this <= TransformType.EulerRotation;
        }

        public static int GetDimension(this TransformType _this)
        {
            switch (_this)
            {
                case TransformType.Translation:
                case TransformType.Scaling:
                case TransformType.EulerRotation:
                    return 3;

                case TransformType.Rotation:
                    return 4;

                default:
                    throw new NotImplementedException($"Binding type {_this} is not implemented");
            }
        }
    }

    public enum ArmType
    {
        LeftHand = 0,
        RightHand = 1,

        Last,
    }

    public static class ArmTypeExtensions
    {
        public static BoneType ToBoneType(this ArmType _this)
        {
            switch (_this)
            {
                case ArmType.LeftHand:
                    return BoneType.LeftHand;
                case ArmType.RightHand:
                    return BoneType.RightHand;

                default:
                    throw new ArgumentException(_this.ToString());
            }
        }
    }

    public enum FingerType
    {
        Thumb = 0,
        Index = 1,
        Middle = 2,
        Ring = 3,
        Little = 4,

        Last,
    }

    public static class FingerTypeExtensions
    {
        public static string ToAttributeString(this FingerType _this)
        {
            if (_this < FingerType.Last)
            {
                return _this.ToString();
            }
            throw new ArgumentException(_this.ToString());
        }
    }

    public enum FingerDoFType
    {
        _1Stretched = 0,
        Spread = 1,
        _2Stretched = 2,
        _3Stretched = 3,

        Last,
    }

    public static class FingerDoFTypeExtensions
    {
        public static string ToAttributeString(this FingerDoFType _this)
        {
            switch (_this)
            {
                case FingerDoFType._1Stretched:
                    return "1 Stretched";
                case FingerDoFType.Spread:
                    return "Spread";
                case FingerDoFType._2Stretched:
                    return "2 Stretched";
                case FingerDoFType._3Stretched:
                    return "3 Stretched";

                default:
                    throw new ArgumentException(_this.ToString());
            }
        }
    }
    public enum TDoFBoneType
    {
        Spine = 0,
        Chest = 1,
        UpperChest = 2,
        Neck = 3,
        Head = 4,
        LeftUpperLeg = 5,
        LeftLowerLeg = 6,
        LeftFoot = 7,
        LeftToes = 8,
        RightUpperLeg = 9,
        RightLowerLeg = 10,
        RightFoot = 11,
        RightToes = 12,
        LeftShoulder = 13,
        LeftUpperArm = 14,
        LeftLowerArm = 15,
        LeftHand = 16,
        RightShoulder = 17,
        RightUpperArm = 18,
        RightLowerArm = 19,
        RightHand = 20,

        Last,
    }

    public static class TDoFBoneTypeExtensions
    {
        public static TDoFBoneType Update(this TDoFBoneType _this, int[] version)
        {
            if (!(version[0] > 5 || (version[0] == 5 && version[1] >= 6)))
            {
                if (_this >= TDoFBoneType.UpperChest)
                {
                    _this++;
                }
            }
            if (!(version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)))
            {
                if (_this >= TDoFBoneType.Head)
                {
                    _this++;
                }
            }
            if (!(version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)))
            {
                if (_this >= TDoFBoneType.LeftLowerLeg)
                {
                    _this += 3;
                }
            }
            if (!(version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)))
            {
                if (_this >= TDoFBoneType.RightLowerLeg)
                {
                    _this += 3;
                }
            }
            if (!(version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)))
            {
                if (_this >= TDoFBoneType.LeftUpperArm)
                {
                    _this += 3;
                }
            }
            if (!(version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)))
            {
                if (_this >= TDoFBoneType.RightUpperArm)
                {
                    _this += 3;
                }
            }
            return _this;
        }

        public static BoneType ToBoneType(this TDoFBoneType _this)
        {
            switch (_this)
            {
                case TDoFBoneType.Spine:
                    return BoneType.Spine;
                case TDoFBoneType.Chest:
                    return BoneType.Chest;
                case TDoFBoneType.UpperChest:
                    return BoneType.UpperChest;
                case TDoFBoneType.Neck:
                    return BoneType.Neck;
                case TDoFBoneType.Head:
                    return BoneType.Head;
                case TDoFBoneType.LeftUpperLeg:
                    return BoneType.LeftUpperLeg;
                case TDoFBoneType.LeftLowerLeg:
                    return BoneType.LeftLowerLeg;
                case TDoFBoneType.LeftFoot:
                    return BoneType.LeftFoot;
                case TDoFBoneType.LeftToes:
                    return BoneType.LeftToes;
                case TDoFBoneType.RightUpperLeg:
                    return BoneType.RightUpperLeg;
                case TDoFBoneType.RightLowerLeg:
                    return BoneType.RightLowerLeg;
                case TDoFBoneType.RightFoot:
                    return BoneType.RightFoot;
                case TDoFBoneType.RightToes:
                    return BoneType.RightToes;
                case TDoFBoneType.LeftShoulder:
                    return BoneType.LeftShoulder;
                case TDoFBoneType.LeftUpperArm:
                    return BoneType.LeftUpperArm;
                case TDoFBoneType.LeftLowerArm:
                    return BoneType.LeftLowerArm;
                case TDoFBoneType.LeftHand:
                    return BoneType.LeftHand;
                case TDoFBoneType.RightShoulder:
                    return BoneType.RightShoulder;
                case TDoFBoneType.RightUpperArm:
                    return BoneType.RightUpperArm;
                case TDoFBoneType.RightLowerArm:
                    return BoneType.RightLowerArm;
                case TDoFBoneType.RightHand:
                    return BoneType.RightHand;

                default:
                    throw new ArgumentException(_this.ToString());
            }
        }
    }
}
