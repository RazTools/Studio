using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MeshRenderer : Renderer
    {
        public PPtr<Mesh> m_AdditionalVertexStreams;
        public MeshRenderer(ObjectReader reader) : base(reader)
        {
            m_AdditionalVertexStreams = new PPtr<Mesh>(reader);
        }
    }
}
