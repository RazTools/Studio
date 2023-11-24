using System.Collections.Generic;

namespace AssetStudio
{
    public class ResourceManager : Object
    {
        public List<KeyValuePair<string, PPtr<Object>>> m_Container;

        public ResourceManager(ObjectReader reader) : base(reader)
        {
            var m_ContainerSize = reader.ReadInt32();
            m_Container = new List<KeyValuePair<string, PPtr<Object>>>();
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container.Add(new KeyValuePair<string, PPtr<Object>>(reader.ReadAlignedString(), new PPtr<Object>(reader)));
            }
        }
    }
}
