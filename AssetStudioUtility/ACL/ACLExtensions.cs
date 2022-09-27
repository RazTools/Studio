namespace AssetStudio
{
    public static class ACLExtensions
    {
        public static void Process(this ACLClip m_ACLClip, out float[] values, out float[] times) => ACL.ACL.DecompressAll(m_ACLClip.m_ClipData, out values, out times);
        public static void ProcessSR(this ACLClip m_ACLClip, out float[] values, out float[] times) => ACL.SRACL.DecompressAll(m_ACLClip.m_ClipDataUint, out values, out times);
    }
}
