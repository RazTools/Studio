namespace AssetStudio
{
    public struct Float : IYAMLExportable
    {
        public float Value;

        public Float(float value)
        {
            Value = value;
        }

        public static implicit operator Float(float value)
        {
            return new Float(value);
        }

        public static implicit operator float(Float value)
        {
            return value.Value;
        }

        public YAMLNode ExportYAML()
        {
            return new YAMLScalarNode(Value);
        }
    }
}
