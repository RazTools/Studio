using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    public class AnimationClipConverter
    {
        private readonly AnimationClip animationClip;

        public static readonly Regex UnknownPathRegex = new Regex($@"^path_[0-9]{{1,10}}$", RegexOptions.Compiled);

        private readonly Dictionary<Vector3Curve, List<Keyframe<Vector3>>> m_translations = new Dictionary<Vector3Curve, List<Keyframe<Vector3>>>();
        private readonly Dictionary<QuaternionCurve, List<Keyframe<Quaternion>>> m_rotations = new Dictionary<QuaternionCurve, List<Keyframe<Quaternion>>>();
        private readonly Dictionary<Vector3Curve, List<Keyframe<Vector3>>> m_scales = new Dictionary<Vector3Curve, List<Keyframe<Vector3>>>();
        private readonly Dictionary<Vector3Curve, List<Keyframe<Vector3>>> m_eulers = new Dictionary<Vector3Curve, List<Keyframe<Vector3>>>();
        private readonly Dictionary<FloatCurve, List<Keyframe<Float>>> m_floats = new Dictionary<FloatCurve, List<Keyframe<Float>>>();
        private readonly Dictionary<PPtrCurve, List<PPtrKeyframe>> m_pptrs = new Dictionary<PPtrCurve, List<PPtrKeyframe>>();

        public Vector3Curve[] Translations { get; private set; }
        public QuaternionCurve[] Rotations { get; private set; }
        public Vector3Curve[] Scales { get; private set; }
        public Vector3Curve[] Eulers { get; private set; }
        public FloatCurve[] Floats { get; private set; }
        public PPtrCurve[] PPtrs { get; private set; }

        public Game Game;

        public AnimationClipConverter(AnimationClip clip, Game game)
        {
            animationClip = clip;
            Game = game;
        }

        public static AnimationClipConverter Process(AnimationClip clip, Game game)
        {
            var converter = new AnimationClipConverter(clip, game);
            converter.ProcessInner();
            return converter;
        }
        private void ProcessInner()
        {
            var m_Clip = animationClip.m_MuscleClip.m_Clip;
            var bindings = animationClip.m_ClipBindingConstant;
            var tos = animationClip.FindTOS();
            
            var streamedFrames = m_Clip.m_StreamedClip.ReadData();
            var lastDenseFrame = m_Clip.m_DenseClip.m_FrameCount / m_Clip.m_DenseClip.m_SampleRate;
            var lastSampleFrame = streamedFrames.Count > 1 ? streamedFrames[streamedFrames.Count - 2].time : 0.0f;
            var lastFrame = Math.Max(lastDenseFrame, lastSampleFrame);

            if (m_Clip.m_ACLClip.IsSet && Game.Name != "SR_CB2" && Game.Name != "SR_CB3")
            {
                var lastACLFrame = ProcessACLClip(m_Clip, bindings, tos);
                lastFrame = Math.Max(lastFrame, lastACLFrame);
            }
            ProcessStreams(streamedFrames, bindings, tos, m_Clip.m_DenseClip.m_SampleRate);
            ProcessDenses(m_Clip, bindings, tos);
            if (m_Clip.m_ACLClip.IsSet && (Game.Name == "SR_CB2" || Game.Name == "SR_CB3"))
            {
                var lastACLFrame = ProcessACLClip(m_Clip, bindings, tos);
                lastFrame = Math.Max(lastFrame, lastACLFrame);
            }
            if (m_Clip.m_ConstantClip != null)
            {
                ProcessConstant(m_Clip, bindings, tos, lastFrame);
            }
            CreateCurves();
        }

        private void CreateCurves()
        {
            Translations = m_translations.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
            Rotations = m_rotations.Select(t => new QuaternionCurve(t.Key, t.Value)).ToArray();
            Scales = m_scales.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
            Eulers = m_eulers.Select(t => new Vector3Curve(t.Key, t.Value)).ToArray();
            Floats = m_floats.Select(t => new FloatCurve(t.Key, t.Value)).ToArray();
            PPtrs = m_pptrs.Select(t => new PPtrCurve(t.Key, t.Value)).ToArray();
        }

        private void ProcessStreams(List<StreamedClip.StreamedFrame> streamFrames, AnimationClipBindingConstant bindings, Dictionary<uint, string> tos, float sampleRate)
        {
            var curveValues = new float[4];
            var inSlopeValues = new float[4];
            var outSlopeValues = new float[4];
            var interval = 1.0f / sampleRate;

            // first (index [0]) stream frame is for slope calculation for the first real frame (index [1])
            // last one (index [count - 1]) is +Infinity
            // it is made for slope processing, but we don't need them
            for (var frameIndex = 1; frameIndex < streamFrames.Count - 1; frameIndex++)
            {
                var frame = streamFrames[frameIndex];
                for (var curveIndex = 0; curveIndex < frame.keyList.Length;)
                {
                    var curve = frame.keyList[curveIndex];
                    var index = curve.index;
                    if (animationClip.m_MuscleClip.m_Clip.m_ACLClip.IsSet && Game.Name != "SR_CB2" && Game.Name != "SR_CB3")
                        index += (int)animationClip.m_MuscleClip.m_Clip.m_ACLClip.m_CurveCount;
                    var binding = bindings.FindBinding(index);

                    var path = GetCurvePath(tos, binding.path);
                    if (binding.typeID == ClassIDType.Transform)
                    {
                        GetPreviousFrame(streamFrames, curve.index, frameIndex, out var prevFrameIndex, out var prevCurveIndex);
                        var dimension = binding.GetDimension();
                        for (int key = 0; key < dimension; key++)
                        {
                            var keyCurve = frame.keyList[curveIndex];
                            var prevFrame = streamFrames[prevFrameIndex];
                            var prevKeyCurve = prevFrame.keyList[prevCurveIndex + key];
                            var deltaTime = frame.time - prevFrame.time;
                            curveValues[key] = keyCurve.value;
                            inSlopeValues[key] = prevKeyCurve.CalculateNextInSlope(deltaTime, keyCurve);
                            outSlopeValues[key] = keyCurve.outSlope;
                            curveIndex = GetNextCurve(frame, curveIndex);
                        }

                        AddTransformCurve(frame.time, binding.attribute, curveValues, inSlopeValues, outSlopeValues, 0, path);
                    }
                    else
                    {
                        if (binding.customType == 8)
                        {
                            AddAnimatorMuscleCurve(binding, frame.time, frame.keyList[curveIndex].value);
                        }
                        else if (binding.customType == 20)
                        {
                            AddBlendShapeCurve(binding, path, frame.time, frame.keyList[curveIndex].value);
                        }
                        curveIndex = GetNextCurve(frame, curveIndex);
                    }
                }
            }
        }

        private void ProcessDenses(Clip clip, AnimationClipBindingConstant bindings, Dictionary<uint, string> tos)
        {
            var dense = clip.m_DenseClip;
            var streamCount = clip.m_StreamedClip.curveCount;
            var slopeValues = new float[4]; // no slopes - 0 values
            for (var frameIndex = 0; frameIndex < dense.m_FrameCount; frameIndex++)
            {
                var time = frameIndex / dense.m_SampleRate;
                var frameOffset = frameIndex * (int)dense.m_CurveCount;
                for (var curveIndex = 0; curveIndex < dense.m_CurveCount;)
                {
                    var index = (int)streamCount + curveIndex;
                    if (clip.m_ACLClip.IsSet && Game.Name != "SR_CB2" && Game.Name != "SR_CB3")
                        index += (int)clip.m_ACLClip.m_CurveCount;
                    var binding = bindings.FindBinding(index);
                    var path = GetCurvePath(tos, binding.path);
                    var framePosition = frameOffset + curveIndex;
                    if (binding.typeID == ClassIDType.Transform)
                    {
                        AddTransformCurve(time, binding.attribute, dense.m_SampleArray, slopeValues, slopeValues, framePosition, path);
                        curveIndex += binding.GetDimension();
                    } 
                    else
                    {
                        if (binding.customType == 8)
                        {
                            AddAnimatorMuscleCurve(binding, time, dense.m_SampleArray[framePosition]);
                        }
                        else if (binding.customType == 20)
                        {
                            AddBlendShapeCurve(binding, path, time, dense.m_SampleArray[framePosition]);
                        }
                        curveIndex++;
                    }
                }
            }
        }
        private float ProcessACLClip(Clip clip, AnimationClipBindingConstant bindings, Dictionary<uint, string> tos)
        {
            float[] values;
            float[] times;
            var acl = clip.m_ACLClip;
            if (Game.Name != "SR_CB2" && Game.Name != "SR_CB3")
            {
                acl.Process(out values, out times);
            }
            else
            {
                acl.ProcessSR(out values, out times);
            }
            float[] slopeValues = new float[4]; // no slopes - 0 values
        
            int frameCount = times.Length;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float time = times[frameIndex];
                int frameOffset = frameIndex * (int)acl.m_CurveCount;
                for (int curveIndex = 0; curveIndex < acl.m_CurveCount;)
                {
                    var index = curveIndex;
                    if (Game.Name == "SR_CB2" || Game.Name == "SR_CB3")
                        index += (int)(clip.m_StreamedClip.curveCount + clip.m_DenseClip.m_CurveCount);
                    GenericBinding binding = bindings.FindBinding(index);
                    string path = GetCurvePath(tos, binding.path);
                    int framePosition = frameOffset + curveIndex;
                    if (binding.typeID == ClassIDType.Transform)
                    {
                        AddTransformCurve(time, binding.attribute, values, slopeValues, slopeValues, framePosition, path);
                        curveIndex += binding.GetDimension();
                    }
                    else
                    {
                        if (binding.customType == 8)
                        {
                            AddAnimatorMuscleCurve(binding, time, values[framePosition]);
                        }
                        else if (binding.customType == 20)
                        {
                            AddBlendShapeCurve(binding, path, time, values[framePosition]);
                        }
                        curveIndex++;
                    }
                }
            }
        
            return times[frameCount - 1];
        }
        private void ProcessConstant(Clip clip, AnimationClipBindingConstant bindings, Dictionary<uint, string> tos, float lastFrame)
        {
            var constant = clip.m_ConstantClip;
            var streamCount = clip.m_StreamedClip.curveCount;
            var denseCount = clip.m_DenseClip.m_CurveCount;
            var slopeValues = new float[4]; // no slopes - 0 values

            // only first and last frames
            var time = 0.0f;
            for (var i = 0; i < 2; i++, time += lastFrame)
            {
                for (var curveIndex = 0; curveIndex < constant.data.Length;)
                {
                    var index = (int)(streamCount + denseCount + curveIndex);
                    if (clip.m_ACLClip.IsSet)
                        index += (int)clip.m_ACLClip.m_CurveCount;
                    GenericBinding binding = bindings.FindBinding(index);
                    string path = GetCurvePath(tos, binding.path);
                    if (binding.typeID == ClassIDType.Transform)
                    {
                        AddTransformCurve(time, binding.attribute, constant.data, slopeValues, slopeValues, curveIndex, path);
                        curveIndex += binding.GetDimension();
                    }
                    else
                    {
                        if (binding.customType == 8)
                        {
                            AddAnimatorMuscleCurve(binding, time, constant.data[curveIndex]);
                        }
                        else if (binding.customType == 20)
                        {
                            AddBlendShapeCurve(binding, path, time, constant.data[curveIndex]);
                        }
                        curveIndex++;
                    }
                }
            }
        }

        private void AddTransformCurve(float time, uint transType, float[] curveValues,
            float[] inSlopeValues, float[] outSlopeValues, int offset, string path)
        {
            switch (transType)
            {
                case 1:
                    {
                        Vector3Curve curve = new Vector3Curve(path);
                        if (!m_translations.TryGetValue(curve, out List<Keyframe<Vector3>> transCurve))
                        {
                            transCurve = new List<Keyframe<Vector3>>();
                            m_translations.Add(curve, transCurve);
                        }

                        float x = curveValues[offset + 0];
                        float y = curveValues[offset + 1];
                        float z = curveValues[offset + 2];

                        float inX = inSlopeValues[0];
                        float inY = inSlopeValues[1];
                        float inZ = inSlopeValues[2];

                        float outX = outSlopeValues[0];
                        float outY = outSlopeValues[1];
                        float outZ = outSlopeValues[2];

                        Vector3 value = new Vector3(x, y, z);
                        Vector3 inSlope = new Vector3(inX, inY, inZ);
                        Vector3 outSlope = new Vector3(outX, outY, outZ);
                        Keyframe<Vector3> transKey = new Keyframe<Vector3>(time, value, inSlope, outSlope, Keyframe<Vector3>.DefaultVector3Weight);
                        transCurve.Add(transKey);
                    }
                    break;

                case 2:
                    {
                        QuaternionCurve curve = new QuaternionCurve(path);
                        if (!m_rotations.TryGetValue(curve, out List<Keyframe<Quaternion>> rotCurve))
                        {
                            rotCurve = new List<Keyframe<Quaternion>>();
                            m_rotations.Add(curve, rotCurve);
                        }

                        float x = curveValues[offset + 0];
                        float y = curveValues[offset + 1];
                        float z = curveValues[offset + 2];
                        float w = curveValues[offset + 3];

                        float inX = inSlopeValues[0];
                        float inY = inSlopeValues[1];
                        float inZ = inSlopeValues[2];
                        float inW = inSlopeValues[3];

                        float outX = outSlopeValues[0];
                        float outY = outSlopeValues[1];
                        float outZ = outSlopeValues[2];
                        float outW = outSlopeValues[3];

                        Quaternion value = new Quaternion(x, y, z, w);
                        Quaternion inSlope = new Quaternion(inX, inY, inZ, inW);
                        Quaternion outSlope = new Quaternion(outX, outY, outZ, outW);
                        Keyframe<Quaternion> rotKey = new Keyframe<Quaternion>(time, value, inSlope, outSlope, Keyframe<Quaternion>.DefaultQuaternionWeight);
                        rotCurve.Add(rotKey);
                    }
                    break;

                case 3:
                    {
                        Vector3Curve curve = new Vector3Curve(path);
                        if (!m_scales.TryGetValue(curve, out List<Keyframe<Vector3>> scaleCurve))
                        {
                            scaleCurve = new List<Keyframe<Vector3>>();
                            m_scales.Add(curve, scaleCurve);
                        }

                        float x = curveValues[offset + 0];
                        float y = curveValues[offset + 1];
                        float z = curveValues[offset + 2];

                        float inX = inSlopeValues[0];
                        float inY = inSlopeValues[1];
                        float inZ = inSlopeValues[2];

                        float outX = outSlopeValues[0];
                        float outY = outSlopeValues[1];
                        float outZ = outSlopeValues[2];

                        Vector3 value = new Vector3(x, y, z);
                        Vector3 inSlope = new Vector3(inX, inY, inZ);
                        Vector3 outSlope = new Vector3(outX, outY, outZ);
                        Keyframe<Vector3> scaleKey = new Keyframe<Vector3>(time, value, inSlope, outSlope, Keyframe<Vector3>.DefaultVector3Weight);
                        scaleCurve.Add(scaleKey);
                    }
                    break;

                case 4:
                    {
                        Vector3Curve curve = new Vector3Curve(path);
                        if (!m_eulers.TryGetValue(curve, out List<Keyframe<Vector3>> eulerCurve))
                        {
                            eulerCurve = new List<Keyframe<Vector3>>();
                            m_eulers.Add(curve, eulerCurve);
                        }

                        float x = curveValues[offset + 0];
                        float y = curveValues[offset + 1];
                        float z = curveValues[offset + 2];

                        float inX = inSlopeValues[0];
                        float inY = inSlopeValues[1];
                        float inZ = inSlopeValues[2];

                        float outX = outSlopeValues[0];
                        float outY = outSlopeValues[1];
                        float outZ = outSlopeValues[2];

                        Vector3 value = new Vector3(x, y, z);
                        Vector3 inSlope = new Vector3(inX, inY, inZ);
                        Vector3 outSlope = new Vector3(outX, outY, outZ);
                        Keyframe<Vector3> eulerKey = new Keyframe<Vector3>(time, value, inSlope, outSlope, Keyframe<Vector3>.DefaultVector3Weight);
                        eulerCurve.Add(eulerKey);
                    }
                    break;

                default:
                    throw new NotImplementedException(transType.ToString());
            }
        }

        private void AddAnimatorMuscleCurve(GenericBinding binding, float time, float value)
        {
            FloatCurve curve = new FloatCurve(string.Empty, binding.GetClipMuscle(), ClassIDType.Animator, new PPtr<MonoScript>(0, 0, null));
            AddFloatKeyframe(curve, time, value);
        }

        private void AddBlendShapeCurve(GenericBinding binding, string path, float time, float value)
        {
            var attribute = "";
            const string Prefix = "blendShape.";
            if (UnknownPathRegex.IsMatch(path))
            {
                attribute = Prefix + binding.attribute;
            }

            foreach (GameObject root in animationClip.FindRoots().ToArray())
            {
                Transform rootTransform = root.GetTransform();
                Transform child = rootTransform.FindChild(path);
                if (child == null)
                {
                    continue;
                }
                SkinnedMeshRenderer skin = null;
                if (child.m_GameObject.TryGet(out var gameObject))
                {
                    skin = gameObject.FindComponent<SkinnedMeshRenderer>();
                }
                if (skin == null)
                {
                    continue;
                }
                if (!skin.m_Mesh.TryGet(out var mesh))
                {
                    continue;
                }
                string shapeName = mesh.FindBlendShapeNameByCRC(binding.attribute);
                if (shapeName == null)
                {
                    continue;
                }

                attribute = Prefix + shapeName;
            }
            attribute = Prefix + attribute;

            FloatCurve curve = new FloatCurve(path, attribute, binding.typeID, binding.script.CastTo<MonoScript>());
            AddFloatKeyframe(curve, time, value);
        }

        private void AddFloatKeyframe(FloatCurve curve, float time, float value)
        {
            if (!m_floats.TryGetValue(curve, out List<Keyframe<Float>> floatCurve))
            {
                floatCurve = new List<Keyframe<Float>>();
                m_floats.Add(curve, floatCurve);
            }

            Keyframe<Float> floatKey = new Keyframe<Float>(time, value, Keyframe<Float>.DefaultFloatWeight);
            floatCurve.Add(floatKey);
        }

        private void GetPreviousFrame(List<StreamedClip.StreamedFrame> streamFrames, int curveID, int currentFrame, out int frameIndex, out int curveIndex)
        {
            for (frameIndex = currentFrame - 1; frameIndex >= 0; frameIndex--)
            {
                var frame = streamFrames[frameIndex];
                for (curveIndex = 0; curveIndex < frame.keyList.Length; curveIndex++)
                {
                    var curve = frame.keyList[curveIndex];
                    if (curve.index == curveID)
                    {
                        return;
                    }
                }
            }
            throw new Exception($"There is no curve with index {curveID} in any of previous frames");
        }

        private int GetNextCurve(StreamedClip.StreamedFrame frame, int currentCurve)
        {
            var curve = frame.keyList[currentCurve];
            int i = currentCurve + 1;
            for (; i < frame.keyList.Length; i++)
            {
                if (frame.keyList[i].index != curve.index)
                {
                    return i;
                }
            }
            return i;
        }

        private static string GetCurvePath(Dictionary<uint, string> tos, uint hash)
        {
            if (tos.TryGetValue(hash, out string path))
            {
                return path;
            }
            else
            {
                return $"path_{hash}";
            }
        }
        
    }
}