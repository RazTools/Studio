using System;
using System.Collections.Generic;

namespace AssetStudio
{
	public static class IDictionaryExportYAMLExtensions
	{
		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<int, T> _this)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML());
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<string, T> _this)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML());
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<Tuple<T1, long>, T2> _this)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			// TODO: test
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				YAMLMappingNode keyMap = new YAMLMappingNode();
				keyMap.Add("first", kvp.Key.Item1.ExportYAML());
				keyMap.Add("second", kvp.Key.Item2);
				kvpMap.Add("first", keyMap);
				kvpMap.Add("second", kvp.Value.ExportYAML());
				node.Add(kvpMap);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, int> _this)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML();
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value);
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value);
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, float> _this)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML();
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value);
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value);
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2> _this)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML();
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML());
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML());
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2[]> _this)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML();
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML());
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML());
				}
				node.Add(map);
			}
			return node;
		}
	}
}
