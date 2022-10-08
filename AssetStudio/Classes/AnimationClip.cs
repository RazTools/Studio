using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AssetStudio
{
    public class Keyframe<T> : IYAMLExportable
        where T : struct, IYAMLExportable
    {
        public float time;
        public T value;
        public T inSlope;
        public T outSlope;
        public int weightedMode;
        public T inWeight;
        public T outWeight;
        public int tangentMode;


        public Keyframe(ObjectReader reader, Func<T> readerFunc)
        {
            time = reader.ReadSingle();
            value = readerFunc();
            inSlope = readerFunc();
            outSlope = readerFunc();
            if (reader.version[0] >= 2018) //2018 and up
            {
                weightedMode = reader.ReadInt32();
                inWeight = readerFunc();
                outWeight = readerFunc();
            }
        }

        public Keyframe(float time, T value, T weight) : this(time, value, default, default, weight)
        {
            tangentMode = 0;
        }
        
        public Keyframe(float time, T value, T inSlope, T outSlope, T weight)
        {
            this.time = time;
            this.value = value;
            this.inSlope = inSlope;
            this.outSlope = outSlope;
            weightedMode = 0;
            inWeight = weight;
            outWeight = weight;
            tangentMode = 1;
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.AddSerializedVersion(2);
            node.Add(nameof(time), time);
            node.Add(nameof(value), value.ExportYAML());
            node.Add(nameof(inSlope), inSlope.ExportYAML());
            node.Add(nameof(outSlope), outSlope.ExportYAML());
            node.Add(nameof(tangentMode), tangentMode);
            node.Add(nameof(weightedMode), weightedMode);
            node.Add(nameof(inWeight), inWeight.ExportYAML());
            node.Add(nameof(outWeight), outWeight.ExportYAML());
            return node;    
        }

        public static Float DefaultFloatWeight => 1.0f / 3.0f;
        public static Vector3 DefaultVector3Weight => new Vector3(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);
        public static Quaternion DefaultQuaternionWeight => new Quaternion(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);
    }

    public class AnimationCurve<T> : IYAMLExportable
        where T : struct, IYAMLExportable
    {
        public Keyframe<T>[] m_Curve;
        public int m_PreInfinity;
        public int m_PostInfinity;
        public int m_RotationOrder;

        public AnimationCurve(ObjectReader reader, Func<T> readerFunc)
        {
            var version = reader.version;
            int numCurves = reader.ReadInt32();
            m_Curve = new Keyframe<T>[numCurves];
            for (int i = 0; i < numCurves; i++)
            {
                m_Curve[i] = new Keyframe<T>(reader, readerFunc);
            }

            m_PreInfinity = reader.ReadInt32();
            m_PostInfinity = reader.ReadInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3))//5.3 and up
            {
                m_RotationOrder = reader.ReadInt32();
            }
        }
        
        public AnimationCurve()
        {
            m_PreInfinity = 2;
            m_PostInfinity = 2;
            m_RotationOrder = 4;
            m_Curve = Array.Empty<Keyframe<T>>();
        }
        
        public AnimationCurve(List<Keyframe<T>> keyframes)
        {
            m_PreInfinity = 2;
            m_PostInfinity = 2;
            m_RotationOrder = 4;
            m_Curve = new Keyframe<T>[keyframes.Count];
            for (int i = 0; i < keyframes.Count; i++)
            {
                m_Curve[i] = keyframes[i];
            }
        }
        
        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.AddSerializedVersion(2);
            node.Add(nameof(m_Curve), m_Curve.ExportYAML());
            node.Add(nameof(m_PreInfinity), m_PreInfinity);
            node.Add(nameof(m_PostInfinity), m_PostInfinity);
            node.Add(nameof(m_RotationOrder), m_RotationOrder);
            return node;
        }
    }

    public class QuaternionCurve : IYAMLExportable
    {
        public AnimationCurve<Quaternion> curve;
        public string path;

        public QuaternionCurve(ObjectReader reader)
        {
            curve = new AnimationCurve<Quaternion>(reader, reader.ReadQuaternion);
            path = reader.ReadAlignedString();
        }
        
        public QuaternionCurve(string path)
        {
            curve = new AnimationCurve<Quaternion>();
            this.path = path;
        }
        
        public QuaternionCurve(QuaternionCurve copy, List<Keyframe<Quaternion>> keyframes) : this(copy.path, keyframes) { }
        
        public QuaternionCurve(string path, List<Keyframe<Quaternion>> keyframes)
        {
            curve = new AnimationCurve<Quaternion>(keyframes);
            this.path = path;
        }
        
        public YAMLNode ExportYAML()
        {
            YAMLMappingNode node = new YAMLMappingNode();
            node.Add(nameof(curve), curve.ExportYAML());
            node.Add(nameof(path), path);
            return node;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is QuaternionCurve quaternionCurve)
            {
                return path == quaternionCurve.path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 199;
            unchecked
            {
                hash = 617 + hash * path.GetHashCode();
            }
            return hash;
        }
    }

    public class ACLClip
    {
        public byte[] m_ClipData;
        public uint[] m_ClipDataUint;

        public uint m_CurveCount;
        public uint m_ConstCurveCount;
        public ACLClip(ObjectReader reader)
        {
            if (reader.Game.Name == "SR")
            {
                m_ClipDataUint = reader.ReadUInt32Array();
            }
            else
            {
                m_ClipData = reader.ReadUInt8Array();
                reader.AlignStream();
            }

            m_CurveCount = reader.ReadUInt32();

            if (reader.Game.Name == "SR")
            {
                m_ConstCurveCount = reader.ReadUInt32();
            }
        }
        public bool IsSet => m_ClipDataUint != null && m_ClipDataUint.Length > 0 || m_ClipData != null && m_ClipData.Length > 0;
    }

    public class PackedFloatVector : IYAMLExportable
    {
        public uint m_NumItems;
        public float m_Range;
        public float m_Start;
        public byte[] m_Data;
        public byte m_BitSize;

        public PackedFloatVector(ObjectReader reader)
        {
            m_NumItems = reader.ReadUInt32();
            m_Range = reader.ReadSingle();
            m_Start = reader.ReadSingle();

            int numData = reader.ReadInt32();
            m_Data = reader.ReadBytes(numData);
            reader.AlignStream();

            m_BitSize = reader.ReadByte();
            reader.AlignStream();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(m_NumItems), m_NumItems);
            node.Add(nameof(m_Range), m_Range);
            node.Add(nameof(m_Start), m_Start);
            node.Add(nameof(m_Data), m_Data.ExportYAML());
            node.Add(nameof(m_BitSize), m_BitSize);
            return node;
        }

        public float[] UnpackFloats(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
        {
            int bitPos = m_BitSize * start;
            int indexPos = bitPos / 8;
            bitPos %= 8;

            float scale = 1.0f / m_Range;
            if (numChunks == -1)
                numChunks = (int)m_NumItems / itemCountInChunk;
            var end = chunkStride * numChunks / 4;
            var data = new List<float>();
            for (var index = 0; index != end; index += chunkStride / 4)
            {
                for (int i = 0; i < itemCountInChunk; ++i)
                {
                    uint x = 0;

                    int bits = 0;
                    while (bits < m_BitSize)
                    {
                        x |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                        int num = Math.Min(m_BitSize - bits, 8 - bitPos);
                        bitPos += num;
                        bits += num;
                        if (bitPos == 8)
                        {
                            indexPos++;
                            bitPos = 0;
                        }
                    }
                    x &= (uint)(1 << m_BitSize) - 1u;
                    data.Add(x / (scale * ((1 << m_BitSize) - 1)) + m_Start);
                }
            }

            return data.ToArray();
        }
    }

    public class PackedIntVector : IYAMLExportable
    {
        public uint m_NumItems;
        public byte[] m_Data;
        public byte m_BitSize;

        public PackedIntVector(ObjectReader reader)
        {
            m_NumItems = reader.ReadUInt32();

            int numData = reader.ReadInt32();
            m_Data = reader.ReadBytes(numData);
            reader.AlignStream();

            m_BitSize = reader.ReadByte();
            reader.AlignStream();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(m_NumItems), m_NumItems);
            node.Add(nameof(m_Data), m_Data.ExportYAML());
            node.Add(nameof(m_BitSize), m_BitSize);
            return node;
        }

        public int[] UnpackInts()
        {
            var data = new int[m_NumItems];
            int indexPos = 0;
            int bitPos = 0;
            for (int i = 0; i < m_NumItems; i++)
            {
                int bits = 0;
                data[i] = 0;
                while (bits < m_BitSize)
                {
                    data[i] |= (m_Data[indexPos] >> bitPos) << bits;
                    int num = Math.Min(m_BitSize - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                data[i] &= (1 << m_BitSize) - 1;
            }
            return data;
        }
    }

    public class PackedQuatVector : IYAMLExportable
    {
        public uint m_NumItems;
        public byte[] m_Data;

        public PackedQuatVector(ObjectReader reader)
        {
            m_NumItems = reader.ReadUInt32();

            int numData = reader.ReadInt32();
            m_Data = reader.ReadBytes(numData);

            reader.AlignStream();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(m_NumItems), m_NumItems);
            node.Add(nameof(m_Data), m_Data.ExportYAML());
            return node;
        }

        public Quaternion[] UnpackQuats()
        {
            var data = new Quaternion[m_NumItems];
            int indexPos = 0;
            int bitPos = 0;

            for (int i = 0; i < m_NumItems; i++)
            {
                uint flags = 0;

                int bits = 0;
                while (bits < 3)
                {
                    flags |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                    int num = Math.Min(3 - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                flags &= 7;


                var q = new Quaternion();
                float sum = 0;
                for (int j = 0; j < 4; j++)
                {
                    if ((flags & 3) != j)
                    {
                        int bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
                        uint x = 0;

                        bits = 0;
                        while (bits < bitSize)
                        {
                            x |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                            int num = Math.Min(bitSize - bits, 8 - bitPos);
                            bitPos += num;
                            bits += num;
                            if (bitPos == 8)
                            {
                                indexPos++;
                                bitPos = 0;
                            }
                        }
                        x &= (uint)((1 << bitSize) - 1);
                        q[j] = x / (0.5f * ((1 << bitSize) - 1)) - 1;
                        sum += q[j] * q[j];
                    }
                }

                int lastComponent = (int)(flags & 3);
                q[lastComponent] = (float)Math.Sqrt(1 - sum);
                if ((flags & 4) != 0u)
                    q[lastComponent] = -q[lastComponent];
                data[i] = q;
            }

            return data;
        }
    }

    public class CompressedAnimationCurve : IYAMLExportable
    {
        public string m_Path;
        public PackedIntVector m_Times;
        public PackedQuatVector m_Values;
        public PackedFloatVector m_Slopes;
        public int m_PreInfinity;
        public int m_PostInfinity;

        public CompressedAnimationCurve(ObjectReader reader)
        {
            m_Path = reader.ReadAlignedString();
            m_Times = new PackedIntVector(reader);
            m_Values = new PackedQuatVector(reader);
            m_Slopes = new PackedFloatVector(reader);
            m_PreInfinity = reader.ReadInt32();
            m_PostInfinity = reader.ReadInt32();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(m_Path), m_Path);
            node.Add(nameof(m_Times), m_Times.ExportYAML());
            node.Add(nameof(m_Values), m_Values.ExportYAML());
            node.Add(nameof(m_Slopes), m_Slopes.ExportYAML());
            node.Add(nameof(m_PreInfinity), m_PreInfinity);
            node.Add(nameof(m_PostInfinity), m_PostInfinity);
            return node;
        }
    }

    public class Vector3Curve : IYAMLExportable
    {
        public AnimationCurve<Vector3> curve;
        public string path;

        public Vector3Curve(ObjectReader reader)
        {
            curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3);
            path = reader.ReadAlignedString();
        }

        public Vector3Curve(Vector3Curve copy, List<Keyframe<Vector3>> keyframes) 
            : this(copy.path, keyframes) { }
        public Vector3Curve(string path)
        {
            curve = new AnimationCurve<Vector3>();
            this.path = path;
        }
        public Vector3Curve(string path, List<Keyframe<Vector3>> keyframes)
        {
            curve = new AnimationCurve<Vector3>(keyframes);
            this.path = path;
        }

        public YAMLNode ExportYAML()
        {
            YAMLMappingNode node = new YAMLMappingNode();
            node.Add(nameof(curve), curve.ExportYAML());
            node.Add(nameof(path), path);
            return node;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3Curve vector3Curve)
            {
                return path == vector3Curve.path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 577;
            unchecked
            {
                hash = 419 + hash * path.GetHashCode();
            }
            return hash;
        }
    }

    public class FloatCurve : IYAMLExportable
    {
        public AnimationCurve<Float> curve;
        public string attribute;
        public string path;
        public ClassIDType classID;
        public PPtr<MonoScript> script;


        public FloatCurve(ObjectReader reader)
        {
            curve = new AnimationCurve<Float>(reader, reader.ReadFloat);
            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = (ClassIDType)reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
        }

        public FloatCurve(FloatCurve copy, List<Keyframe<Float>> keyframes) : this(copy.path, copy.attribute, copy.classID, copy.script, keyframes) { }
        public FloatCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script)
        {
            curve = new AnimationCurve<Float>();
            this.attribute = attribute;
            this.path = path;
            this.classID = classID;
            this.script = script;
        }
        public FloatCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script, List<Keyframe<Float>> keyframes)
            : this(path, attribute, classID, script)
        {
            curve = new AnimationCurve<Float>(keyframes);
            this.attribute = attribute;
            this.path = path;
            this.classID = classID;
            this.script = script;
        }

        public YAMLNode ExportYAML()
        {
            YAMLMappingNode node = new YAMLMappingNode();
            node.Add(nameof(curve), curve.ExportYAML());
            node.Add(nameof(attribute), attribute);
            node.Add(nameof(path), path);
            node.Add(nameof(classID), (int)classID);
            node.Add(nameof(script), script.ExportYAML());
            return node;
        }

        public override bool Equals(object obj)
        {
            if (obj is FloatCurve floatCurve)
            {
                return attribute == floatCurve.attribute && path == floatCurve.path && classID == floatCurve.classID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 23 + path.GetHashCode();
            }
            return hash;
        }
    }

    public class PPtrKeyframe : IYAMLExportable
    {
        public float time;
        public PPtr<Object> value;


        public PPtrKeyframe(ObjectReader reader)
        {
            time = reader.ReadSingle();
            value = new PPtr<Object>(reader);
        }
        public PPtrKeyframe(float time, PPtr<Object> script)
        {
            this.time = time;
            value = script;
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(time), time);
            node.Add(nameof(value), value.ExportYAML());
            return node;
        }
    }

    public class PPtrCurve : IYAMLExportable
    {
        public PPtrKeyframe[] curve;
        public string attribute;
        public string path;
        public ClassIDType classID;
        public PPtr<MonoScript> script;


        public PPtrCurve(ObjectReader reader)
        {
            int numCurves = reader.ReadInt32();
            curve = new PPtrKeyframe[numCurves];
            for (int i = 0; i < numCurves; i++)
            {
                curve[i] = new PPtrKeyframe(reader);
            }

            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = (ClassIDType)reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
        }

        public PPtrCurve(PPtrCurve copy, List<PPtrKeyframe> keyframes) : this(copy.path, copy.attribute, copy.classID, copy.script, keyframes) { }
        public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script)
        {
            this.attribute = attribute;
            this.path = path;
            this.classID = classID;
            this.script = script;
        }
        public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script, IReadOnlyList<PPtrKeyframe> keyframes) :
            this(path, attribute, classID, script)
        {
            curve = new PPtrKeyframe[keyframes.Count];
            for (int i = 0; i < keyframes.Count; i++)
            {
                curve[i] = keyframes[i];
            }
        }

        public YAMLNode ExportYAML()
        {
            YAMLMappingNode node = new YAMLMappingNode();
            node.Add(nameof(curve), curve.ExportYAML());
            node.Add(nameof(attribute), attribute);
            node.Add(nameof(path), path);
            node.Add(nameof(classID), ((int)classID).ToString());
            node.Add(nameof(script), script.ExportYAML());
            return node;
        }

        public override bool Equals(object obj)
        {
            if (obj is PPtrCurve pptrCurve)
            {
                return this == pptrCurve;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 113;
            unchecked
            {
                hash = hash + 457 * attribute.GetHashCode();
                hash = hash * 433 + path.GetHashCode();
                hash = hash * 223 + classID.GetHashCode();
                hash = hash * 911 + script.GetHashCode();
            }
            return hash;
        }
    }

    public class AABB : IYAMLExportable
    {
        public Vector3 m_Center;
        public Vector3 m_Extent;

        public AABB(ObjectReader reader)
        {
            m_Center = reader.ReadVector3();
            m_Extent = reader.ReadVector3();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(m_Center), m_Center.ExportYAML());
            node.Add(nameof(m_Extent), m_Extent.ExportYAML());
            return node;
        }
    }

    public class xform
    {
        public Vector3 t;
        public Quaternion q;
        public Vector3 s;

        public xform(ObjectReader reader)
        {
            var version = reader.version;
            t = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
            q = reader.ReadQuaternion();
            s = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
        }
    }

    public class HandPose
    {
        public xform m_GrabX;
        public float[] m_DoFArray;
        public float m_Override;
        public float m_CloseOpen;
        public float m_InOut;
        public float m_Grab;

        public HandPose(ObjectReader reader)
        {
            m_GrabX = new xform(reader);
            m_DoFArray = reader.ReadSingleArray();
            m_Override = reader.ReadSingle();
            m_CloseOpen = reader.ReadSingle();
            m_InOut = reader.ReadSingle();
            m_Grab = reader.ReadSingle();
        }
    }

    public class HumanGoal
    {
        public xform m_X;
        public float m_WeightT;
        public float m_WeightR;
        public Vector3 m_HintT;
        public float m_HintWeightT;

        public HumanGoal(ObjectReader reader)
        {
            var version = reader.version;
            m_X = new xform(reader);
            m_WeightT = reader.ReadSingle();
            m_WeightR = reader.ReadSingle();
            if (version[0] >= 5)//5.0 and up
            {
                m_HintT = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
                m_HintWeightT = reader.ReadSingle();
            }
        }
    }

    public class HumanPose
    {
        public xform m_RootX;
        public Vector3 m_LookAtPosition;
        public Vector4 m_LookAtWeight;
        public HumanGoal[] m_GoalArray;
        public HandPose m_LeftHandPose;
        public HandPose m_RightHandPose;
        public float[] m_DoFArray;
        public Vector3[] m_TDoFArray;

        public HumanPose(ObjectReader reader)
        {
            var version = reader.version;
            m_RootX = new xform(reader);
            m_LookAtPosition = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
            m_LookAtWeight = reader.ReadVector4();

            int numGoals = reader.ReadInt32();
            m_GoalArray = new HumanGoal[numGoals];
            for (int i = 0; i < numGoals; i++)
            {
                m_GoalArray[i] = new HumanGoal(reader);
            }

            m_LeftHandPose = new HandPose(reader);
            m_RightHandPose = new HandPose(reader);

            m_DoFArray = reader.ReadSingleArray();

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2))//5.2 and up
            {
                int numTDof = reader.ReadInt32();
                m_TDoFArray = new Vector3[numTDof];
                for (int i = 0; i < numTDof; i++)
                {
                    m_TDoFArray[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
                }
            }
        }
    }

    public class StreamedClip
    {
        public uint[] data;
        public uint curveCount;

        public StreamedClip(ObjectReader reader)
        {
            data = reader.ReadUInt32Array();
            curveCount = reader.ReadUInt32();
        }

        public class StreamedCurveKey
        {
            public int index;
            public float[] coeff;

            public float value;
            public float outSlope;
            public float inSlope;

            public StreamedCurveKey(BinaryReader reader)
            {
                index = reader.ReadInt32();
                coeff = reader.ReadSingleArray(4);

                outSlope = coeff[2];
                value = coeff[3];
            }

            public float CalculateNextInSlope(float dx, StreamedCurveKey rhs)
            {
                //Stepped
                if (coeff[0] == 0f && coeff[1] == 0f && coeff[2] == 0f)
                {
                    return float.PositiveInfinity;
                }

                dx = Math.Max(dx, 0.0001f);
                var dy = rhs.value - value;
                var length = 1.0f / (dx * dx);
                var d1 = outSlope * dx;
                var d2 = dy + dy + dy - d1 - d1 - coeff[1] / length;
                return d2 / dx;
            }
        }

        public class StreamedFrame
        {
            public float time;
            public StreamedCurveKey[] keyList;

            public StreamedFrame(BinaryReader reader)
            {
                time = reader.ReadSingle();

                int numKeys = reader.ReadInt32();
                keyList = new StreamedCurveKey[numKeys];
                for (int i = 0; i < numKeys; i++)
                {
                    keyList[i] = new StreamedCurveKey(reader);
                }
            }
        }

        public List<StreamedFrame> ReadData()
        {
            var frameList = new List<StreamedFrame>();
            var buffer = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);
            using (var reader = new BinaryReader(new MemoryStream(buffer)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    frameList.Add(new StreamedFrame(reader));
                }
            }

            for (int frameIndex = 2; frameIndex < frameList.Count - 1; frameIndex++)
            {
                var frame = frameList[frameIndex];
                foreach (var curveKey in frame.keyList)
                {
                    for (int i = frameIndex - 1; i >= 0; i--)
                    {
                        var preFrame = frameList[i];
                        var preCurveKey = preFrame.keyList.FirstOrDefault(x => x.index == curveKey.index);
                        if (preCurveKey != null)
                        {
                            curveKey.inSlope = preCurveKey.CalculateNextInSlope(frame.time - preFrame.time, curveKey);
                            break;
                        }
                    }
                }
            }
            return frameList;
        }
    }

    public class DenseClip
    {
        public int m_FrameCount;
        public uint m_CurveCount;
        public float m_SampleRate;
        public float m_BeginTime;
        public float[] m_SampleArray;

        public DenseClip(ObjectReader reader)
        {
            m_FrameCount = reader.ReadInt32();
            m_CurveCount = reader.ReadUInt32();
            m_SampleRate = reader.ReadSingle();
            m_BeginTime = reader.ReadSingle();
            m_SampleArray = reader.ReadSingleArray();
        }
    }

    public class ConstantClip
    {
        public float[] data;

        public ConstantClip(ObjectReader reader)
        {
            data = reader.ReadSingleArray();
        }
    }

    public class ValueConstant
    {
        public uint m_ID;
        public uint m_TypeID;
        public uint m_Type;
        public uint m_Index;

        public ValueConstant(ObjectReader reader)
        {
            var version = reader.version;
            m_ID = reader.ReadUInt32();
            if (version[0] < 5 || (version[0] == 5 && version[1] < 5))//5.5 down
            {
                m_TypeID = reader.ReadUInt32();
            }
            m_Type = reader.ReadUInt32();
            m_Index = reader.ReadUInt32();
        }
    }

    public class ValueArrayConstant
    {
        public ValueConstant[] m_ValueArray;

        public ValueArrayConstant(ObjectReader reader)
        {
            int numVals = reader.ReadInt32();
            m_ValueArray = new ValueConstant[numVals];
            for (int i = 0; i < numVals; i++)
            {
                m_ValueArray[i] = new ValueConstant(reader);
            }
        }
    }

    public class Clip
    {
        public StreamedClip m_StreamedClip;
        public DenseClip m_DenseClip;
        public ConstantClip m_ConstantClip;
        public ACLClip m_ACLClip;
        public ValueArrayConstant m_Binding;

        public Clip(ObjectReader reader)
        {
            var version = reader.version;
            m_StreamedClip = new StreamedClip(reader);
            m_DenseClip = new DenseClip(reader);
            if (reader.Game.Name == "SR")
            {
                m_ACLClip = new ACLClip(reader);
            }
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_ConstantClip = new ConstantClip(reader);
            }
            if (reader.Game.Name != "SR" && reader.Game.Name != "TOT")
            {
                m_ACLClip = new ACLClip(reader);
            }
            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 3)) //2018.3 down
            {
                m_Binding = new ValueArrayConstant(reader);
            }
        }

        public AnimationClipBindingConstant ConvertValueArrayToGenericBinding()
        {
            var bindings = new AnimationClipBindingConstant();
            var genericBindings = new List<GenericBinding>();
            var values = m_Binding;
            for (int i = 0; i < values.m_ValueArray.Length;)
            {
                var curveID = values.m_ValueArray[i].m_ID;
                var curveTypeID = values.m_ValueArray[i].m_TypeID;
                var binding = new GenericBinding();
                genericBindings.Add(binding);
                if (curveTypeID == 4174552735) //CRC(PositionX))
                {
                    binding.path = curveID;
                    binding.attribute = 1; //kBindTransformPosition
                    binding.typeID = ClassIDType.Transform;
                    i += 3;
                }
                else if (curveTypeID == 2211994246) //CRC(QuaternionX))
                {
                    binding.path = curveID;
                    binding.attribute = 2; //kBindTransformRotation
                    binding.typeID = ClassIDType.Transform;
                    i += 4;
                }
                else if (curveTypeID == 1512518241) //CRC(ScaleX))
                {
                    binding.path = curveID;
                    binding.attribute = 3; //kBindTransformScale
                    binding.typeID = ClassIDType.Transform;
                    i += 3;
                }
                else
                {
                    binding.typeID = ClassIDType.Animator;
                    binding.path = 0;
                    binding.attribute = curveID;
                    i++;
                }
            }
            bindings.genericBindings = genericBindings.ToArray();
            return bindings;
        }
    }

    public class ValueDelta
    {
        public float m_Start;
        public float m_Stop;

        public ValueDelta(ObjectReader reader)
        {
            m_Start = reader.ReadSingle();
            m_Stop = reader.ReadSingle();
        }
    }

    public class ClipMuscleConstant : IYAMLExportable
    {
        public HumanPose m_DeltaPose;
        public xform m_StartX;
        public xform m_StopX;
        public xform m_LeftFootStartX;
        public xform m_RightFootStartX;
        public xform m_MotionStartX;
        public xform m_MotionStopX;
        public Vector3 m_AverageSpeed;
        public Clip m_Clip;
        public float m_StartTime;
        public float m_StopTime;
        public float m_OrientationOffsetY;
        public float m_Level;
        public float m_CycleOffset;
        public float m_AverageAngularSpeed;
        public int[] m_IndexArray;
        public ValueDelta[] m_ValueArrayDelta;
        public float[] m_ValueArrayReferencePose;
        public bool m_Mirror;
        public bool m_LoopTime;
        public bool m_LoopBlend;
        public bool m_LoopBlendOrientation;
        public bool m_LoopBlendPositionY;
        public bool m_LoopBlendPositionXZ;
        public bool m_StartAtOrigin;
        public bool m_KeepOriginalOrientation;
        public bool m_KeepOriginalPositionY;
        public bool m_KeepOriginalPositionXZ;
        public bool m_HeightFromFeet;

        public ClipMuscleConstant(ObjectReader reader)
        {
            var version = reader.version;
            m_DeltaPose = new HumanPose(reader);
            m_StartX = new xform(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5))//5.5 and up
            {
                m_StopX = new xform(reader);
            }
            m_LeftFootStartX = new xform(reader);
            m_RightFootStartX = new xform(reader);
            if (version[0] < 5)//5.0 down
            {
                m_MotionStartX = new xform(reader);
                m_MotionStopX = new xform(reader);
            }
            m_AverageSpeed = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4();//5.4 and up
            m_Clip = new Clip(reader);
            m_StartTime = reader.ReadSingle();
            m_StopTime = reader.ReadSingle();
            m_OrientationOffsetY = reader.ReadSingle();
            m_Level = reader.ReadSingle();
            m_CycleOffset = reader.ReadSingle();
            m_AverageAngularSpeed = reader.ReadSingle();

            m_IndexArray = reader.ReadInt32Array();
            if (version[0] < 4 || (version[0] == 4 && version[1] < 3)) //4.3 down
            {
                var m_AdditionalCurveIndexArray = reader.ReadInt32Array();
            }
            int numDeltas = reader.ReadInt32();
            m_ValueArrayDelta = new ValueDelta[numDeltas];
            for (int i = 0; i < numDeltas; i++)
            {
                m_ValueArrayDelta[i] = new ValueDelta(reader);
            }
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3))//5.3 and up
            {
                m_ValueArrayReferencePose = reader.ReadSingleArray();
            }

            m_Mirror = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_LoopTime = reader.ReadBoolean();
            }
            m_LoopBlend = reader.ReadBoolean();
            m_LoopBlendOrientation = reader.ReadBoolean();
            m_LoopBlendPositionY = reader.ReadBoolean();
            m_LoopBlendPositionXZ = reader.ReadBoolean();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5))//5.5 and up
            {
                m_StartAtOrigin = reader.ReadBoolean();
            }
            m_KeepOriginalOrientation = reader.ReadBoolean();
            m_KeepOriginalPositionY = reader.ReadBoolean();
            m_KeepOriginalPositionXZ = reader.ReadBoolean();
            m_HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.AddSerializedVersion(2);
            node.Add(nameof(m_StartTime), m_StartTime);
            node.Add(nameof(m_StopTime), m_StopTime);
            node.Add(nameof(m_OrientationOffsetY), m_OrientationOffsetY);
            node.Add(nameof(m_Level), m_Level);
            node.Add(nameof(m_CycleOffset), m_CycleOffset);
            node.Add(nameof(m_LoopTime), m_LoopTime);
            node.Add(nameof(m_LoopBlend), m_LoopBlend);
            node.Add(nameof(m_LoopBlendOrientation), m_LoopBlendOrientation);
            node.Add(nameof(m_LoopBlendPositionY), m_LoopBlendPositionY);
            node.Add(nameof(m_LoopBlendPositionXZ), m_LoopBlendPositionXZ);
            node.Add(nameof(m_KeepOriginalOrientation), m_KeepOriginalOrientation);
            node.Add(nameof(m_KeepOriginalPositionY), m_KeepOriginalPositionY);
            node.Add(nameof(m_KeepOriginalPositionXZ), m_KeepOriginalPositionXZ);
            node.Add(nameof(m_HeightFromFeet), m_HeightFromFeet);
            node.Add(nameof(m_Mirror), m_Mirror);
            return node;
        }
    }

    public class GenericBinding : IYAMLExportable
    {
        public int[] version;
        public uint path;
        public uint attribute;
        public PPtr<Object> script;
        public ClassIDType typeID;
        public byte customType;
        public byte isPPtrCurve;
        
        public GenericBinding() { }

        public GenericBinding(ObjectReader reader)
        {
            version = reader.version;
            path = reader.ReadUInt32();
            attribute = reader.ReadUInt32();
            script = new PPtr<Object>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                typeID = (ClassIDType)reader.ReadInt32();
            }
            else
            {
                typeID = (ClassIDType)reader.ReadUInt16();
            }
            customType = reader.ReadByte();
            isPPtrCurve = reader.ReadByte();
            reader.AlignStream();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(path), path);
            node.Add(nameof(attribute), attribute);
            node.Add(nameof(script), script.ExportYAML());
            node.Add("classID", ((int)typeID).ToString());
            node.Add(nameof(customType), ((int)customType).ToString());
            node.Add(nameof(isPPtrCurve), isPPtrCurve);
            return node;
        }

        public int GetDimension() => attribute == 2 ? 4 : 3;
    }
    public class AnimationClipBindingConstant : IYAMLExportable
    {
        public GenericBinding[] genericBindings;
        public PPtr<Object>[] pptrCurveMapping;

        public AnimationClipBindingConstant() { }

        public AnimationClipBindingConstant(ObjectReader reader)
        {
            int numBindings = reader.ReadInt32();
            genericBindings = new GenericBinding[numBindings];
            for (int i = 0; i < numBindings; i++)
            {
                genericBindings[i] = new GenericBinding(reader);
            }

            int numMappings = reader.ReadInt32();
            pptrCurveMapping = new PPtr<Object>[numMappings];
            for (int i = 0; i < numMappings; i++)
            {
                pptrCurveMapping[i] = new PPtr<Object>(reader);
            }
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(genericBindings), genericBindings.ExportYAML());
            node.Add(nameof(pptrCurveMapping), pptrCurveMapping.ExportYAML());
            return node;
        }

        public GenericBinding FindBinding(int index)
        {
            int curves = 0;
            foreach (var b in genericBindings)
            {
                if (b.typeID == ClassIDType.Transform)
                {
                    switch (b.attribute)
                    {
                        case 1: //kBindTransformPosition
                        case 3: //kBindTransformScale
                        case 4: //kBindTransformEuler
                            curves += 3;
                            break;
                        case 2: //kBindTransformRotation
                            curves += 4;
                            break;
                        default:
                            curves += 1;
                            break;
                    }
                }
                else
                {
                    curves += 1;
                }
                if (curves > index)
                {
                    return b;
                }
            }

            return null;
        }
    }

    public class AnimationEvent : IYAMLExportable
    {
        public float time;
        public string functionName;
        public string data;
        public PPtr<Object> objectReferenceParameter;
        public float floatParameter;
        public int intParameter;
        public int messageOptions;

        public AnimationEvent(ObjectReader reader)
        {
            var version = reader.version;

            time = reader.ReadSingle();
            functionName = reader.ReadAlignedString();
            data = reader.ReadAlignedString();
            objectReferenceParameter = new PPtr<Object>(reader);
            floatParameter = reader.ReadSingle();
            if (version[0] >= 3) //3 and up
            {
                intParameter = reader.ReadInt32();
            }
            messageOptions = reader.ReadInt32();
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(time), time);
            node.Add(nameof(functionName), functionName);
            node.Add(nameof(data), data);
            node.Add(nameof(objectReferenceParameter), objectReferenceParameter.ExportYAML());
            node.Add(nameof(floatParameter), floatParameter);
            node.Add(nameof(intParameter), intParameter);
            node.Add(nameof(messageOptions), messageOptions);
            return node;
        }
    }

    public enum AnimationType
    {
        Legacy = 1,
        Generic = 2,
        Humanoid = 3
    };

    public sealed class AnimationClip : NamedObject, IYAMLExportable
    {
        public AnimationType m_AnimationType;
        public bool m_Legacy;
        public bool m_Compressed;
        public bool m_UseHighQualityCurve;
        public QuaternionCurve[] m_RotationCurves;
        public CompressedAnimationCurve[] m_CompressedRotationCurves;
        public Vector3Curve[] m_EulerCurves;
        public Vector3Curve[] m_PositionCurves;
        public Vector3Curve[] m_ScaleCurves;
        public FloatCurve[] m_FloatCurves;
        public PPtrCurve[] m_PPtrCurves;
        public float m_SampleRate;
        public int m_WrapMode;
        public AABB m_Bounds;
        public uint m_MuscleClipSize;
        public ClipMuscleConstant m_MuscleClip;
        public byte[] m_AclClipData;
        public GenericBinding[] m_AclBindings;
        public KeyValuePair<float, float> m_AclRange;
        public AnimationClipBindingConstant m_ClipBindingConstant;
        public AnimationEvent[] m_Events;


        public AnimationClip(ObjectReader reader) : base(reader)
        {
            if (version[0] >= 5)//5.0 and up
            {
                m_Legacy = reader.ReadBoolean();
            }
            else if (version[0] >= 4)//4.0 and up
            {
                m_AnimationType = (AnimationType)reader.ReadInt32();
                if (m_AnimationType == AnimationType.Legacy)
                    m_Legacy = true;
            }
            else
            {
                m_Legacy = true;
            }
            m_Compressed = reader.ReadBoolean();
            if (m_Compressed && reader.Game.Name != "ToT")
            {
                m_Compressed = false;
            }
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3))//4.3 and up
            {
                m_UseHighQualityCurve = reader.ReadBoolean();
            }
            reader.AlignStream();
            int numRCurves = reader.ReadInt32();
            m_RotationCurves = new QuaternionCurve[numRCurves];
            for (int i = 0; i < numRCurves; i++)
            {
                m_RotationCurves[i] = new QuaternionCurve(reader);
            }

            int numCRCurves = reader.ReadInt32();
            m_CompressedRotationCurves = new CompressedAnimationCurve[numCRCurves];
            for (int i = 0; i < numCRCurves; i++)
            {
                m_CompressedRotationCurves[i] = new CompressedAnimationCurve(reader);
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3))//5.3 and up
            {
                int numEulerCurves = reader.ReadInt32();
                m_EulerCurves = new Vector3Curve[numEulerCurves];
                for (int i = 0; i < numEulerCurves; i++)
                {
                    m_EulerCurves[i] = new Vector3Curve(reader);
                }
            }

            int numPCurves = reader.ReadInt32();
            m_PositionCurves = new Vector3Curve[numPCurves];
            for (int i = 0; i < numPCurves; i++)
            {
                m_PositionCurves[i] = new Vector3Curve(reader);
            }

            int numSCurves = reader.ReadInt32();
            m_ScaleCurves = new Vector3Curve[numSCurves];
            for (int i = 0; i < numSCurves; i++)
            {
                m_ScaleCurves[i] = new Vector3Curve(reader);
            }

            int numFCurves = reader.ReadInt32();
            m_FloatCurves = new FloatCurve[numFCurves];
            for (int i = 0; i < numFCurves; i++)
            {
                m_FloatCurves[i] = new FloatCurve(reader);
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                int numPtrCurves = reader.ReadInt32();
                m_PPtrCurves = new PPtrCurve[numPtrCurves];
                for (int i = 0; i < numPtrCurves; i++)
                {
                    m_PPtrCurves[i] = new PPtrCurve(reader);
                }
            }

            m_SampleRate = reader.ReadSingle();
            m_WrapMode = reader.ReadInt32();
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 4)) //3.4 and up
            {
                m_Bounds = new AABB(reader);
            }
            if (version[0] >= 4)//4.0 and up
            {
                m_MuscleClipSize = reader.ReadUInt32();
                m_MuscleClip = new ClipMuscleConstant(reader);
            }
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                if (reader.Game.Name == "SR")
                {
                    m_AclClipData = reader.ReadUInt8Array();
                    var aclBindingsCount = reader.ReadInt32();
                    m_AclBindings = new GenericBinding[aclBindingsCount];
                    for (int i = 0; i < aclBindingsCount; i++)
                    {
                        m_AclBindings[i] = new GenericBinding(reader);
                    }
                    m_AclRange = new KeyValuePair<float, float>(reader.ReadSingle(), reader.ReadSingle());
                }
                m_ClipBindingConstant = new AnimationClipBindingConstant(reader);
            }
            if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 3)) //2018.3 and up
            {
                var m_HasGenericRootTransform = reader.ReadBoolean();
                var m_HasMotionFloatCurves = reader.ReadBoolean();
                reader.AlignStream();
            }
            int numEvents = reader.ReadInt32();
            m_Events = new AnimationEvent[numEvents];
            for (int i = 0; i < numEvents; i++)
            {
                m_Events[i] = new AnimationEvent(reader);
            }
            if (version[0] >= 2017) //2017 and up
            {
                reader.AlignStream();
            }
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add(nameof(m_Name), m_Name);
            node.AddSerializedVersion(6);
            node.Add(nameof(m_Legacy), m_Legacy);
            node.Add(nameof(m_Compressed), m_Compressed);
            node.Add(nameof(m_UseHighQualityCurve), m_UseHighQualityCurve);
            node.Add(nameof(m_RotationCurves), m_RotationCurves.ExportYAML());
            node.Add(nameof(m_CompressedRotationCurves), m_CompressedRotationCurves.ExportYAML());
            node.Add(nameof(m_EulerCurves), m_EulerCurves.ExportYAML());
            node.Add(nameof(m_PositionCurves), m_PositionCurves.ExportYAML());
            node.Add(nameof(m_ScaleCurves), m_ScaleCurves.ExportYAML());
            node.Add(nameof(m_FloatCurves), m_FloatCurves.ExportYAML());
            node.Add(nameof(m_PPtrCurves), m_PPtrCurves.ExportYAML());
            node.Add(nameof(m_SampleRate), m_SampleRate);
            node.Add(nameof(m_WrapMode), m_WrapMode);
            node.Add(nameof(m_Bounds), m_Bounds.ExportYAML());
            node.Add(nameof(m_ClipBindingConstant), m_ClipBindingConstant.ExportYAML());
            node.Add("m_AnimationClipSettings", m_MuscleClip.ExportYAML());
            node.Add(nameof(m_Events), m_Events.ExportYAML());
            return node;
        }

        public Dictionary<uint, string> FindTOS()
        {
            var tos = new Dictionary<uint, string>() { { 0, string.Empty } };
            foreach (var asset in assetsFile.assetsManager.assetsFileList.SelectMany(x => x.Objects).OrderBy(x => x.type).ToArray())
            {
                switch (asset.type)
                {
                    case ClassIDType.Avatar:
                        var avatar = asset as Avatar;
                        if (AddAvatarTOS(avatar, tos))
                        {
                            return tos;
                        }
                        break;
                    case ClassIDType.Animator:
                        var animator = asset as Animator;
                        if (IsAnimatorContainsClip(animator))
                        {
                            if (AddAnimatorTOS(animator, tos))
                            {
                                return tos;
                            }
                        }
                        break;
                    case ClassIDType.Animation:
                        var animation = asset as Animation;
                        if (IsAnimationContainsClip(animation))
                        {
                            if (AddAnimationTOS(animation, tos))
                            {
                                return tos;
                            }
                        }
                        break;
                }
            }
            return tos;
        }
        public IEnumerable<GameObject> FindRoots()
        {
            foreach (var asset in assetsFile.assetsManager.assetsFileList.SelectMany(x => x.Objects))
            {
                switch (asset.type)
                {
                    case ClassIDType.Animator:
                        Animator animator = (Animator)asset;
                        if (IsAnimatorContainsClip(animator))
                        {
                            if (animator.m_GameObject.TryGet(out var go))
                            {
                                yield return go;
                            }
                        }
                        break;

                    case ClassIDType.Animation:
                        Animation animation = (Animation)asset;
                        if (IsAnimationContainsClip(animation))
                        {
                            if (animation.m_GameObject.TryGet(out var go))
                            {
                                yield return go;
                            }
                        }
                        break;
                }
            }

            yield break;
        }
        private bool IsAnimatorContainsClip(Animator animator)
        {
            if (animator.m_Controller.TryGet(out var runtime))
            {
                return runtime.IsContainsAnimationClip(this);
            }
            else
            {
                return false;
            }
        }
        private bool IsAnimationContainsClip(Animation animation)
        {
            return animation.IsContainsAnimationClip(this);
        }
        private bool AddAvatarTOS(Avatar avatar, Dictionary<uint, string> tos)
        {
            return AddTOS(avatar.m_TOS.ToDictionary(x => x.Key, x => x.Value), tos);
        }
        private bool AddAnimatorTOS(Animator animator, Dictionary<uint, string> tos)
        {
            if (animator.m_Avatar.TryGet(out var avatar))
            {
                if (AddAvatarTOS(avatar, tos))
                {
                    return true;
                }
            }

            Dictionary<uint, string> animatorTOS = animator.BuildTOS();
            return AddTOS(animatorTOS, tos);
        }
        private bool AddAnimationTOS(Animation animation, Dictionary<uint, string> tos)
        {
            if (animation.m_GameObject.TryGet(out var go))
            {
                Dictionary<uint, string> animationTOS = go.BuildTOS();
                return AddTOS(animationTOS, tos);
            }
            return false;
        }
        private bool AddTOS(Dictionary<uint, string> src, Dictionary<uint, string> dest)
        {
            int tosCount = m_ClipBindingConstant.genericBindings.Length;
            for (int i = 0; i < tosCount; i++)
            {
                ref GenericBinding binding = ref m_ClipBindingConstant.genericBindings[i];
                if (src.TryGetValue(binding.path, out string path))
                {
                    dest[binding.path] = path;
                    if (dest.Count == tosCount)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
