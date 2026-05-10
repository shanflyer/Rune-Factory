using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GameAction 强类型 ScriptableObject 代码生成器（纯 C# 版，集成 Unity Editor）
/// 
/// 用法: Tools → GameAction → 重新生成强类型
/// 扫描所有 GameAction struct → 生成对应的 XxxActionData.cs
/// </summary>
public static class GameActionCodeGenerator
{
	private const string OUTPUT_DIR = "Assets/Scripts/Action/TypedActions";
	private static readonly HashSet<string> ExcludedFields = new() { "setValue", "setResult", "endAction" };
	private static readonly HashSet<string> ShadowedFields = new() { "id", "name" };
	private static readonly HashSet<string> SimpleTypes = new()
	{
		"int", "bool", "string", "float", "Vector2", "Vector3", "Vector2Int", "Vector3Int"
	};

	[MenuItem("Tools/GameAction/重新生成强类型", priority = 100)]
	public static void RegenerateAll()
	{
		Directory.CreateDirectory(OUTPUT_DIR);

		var interfaceType = typeof(GameAction);
		var structTypes = new List<Type>();

		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (asm.GetName().Name.StartsWith("Unity") ||
			    asm.GetName().Name.StartsWith("System") ||
			    asm.GetName().Name == "mscorlib")
				continue;

			try
			{
				foreach (var t in asm.GetTypes())
				{
					if (t.IsValueType && !t.IsAbstract && interfaceType.IsAssignableFrom(t) && t != interfaceType)
						structTypes.Add(t);
				}
			}
			catch { }
		}

		structTypes = structTypes.OrderBy(t => t.Name).ToList();
		Debug.Log($"[GameAction生成器] 扫描到 {structTypes.Count} 个 GameAction struct");

		int generated = 0;
		var errors = new List<string>();

		try
		{
			AssetDatabase.StartAssetEditing();
		}
		catch { }

		foreach (var type in structTypes)
		{
			try
			{
				GenerateForType(type);
				generated++;
			}
			catch (Exception e)
			{
				errors.Add($"  {type.Name}: {e.Message}");
			}
		}

		try { AssetDatabase.StopAssetEditing(); } catch { }
		AssetDatabase.Refresh();

