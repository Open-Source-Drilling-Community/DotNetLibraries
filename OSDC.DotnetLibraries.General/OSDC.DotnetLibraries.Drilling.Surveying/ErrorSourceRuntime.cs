using System.Reflection;

namespace OSDC.DotnetLibraries.Drilling.Surveying;

/// <summary>
/// Restores the executable weighting functions that are intentionally excluded from JSON and OpenAPI contracts.
/// The serialized error code and parameters remain authoritative; hydration changes runtime delegates only.
/// </summary>
internal static class ErrorSourceRuntime
{
    private static readonly Lazy<IReadOnlyDictionary<ErrorCode, ErrorSource>> Templates =
        new(CreateTemplates, LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool TryHydrate(ErrorSource? source, out string? error)
    {
        if (source == null)
        {
            error = "Error source is required.";
            return false;
        }
        if (!Templates.Value.TryGetValue(source.ErrorCode, out ErrorSource? template))
        {
            error = $"No executable weighting functions are registered for error source {source.ErrorCode}.";
            return false;
        }

        source.WeightingFunctionMD = template.WeightingFunctionMD;
        source.WeightingFunctionIncl = template.WeightingFunctionIncl;
        source.WeightingFunctionAzim = template.WeightingFunctionAzim;
        source.VerticalHoleWeightingFunctionNorth = template.VerticalHoleWeightingFunctionNorth;
        source.VerticalHoleWeightingFunctionEast = template.VerticalHoleWeightingFunctionEast;
        source.VerticalHoleWeightingFunctionVertical = template.VerticalHoleWeightingFunctionVertical;
        source.WeightingFunctionDepthGyro = template.WeightingFunctionDepthGyro;

        if (source.WeightingFunctionMD == null || source.WeightingFunctionIncl == null ||
            source.WeightingFunctionAzim == null)
        {
            error = $"Incomplete executable weighting functions are registered for error source {source.ErrorCode}.";
            return false;
        }

        error = null;
        return true;
    }

    private static IReadOnlyDictionary<ErrorCode, ErrorSource> CreateTemplates()
    {
        Dictionary<ErrorCode, ErrorSource> result = [];
        foreach (MethodInfo method in typeof(ErrorSourceFactory).GetMethods(BindingFlags.Public | BindingFlags.Static)
                     .Where(method => method.Name.StartsWith("Create_", StringComparison.Ordinal) &&
                                      method.ReturnType == typeof(ErrorSource) &&
                                      method.GetParameters().All(parameter => parameter.HasDefaultValue)))
        {
            object?[] arguments = method.GetParameters().Select(_ => Type.Missing).ToArray();
            if (method.Invoke(null, arguments) is ErrorSource template)
            {
                if (!result.TryAdd(template.ErrorCode, template))
                {
                    throw new InvalidOperationException(
                        $"Multiple error-source factories are registered for {template.ErrorCode}.");
                }
            }
        }
        return result;
    }
}
