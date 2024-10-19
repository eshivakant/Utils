using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

public static class JsonToCSharpGenerator
{
    public static string GenerateCSharpCode(string json)
    {
        var jsonDoc = JsonDocument.Parse(json);
        var sb = new StringBuilder();

        sb.AppendLine("public static MyClass CreateFromJson()");
        sb.AppendLine("{");
        sb.AppendLine("    return new MyClass");
        sb.AppendLine("    {");

        foreach (var property in jsonDoc.RootElement.EnumerateObject())
        {
            GeneratePropertyAssignment(sb, property);
        }

        sb.AppendLine("    };");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GeneratePropertyAssignment(StringBuilder sb, JsonProperty property)
    {
        switch (property.Value.ValueKind)
        {
            case JsonValueKind.Number:
                sb.AppendLine($"        {property.Name} = {property.Value.GetDouble()},");
                break;

            case JsonValueKind.String:
                sb.AppendLine($"        {property.Name} = \"{property.Value.GetString()}\",");
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                sb.AppendLine($"        {property.Name} = {property.Value.GetBoolean().ToString().ToLower()},");
                break;

            case JsonValueKind.Array:
                GenerateArrayAssignment(sb, property);
                break;

            case JsonValueKind.Object:
                sb.AppendLine($"        {property.Name} = new {property.Name}");
                sb.AppendLine("        {");
                foreach (var nestedProperty in property.Value.EnumerateObject())
                {
                    GeneratePropertyAssignment(sb, nestedProperty);
                }
                sb.AppendLine("        },");
                break;

            default:
                sb.AppendLine($"        {property.Name} = default,");
                break;
        }
    }

    private static void GenerateArrayAssignment(StringBuilder sb, JsonProperty property)
    {
        var firstElement = property.Value.EnumerateArray().FirstOrDefault();

        if (firstElement.ValueKind == JsonValueKind.String)
        {
            sb.AppendLine($"        {property.Name} = new List<string> {{ {string.Join(", ", property.Value.EnumerateArray().Select(v => $"\"{v.GetString()}\""))} }},");
        }
        else if (firstElement.ValueKind == JsonValueKind.Number)
        {
            sb.AppendLine($"        {property.Name} = new List<double> {{ {string.Join(", ", property.Value.EnumerateArray().Select(v => v.GetDouble().ToString()))} }},");
        }
        else if (firstElement.ValueKind == JsonValueKind.True || firstElement.ValueKind == JsonValueKind.False)
        {
            sb.AppendLine($"        {property.Name} = new List<bool> {{ {string.Join(", ", property.Value.EnumerateArray().Select(v => v.GetBoolean().ToString().ToLower()))} }},");
        }
        else if (firstElement.ValueKind == JsonValueKind.Object)
        {
            sb.AppendLine($"        {property.Name} = new List<{property.Name}Item> {{");
            foreach (var element in property.Value.EnumerateArray())
            {
                sb.AppendLine("            new " + property.Name + "Item");
                sb.AppendLine("            {");
                foreach (var nestedProperty in element.EnumerateObject())
                {
                    GeneratePropertyAssignment(sb, nestedProperty);
                }
                sb.AppendLine("            },");
            }
            sb.AppendLine("        },");
        }
        else
        {
            sb.AppendLine($"        {property.Name} = new List<object>(), // Unsupported element type");
        }
    }
}
