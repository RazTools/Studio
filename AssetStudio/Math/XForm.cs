using System;
using System.Runtime.InteropServices;

namespace AssetStudio
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct XForm : IEquatable<XForm>
    {
        public Vector3 t;
        public Quaternion q;
        public Vector3 s;

        public XForm(Vector3 t, Quaternion q, Vector3 s)
        {
            this.t = t;
            this.q = q;
            this.s = s;
        }
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return t.X;
                    case 1: return t.Y;
                    case 2: return t.Z;
                    case 3: return q.X;
                    case 4: return q.Y;
                    case 5: return q.Z;
                    case 6: return q.W;
                    case 7: return s.X;
                    case 8: return s.Y;
                    case 9: return s.Z;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid xform index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: t.X = value; break;
                    case 1: t.Y = value; break;
                    case 2: t.Z = value; break;
                    case 3: q.X = value; break;
                    case 4: q.Y = value; break;
                    case 5: q.Z = value; break;
                    case 6: q.W = value; break;
                    case 7: s.X = value; break;
                    case 8: s.Y = value; break;
                    case 9: s.Z = value; break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Invalid xform index!");
                }
            }
        }

        public override int GetHashCode()
        {
            return t.GetHashCode() ^ (q.GetHashCode() << 2) ^ (s.GetHashCode() >> 2);
        }

        bool IEquatable<XForm>.Equals(XForm other)
        {
            return t.Equals(other.t) && q.Equals(other.q) && s.Equals(other.s);
        }

        public static XForm Zero => new XForm(Vector3.Zero, Quaternion.Zero, Vector3.One);
    }
}
