using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MeshRenderer : Renderer
    {
        public MeshRenderer(ObjectReader reader) : base(reader)
        {
            var m_AdditionalVertexStreams = new PPtr<Mesh>(reader);
        }
    }
}