		Debug.Log($"[GameAction生成器] 完成: ✓{generated}" +
		          (errors.Count > 0 ? $" ✗{errors.Count}" : ""));
		foreach (var e in errors) Debug.LogError(e);
	}

	private static void GenerateForType(Type structType)
	{
		var structName = structType.Name;
		var fileName = $"{structName}ActionData.cs";
		var filePath = Path.Combine(OUTPUT_DIR, fileName);

		// 提取字段 + Init() 信息
		var fields = ExtractStructFields(structType);
		var initInfo = ExtractInitInfo(structType, fields);
		var code = GenerateCode(structName, fields, initInfo);

		// 只有内容变化才写入（避免不必要的 reimport）
		if (File.Exists(filePath))
		{
			var existing = File.ReadAllText(filePath);
			if (existing == code) return;
		}

		File.WriteAllText(filePath, code, new UTF8Encoding(false));
	}

	private static Dictionary<string, string> ExtractStructFields(Type type)
	{
		var result = new Dictionary<string, string>();

		foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
		{
			if (ExcludedFields.Contains(f.Name)) continue;
			if (f.FieldType == typeof(Action)) continue;
			if (typeof(Delegate).IsAssignableFrom(f.FieldType)) continue;

			var simplified = SimplifyType(f.FieldType);
			if (simplified == null) continue;

			result[f.Name] = simplified;
		}

		return result;
	}

	private static string SimplifyType(Type type)
	{
		if (type == typeof(int))    return "int";
		if (type == typeof(bool))   return "bool";
		if (type == typeof(string)) return "string";
		if (type == typeof(float))  return "float";
		if (type == typeof(Vector2))     return "Vector2";
		if (type == typeof(Vector3))     return "Vector3";
		if (type == typeof(Vector2Int))  return "Vector2Int";
		if (type == typeof(Vector3Int))  return "Vector3Int";

		return null; // 复杂类型不纳入生成
	}

	private static (List<(string name, int index, string type)> paramFields,
	                HashSet<string> sourceOverrides,
	                HashSet<string> targetOverrides,
	                HashSet<string> valueOverrides)
	ExtractInitInfo(Type type, Dictionary<string, string> structFields)
	{
		var paramFields = new List<(string name, int index, string type)>();
		var sourceOverrides = new HashSet<string>();
		var targetOverrides = new HashSet<string>();
		var valueOverrides = new HashSet<string>();

		var initMethod = type.GetMethod("Init");
		if (initMethod == null) return (paramFields, sourceOverrides, targetOverrides, valueOverrides);

		// 读取 Init() 源码字符串
		string initSource = ReadMethodBody(type, "Init");
		if (string.IsNullOrEmpty(initSource))
			return (paramFields, sourceOverrides, targetOverrides, valueOverrides);

		// 1. 字段 = parameters[N].value 映射
		var seen = new HashSet<string>();
		foreach (Match match in Regex.Matches(initSource, @"(\w+)\s*=\s*int\.Parse\s*\(\s*parameters\s*\[(\d+)\]\s*\.value\s*\)"))
		{
			var fname = match.Groups[1].Value;
			var idx   = int.Parse(match.Groups[2].Value);
			if (structFields.ContainsKey(fname) && seen.Add(fname))
				paramFields.Add((fname, idx, "int"));
		}
		foreach (Match match in Regex.Matches(initSource, @"(\w+)\s*=\s*bool\.Parse\s*\(\s*parameters\s*\[(\d+)\]\s*\.value\s*\)"))
		{
			var fname = match.Groups[1].Value;
			var idx   = int.Parse(match.Groups[2].Value);
			if (structFields.ContainsKey(fname) && seen.Add(fname))
				paramFields.Add((fname, idx, "bool"));
		}
		foreach (Match match in Regex.Matches(initSource, @"(\w+)\s*=\s*float\.Parse\s*\(\s*parameters\s*\[(\d+)\]\s*\.value\s*\)"))
		{
			var fname = match.Groups[1].Value;
			var idx   = int.Parse(match.Groups[2].Value);
			if (structFields.ContainsKey(fname) && seen.Add(fname))
				paramFields.Add((fname, idx, "float"));
		}
		foreach (Match match in Regex.Matches(initSource, @"(\w+)\s*=\s*parameters\s*\[(\d+)\]\s*\.value(?!\s*\.)"))
		{
			var fname = match.Groups[1].Value;
			var idx   = int.Parse(match.Groups[2].Value);
			if (structFields.ContainsKey(fname) && seen.Add(fname))
				paramFields.Add((fname, idx, "string"));
		}

		// 2. source/target/value 覆盖
		foreach (Match m in Regex.Matches(initSource, @"(\w+)\s*=\s*source\s*;"))
			if (structFields.ContainsKey(m.Groups[1].Value))
				sourceOverrides.Add(m.Groups[1].Value);
		foreach (Match m in Regex.Matches(initSource, @"(\w+)\s*=\s*target\s*;"))
			if (structFields.ContainsKey(m.Groups[1].Value))
				targetOverrides.Add(m.Groups[1].Value);
		foreach (Match m in Regex.Matches(initSource, @"(\w+)\s*=\s*value\s*;"))
			if (structFields.ContainsKey(m.Groups[1].Value))
				valueOverrides.Add(m.Groups[1].Value);

		return (paramFields, sourceOverrides, targetOverrides, valueOverrides);
	}

	/// <summary>读取方法体源码（扫描 Action 目录）</summary>
	private static string ReadMethodBody(Type type, string methodName)
	{
		var actionDir = Path.Combine(Application.dataPath, "Scripts", "Action");
		if (!Directory.Exists(actionDir)) return null;

		foreach (var file in Directory.GetFiles(actionDir, "*.cs"))
		{
			string content;
			try { content = File.ReadAllText(file); }
			catch { continue; }

			// 确认此文件包含该 struct 定义
			if (!content.Contains($"struct {type.Name} ") &&
			    !Regex.IsMatch(content, $@"struct\s+{type.Name}\s*:") ) 
				continue;
				
			var structIdx = content.IndexOf($"struct {type.Name} ");
			if (structIdx < 0)
				structIdx = Regex.Match(content, $@"struct\s+{Regex.Escape(type.Name)}\s*:").Index;
			if (structIdx < 0) continue;

			var methodSig = $"void {methodName}";
			var sigIdx = content.IndexOf(methodSig, structIdx);
			if (sigIdx < 0) continue;

			var braceIdx = content.IndexOf('{', sigIdx);
			if (braceIdx < 0) continue;

			return ExtractBraceBlock(content, braceIdx);
		}
		return null;
	}

	private static string ExtractBraceBlock(string text, int start)
	{
		int depth = 0;
		for (int i = start; i < text.Length; i++)
		{
			if (text[i] == '{') depth++;
			else if (text[i] == '}')
			{
				depth--;
				if (depth == 0)
					return text.Substring(start + 1, i - start - 1);
			}
		}
		return text.Substring(start + 1);
	}

	private static string GenerateCode(string structName,
		Dictionary<string, string> fields,
		(List<(string, int, string)> paramFields,
		 HashSet<string> src, HashSet<string> tgt, HashSet<string> val) initInfo)
	{
		var sb = new StringBuilder();
		sb.AppendLine("// ────────────────────────────────────────────────────");
		sb.AppendLine($"// 自动生成: GameAction 强类型配置 - {structName}");
		sb.AppendLine("// ────────────────────────────────────────────────────");
		sb.AppendLine("using UnityEngine;");
		sb.AppendLine();
		sb.AppendLine($"[CreateAssetMenu(menuName = \"GameAction/{structName}\")]");
		sb.AppendLine($"public class {structName}ActionData : GameActionBaseData");
		sb.AppendLine("{");

		var paramFields = initInfo.paramFields.OrderBy(x => x.Item2).ToList();
		var usedNames = new HashSet<string>();

		// 参数字段
		foreach (var entry in paramFields)
		{
			var name  = entry.Item1;
			var idx   = entry.Item2;
			var ptype = entry.Item3;
			var actualType = fields.TryGetValue(name, out var ft) ? ft : ptype;
			var ct = actualType switch
			{
				"int" => "int", "bool" => "bool", "float" => "float",
				"Vector2" => "Vector2", "Vector3" => "Vector3",
				"Vector2Int" => "Vector2Int", "Vector3Int" => "Vector3Int",
				_ => "string"
			};
			var kw = ShadowedFields.Contains(name) ? "new " : "";
			sb.AppendLine($"        public {kw}{ct} {name};");
			usedNames.Add(name);
		}

		// 未映射的 struct 字段
		foreach (var kv in fields)
		{
			if (usedNames.Contains(kv.Key)) continue;
			var kw = ShadowedFields.Contains(kv.Key) ? "new " : "";
			sb.AppendLine($"        public {kw}{kv.Value} {kv.Key};");
			usedNames.Add(kv.Key);
		}

		sb.AppendLine();
		sb.AppendLine("    public override GameAction CreateAction(");
		sb.AppendLine("        int source = 0, int target = 0, int value = -1,");
		sb.AppendLine("        SetResult setResult = null, SetValue setValue = null,");
		sb.AppendLine("        bool immediately = false)");
		sb.AppendLine("    {");

		// struct 初始化
		sb.AppendLine($"        var action = new {structName}");
		sb.AppendLine("        {");
		var allFields = new List<string>();
		foreach (var entry in paramFields) allFields.Add(entry.Item1);
		foreach (var kv in fields.Where(x => !usedNames.Contains(x.Key) || paramFields.Any(p => p.Item1 == x.Key)))
			if (!allFields.Contains(kv.Key))
				allFields.Add(kv.Key);
		// 去重
		var written = new HashSet<string>();
		foreach (var fname in allFields)
		{
			if (!written.Add(fname)) continue;
			sb.AppendLine($"                {fname} = this.{fname},");
		}
		sb.AppendLine("        };");
		sb.AppendLine();
		sb.AppendLine("        action.setValue = setValue;");
		sb.AppendLine("        action.setResult = setResult;");

		// 覆盖逻辑
		foreach (var f in initInfo.src)
			sb.AppendLine($"            if (source != 0 && source != int.MinValue) action.{f} = source;");
		foreach (var f in initInfo.tgt)
			sb.AppendLine($"            if (target != 0 && target != int.MinValue) action.{f} = target;");
		foreach (var f in initInfo.val)
			sb.AppendLine($"            if (value != -1) action.{f} = value;");

		sb.AppendLine();
		sb.AppendLine("        GameActionManager.instance.QueueAction(action, immediately);");
		sb.AppendLine("        return action;");
		sb.AppendLine("    }");
		sb.AppendLine("}");

		return sb.ToString();
	}
}
