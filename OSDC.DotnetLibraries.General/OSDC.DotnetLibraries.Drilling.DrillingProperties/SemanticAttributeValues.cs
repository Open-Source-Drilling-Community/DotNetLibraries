using DWIS.API.DTO;
using DWIS.Vocabulary.Schemas;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties;

internal static class SemanticAttributeValues
{
    // Use generated schema metadata rather than guessing a type from the supplied value.
    private static readonly Dictionary<string, string[]> Types = typeof(DWIS.Vocabulary.Schemas.Attributes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && f.FieldType == typeof(string) && !f.Name.EndsWith("_Type", StringComparison.Ordinal))
        .Select(f => (Name: (string)f.GetRawConstantValue()!, Type: typeof(DWIS.Vocabulary.Schemas.Attributes)
            .GetField(f.Name + "_Type")?.GetRawConstantValue() as string))
        .Where(f => f.Type is not null)
        .GroupBy(f => f.Name, StringComparer.Ordinal)
        .ToDictionary(g => g.Key, g => g.Select(f => f.Type!).Distinct().ToArray(), StringComparer.Ordinal);

    internal static string? Target(SemanticFact fact) =>
        fact.Verb == Verbs.Enum.BelongsToClass ? fact.SubjectName : fact.ObjectName;

    internal static IEnumerable<(string Name, string Value, string Datatype)> Read(SemanticFact fact)
    {
        foreach (var pair in fact.ObjectAttributes ?? [])
        {
            if (string.IsNullOrEmpty(Target(fact)))
                throw new ArgumentException("Semantic attributes require a named node target.");
            if (!Types.TryGetValue(pair.Item1, out var types) || types.Length != 1)
                throw new ArgumentException($"Unknown or ambiguous semantic attribute '{pair.Item1}'.");
            string type = types[0].ToLowerInvariant();
            string value = pair.Item2 ?? throw new ArgumentException($"Null value for semantic attribute '{pair.Item1}'.");
            string datatype;
            try
            {
                switch (type)
                {
                    case "string": datatype = "string"; break;
                    case "bool": case "boolean":
                        value = XmlConvert.ToString(XmlConvert.ToBoolean(value.Trim().ToLowerInvariant())); datatype = "boolean"; break;
                    case "int": value = XmlConvert.ToString(XmlConvert.ToInt32(value)); datatype = "int"; break;
                    case "long": value = XmlConvert.ToString(XmlConvert.ToInt64(value)); datatype = "long"; break;
                    case "short": value = XmlConvert.ToString(XmlConvert.ToInt16(value)); datatype = "short"; break;
                    case "byte": value = XmlConvert.ToString(XmlConvert.ToSByte(value)); datatype = "byte"; break;
                    case "uint": value = XmlConvert.ToString(XmlConvert.ToUInt32(value)); datatype = "unsignedInt"; break;
                    case "ulong": value = XmlConvert.ToString(XmlConvert.ToUInt64(value)); datatype = "unsignedLong"; break;
                    case "double":
                        double number = XmlConvert.ToDouble(value);
                        if (!double.IsFinite(number)) throw new FormatException("Expected a finite number.");
                        value = XmlConvert.ToString(number); datatype = "double"; break;
                    case "float":
                        float single = XmlConvert.ToSingle(value);
                        if (!float.IsFinite(single)) throw new FormatException("Expected a finite number.");
                        value = XmlConvert.ToString(single); datatype = "float"; break;
                    case "decimal": value = XmlConvert.ToString(XmlConvert.ToDecimal(value)); datatype = "decimal"; break;
                    case "datetime":
                        value = XmlConvert.ToString(XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind), XmlDateTimeSerializationMode.RoundtripKind);
                        datatype = "dateTime"; break;
                    default: throw new NotSupportedException($"Semantic attribute '{pair.Item1}' has unsupported type '{types[0]}'.");
                }
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                throw new ArgumentException($"Invalid {types[0]} value for semantic attribute '{pair.Item1}'.", ex);
            }
            yield return (pair.Item1, value, datatype);
        }
    }

    internal static IList<Field>? Fields(string node, IEnumerable<SemanticFact> facts)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var fact in facts.Where(f => Target(f) == node))
        foreach (var attribute in Read(fact))
        {
            if (fields.TryGetValue(attribute.Name, out var previous) && previous != attribute.Value)
                throw new ArgumentException($"Conflicting values for semantic attribute '{attribute.Name}' on '{node}'.");
            fields[attribute.Name] = attribute.Value;
        }
        return fields.Count == 0 ? null : fields.Select(f => new Field { FieldName = f.Key, FieldValue = f.Value }).ToList();
    }

    internal static void Validate(IEnumerable<SemanticFact> facts)
    {
        var list = facts.ToList();
        foreach (var node in list.Where(f => f.ObjectAttributes?.Length > 0).Select(Target).Distinct())
            Fields(node ?? throw new ArgumentException("Semantic attributes require a named target."), list);
    }

    internal static string Query(SemanticFact fact)
    {
        var text = new StringBuilder();
        foreach (var attribute in Read(fact))
            text.Append('\t').Append(Target(fact)).Append(" ddhub:").Append(attribute.Name)
                .Append(" \"").Append(Escape(attribute.Value)).Append("\"^^<http://www.w3.org/2001/XMLSchema#")
                .Append(attribute.Datatype).Append("> .\n");
        return text.ToString();
    }

    private static string Escape(string value)
    {
        var text = new StringBuilder();
        foreach (char c in value)
            text.Append(c switch
            {
                '\\' => "\\\\", '"' => "\\\"", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t",
                '\b' => "\\b", '\f' => "\\f",
                _ when char.IsControl(c) => "\\u" + ((int)c).ToString("X4", CultureInfo.InvariantCulture),
                _ => c.ToString()
            });
        return text.ToString();
    }
}
