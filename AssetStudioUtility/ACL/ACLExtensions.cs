using ACLLibs;

namespace AssetStudio
{
    public static class ACLExtensions
    {
        public static void Process(this ACLClip m_ACLClip, Game game, out float[] values, out float[] times) 
        {
            if (game.Type.IsSRGroup())
            {
                SRACL.DecompressAll(m_ACLClip.m_ClipData, out values, out times);
            }
            else
            {
                if (!m_ACLClip.m_DatabaseData.IsNullOrEmpty())
                {
                    DBACL.DecompressTracks(m_ACLClip.m_ClipData, m_ACLClip.m_DatabaseData, out values, out times);
                }
                else
                {
                    ACL.DecompressAll(m_ACLClip.m_ClipData, out values, out times);
                }
            }
        }
    }
}
