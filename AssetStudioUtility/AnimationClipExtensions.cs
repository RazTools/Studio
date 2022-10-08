using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class AnimationClipExtensions
    {
        public static string Convert(this AnimationClip animationClip, Game game)
        {
            var converter = AnimationClipConverter.Process(animationClip, game);
            animationClip.m_RotationCurves = converter.Rotations.Union(animationClip.m_RotationCurves).ToArray();
            animationClip.m_EulerCurves = converter.Eulers.Union(animationClip.m_EulerCurves).ToArray();
            animationClip.m_PositionCurves = converter.Translations.Union(animationClip.m_PositionCurves).ToArray();
            animationClip.m_ScaleCurves = converter.Scales.Union(animationClip.m_ScaleCurves).ToArray();
            animationClip.m_FloatCurves = converter.Floats.Union(animationClip.m_FloatCurves).ToArray();
            animationClip.m_PPtrCurves = converter.PPtrs.Union(animationClip.m_PPtrCurves).ToArray();
            return ConvertSerializedAnimationClip(animationClip);
        }

        public static string ConvertSerializedAnimationClip(AnimationClip animationClip)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriter(sb))
            {
                YAMLWriter writer = new YAMLWriter();
                YAMLDocument doc = ExportYAMLDocument(animationClip);
                writer.AddDocument(doc);
                writer.Write(stringWriter);
                return sb.ToString();
            }
        }

        public static YAMLDocument ExportYAMLDocument(AnimationClip animationClip)
        {
            YAMLDocument document = new YAMLDocument();
            YAMLMappingNode root = document.CreateMappingRoot();
            root.Tag = ((int)ClassIDType.AnimationClip).ToString();
            root.Anchor = ((int)ClassIDType.AnimationClip * 100000).ToString();
            YAMLMappingNode node = (YAMLMappingNode)animationClip.ExportYAML();
            root.Add(ClassIDType.AnimationClip.ToString(), node);
            return document;
        }
        public static string GetClipMuscle(this GenericBinding genericBinding) => ClipMuscles[genericBinding.attribute] ?? $"unknown_{genericBinding.attribute}";
        
        public static string[] ClipMuscles =
        {
            "MotionT.x",
            "MotionT.y",
            "MotionT.z",
            "MotionQ.x",
            "MotionQ.y",
            "MotionQ.z",
            "MotionQ.w",
            "RootT.x",
            "RootT.y",
            "RootT.z",
            "RootQ.x",
            "RootQ.y",
            "RootQ.z",
            "RootQ.w",
            "LeftFootT.x",
            "LeftFootT.y",
            "LeftFootT.z",
            "LeftFootQ.x",
            "LeftFootQ.y",
            "LeftFootQ.z",
            "LeftFootQ.w",
            "RightFootT.x",
            "RightFootT.y",
            "RightFootT.z",
            "RightFootQ.x",
            "RightFootQ.y",
            "RightFootQ.z",
            "RightFootQ.w",
            "LeftHandT.x",
            "LeftHandT.y",
            "LeftHandT.z",
            "LeftHandQ.x",
            "LeftHandQ.y",
            "LeftHandQ.z",
            "LeftHandQ.w",
            "RightHandT.x",
            "RightHandT.y",
            "RightHandT.z",
            "RightHandQ.x",
            "RightHandQ.y",
            "RightHandQ.z",
            "RightHandQ.w",
            "Spine Front-Back",
            "Spine Left-Right",
            "Spine Twist Left-Right",
            "Chest Front-Back",
            "Chest Left-Right",
            "Chest Twist Left-Right",
            "UpperChest Front-Back",
            "UpperChest Left-Right",
            "UpperChest Twist Left-Right",
            "Neck Nod Down-Up",
            "Neck Tilt Left-Right",
            "Neck Turn Left-Right",
            "Head Nod Down-Up",
            "Head Tilt Left-Right",
            "Head Turn Left-Right",
            "Left Eye Down-Up",
            "Left Eye In-Out",
            "Right Eye Down-Up",
            "Right Eye In-Out",
            "Jaw Close",
            "Jaw Left-Right",
            "Left Upper Leg Front-Back",
            "Left Upper Leg In-Out",
            "Left Upper Leg Twist In-Out",
            "Left Lower Leg Stretch",
            "Left Lower Leg Twist In-Out",
            "Left Foot Up-Down",
            "Left Foot Twist In-Out",
            "Left Toes Up-Down",
            "Right Upper Leg Front-Back",
            "Right Upper Leg In-Out",
            "Right Upper Leg Twist In-Out",
            "Right Lower Leg Stretch",
            "Right Lower Leg Twist In-Out",
            "Right Foot Up-Down",
            "Right Foot Twist In-Out",
            "Right Toes Up-Down",
            "Left Shoulder Down-Up",
            "Left Shoulder Front-Back",
            "Left Arm Down-Up",
            "Left Arm Front-Back",
            "Left Arm Twist In-Out",
            "Left Forearm Stretch",
            "Left Forearm Twist In-Out",
            "Left Hand Down-Up",
            "Left Hand In-Out",
            "Right Shoulder Down-Up",
            "Right Shoulder Front-Back",
            "Right Arm Down-Up",
            "Right Arm Front-Back",
            "Right Arm Twist In-Out",
            "Right Forearm Stretch",
            "Right Forearm Twist In-Out",
            "Right Hand Down-Up",
            "Right Hand In-Out",
            "LeftHand.Thumb.1 Stretched",
            "LeftHand.Thumb.Spread",
            "LeftHand.Thumb.2 Stretched",
            "LeftHand.Thumb.3 Stretched",
            "LeftHand.Index.1 Stretched",
            "LeftHand.Index.Spread",
            "LeftHand.Index.2 Stretched",
            "LeftHand.Index.3 Stretched",
            "LeftHand.Middle.1 Stretched",
            "LeftHand.Middle.Spread",
            "LeftHand.Middle.2 Stretched",
            "LeftHand.Middle.3 Stretched",
            "LeftHand.Ring.1 Stretched",
            "LeftHand.Ring.Spread",
            "LeftHand.Ring.2 Stretched",
            "LeftHand.Ring.3 Stretched",
            "LeftHand.Little.1 Stretched",
            "LeftHand.Little.Spread",
            "LeftHand.Little.2 Stretched",
            "LeftHand.Little.3 Stretched",
            "RightHand.Thumb.1 Stretched",
            "RightHand.Thumb.Spread",
            "RightHand.Thumb.2 Stretched",
            "RightHand.Thumb.3 Stretched",
            "RightHand.Index.1 Stretched",
            "RightHand.Index.Spread",
            "RightHand.Index.2 Stretched",
            "RightHand.Index.3 Stretched",
            "RightHand.Middle.1 Stretched",
            "RightHand.Middle.Spread",
            "RightHand.Middle.2 Stretched",
            "RightHand.Middle.3 Stretched",
            "RightHand.Ring.1 Stretched",
            "RightHand.Ring.Spread",
            "RightHand.Ring.2 Stretched",
            "RightHand.Ring.3 Stretched",
            "RightHand.Little.1 Stretched",
            "RightHand.Little.Spread",
            "RightHand.Little.2 Stretched",
            "RightHand.Little.3 Stretched",
            "SpineTDOF.x",
            "SpineTDOF.y",
            "SpineTDOF.z",
            "ChestTDOF.x",
            "ChestTDOF.y",
            "ChestTDOF.z",
            "UpperChestTDOF.x",
            "UpperChestTDOF.y",
            "UpperChestTDOF.z",
            "NeckTDOF.x",
            "NeckTDOF.y",
            "NeckTDOF.z",
            "HeadTDOF.x",
            "HeadTDOF.y",
            "HeadTDOF.z",
            "LeftUpperLegTDOF.x",
            "LeftUpperLegTDOF.y",
            "LeftUpperLegTDOF.z",
            "LeftLowerLegTDOF.x",
            "LeftLowerLegTDOF.y",
            "LeftLowerLegTDOF.z",
            "LeftFootTDOF.x",
            "LeftFootTDOF.y",
            "LeftFootTDOF.z",
            "LeftToesTDOF.x",
            "LeftToesTDOF.y",
            "LeftToesTDOF.z",
            "RightUpperLegTDOF.x",
            "RightUpperLegTDOF.y",
            "RightUpperLegTDOF.z",
            "RightLowerLegTDOF.x",
            "RightLowerLegTDOF.y",
            "RightLowerLegTDOF.z",
            "RightFootTDOF.x",
            "RightFootTDOF.y",
            "RightFootTDOF.z",
            "RightToesTDOF.x",
            "RightToesTDOF.y",
            "RightToesTDOF.z",
            "LeftShoulderTDOF.x",
            "LeftShoulderTDOF.y",
            "LeftShoulderTDOF.z",
            "LeftUpperArmTDOF.x",
            "LeftUpperArmTDOF.y",
            "LeftUpperArmTDOF.z",
            "LeftLowerArmTDOF.x",
            "LeftLowerArmTDOF.y",
            "LeftLowerArmTDOF.z",
            "LeftHandTDOF.x",
            "LeftHandTDOF.y",
            "LeftHandTDOF.z",
            "RightShoulderTDOF.x",
            "RightShoulderTDOF.y",
            "RightShoulderTDOF.z",
            "RightUpperArmTDOF.x",
            "RightUpperArmTDOF.y",
            "RightUpperArmTDOF.z",
            "RightLowerArmTDOF.x",
            "RightLowerArmTDOF.y",
            "RightLowerArmTDOF.z",
            "RightHandTDOF.x",
            "RightHandTDOF.y",
            "RightHandTDOF.z",
        };
    }
}
