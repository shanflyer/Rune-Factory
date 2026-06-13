using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GameActionNodeGenerator
{
    private const string OutputDir = "Assets/Scripts/Action/TypedNodes";

    [MenuItem("Tools/GameAction/Generate Missing Nodes")]
    public static void GenerateMissingNodes()
    {
        Directory.CreateDirectory(OutputDir);

        var actionTypes = typeof(GameAction).Assembly.GetTypes()
            .Where(t => t.IsValueType && !t.IsEnum && typeof(GameAction).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToList();

        var created = 0;
        var skipped = 0;

        foreach (var actionType in actionTypes)
        {
            var nodeName = actionType.Name + "Node";
            var path = Path.Combine(OutputDir, nodeName + ".cs").Replace("\\", "/");

            if (File.Exists(path))
            {
                skipped++;
                continue;
            }

            File.WriteAllText(path, GenerateNodeCode(actionType, nodeName), Encoding.UTF8);
            created++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[GameActionNodeGenerator] Created {created} node(s), skipped {skipped} existing node(s).");
    }

    private static string GenerateNodeCode(Type actionType, string nodeName)
    {
        var fields = GetConfigFields(actionType).ToList();
        var usings = new SortedSet<string> { "System", "UnityEngine" };

        foreach (var field in fields)
        {
            CollectUsings(field.FieldType, usings);
        }

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated for new GameAction assets. Add custom runtime overrides manually if needed.");
        foreach (var ns in usings)
        {
            sb.AppendLine("using " + ns + ";");
        }

        sb.AppendLine();
        sb.AppendLine("public class " + nodeName + " : ActionNode");
        sb.AppendLine("{");

        foreach (var field in fields)
        {
            sb.AppendLine("    public " + GetTypeName(field.FieldType) + " " + field.Name + ";");
        }

        if (fields.Count > 0) sb.AppendLine();

        sb.AppendLine("    public override GameAction CreateAction(");
        sb.AppendLine("        int source = 0, int target = 0, int value = -1,");
        sb.AppendLine("        SetResult setResult = null, SetValue setValue = null,");
        sb.AppendLine("        bool immediately = false)");
        sb.AppendLine("    {");
        sb.AppendLine("        var action = new " + actionType.Name);
        sb.AppendLine("        {");
        foreach (var field in fields)
        {
            sb.AppendLine("            " + field.Name + " = this." + field.Name + ",");
        }

        sb.AppendLine("        };");
        sb.AppendLine("        action.setValue = setValue;");
        sb.AppendLine("        action.setResult = setResult;");
        sb.AppendLine("        GameActionManager.instance.QueueAction(action, immediately);");
        sb.AppendLine("        return action;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static IEnumerable<FieldInfo> GetConfigFields(Type actionType)
    {
        return actionType
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.DeclaringType == actionType)
            .Where(f => !f.IsDefined(typeof(NonSerializedAttribute), true))
            .Where(f => f.Name != nameof(GameAction.setValue))
            .Where(f => f.Name != nameof(GameAction.setResult))
            .Where(f => IsSupportedConfigField(f.FieldType));
    }

    private static bool IsSupportedConfigField(Type type)
    {
        if (typeof(Delegate).IsAssignableFrom(type)) return false;
        if (type == typeof(Action)) return false;
        if (type == typeof(SetValue) || type == typeof(SetResult) || type == typeof(SetPanelReference)) return false;
        return true;
    }

    private static void CollectUsings(Type type, ISet<string> usings)
    {
        if (type.IsGenericType)
        {
            usings.Add("System.Collections.Generic");
            foreach (var arg in type.GetGenericArguments())
            {
                CollectUsings(arg, usings);
            }
        }

        if (!string.IsNullOrEmpty(type.Namespace) &&
            (type.Namespace == "Unity.Mathematics" || type.Namespace == "UnityEngine"))
        {
            usings.Add(type.Namespace);
        }
    }

    private static string GetTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(string)) return "string";
        if (type == typeof(double)) return "double";

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name;
        var tick = name.IndexOf('`');
        if (tick >= 0) name = name.Substring(0, tick);

        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(GetTypeName)) + ">";
    }
}
