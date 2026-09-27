using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DynamicProperty.SourceGen.Tests;

internal static class GeneratorTestHelper
{
    public static GeneratorTestResult Run(
        string userSource)
    {
        var parseOptions =
            CSharpParseOptions.Default
                .WithLanguageVersion(
                    LanguageVersion.Latest);

        var userSyntaxTree =
            CSharpSyntaxTree.ParseText(
                userSource,
                parseOptions);

        var runtimeSyntaxTree =
            CSharpSyntaxTree.ParseText(
                RuntimeStubs,
                parseOptions);

        var compilation =
            CSharpCompilation.Create(
                assemblyName: "GeneratorTests",
                syntaxTrees: new[]
                {
                    userSyntaxTree,
                    runtimeSyntaxTree
                },
                references: GetFrameworkReferences(),
                options:
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary));

        ISourceGenerator generator =
            new PropertySetExtensionsGenerator();

        GeneratorDriver driver =
            CSharpGeneratorDriver.Create(
                generators: new[]
                {
                    generator
                },
                parseOptions: parseOptions);

        driver =
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        return new GeneratorTestResult(
            driver.GetRunResult(),
            outputCompilation,
            diagnostics);
    }

    private static MetadataReference[] GetFrameworkReferences()
    {
        string trustedAssemblies =
            (string)AppContext.GetData(
                "TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trustedAssemblies
            .Split(Path.PathSeparator)
            .Select(path =>
                MetadataReference.CreateFromFile(path))
            .ToArray();
    }

    private const string RuntimeStubs = """
using System;

namespace DynamicProperty
{
    public class PropertySet
    {
        public bool TryGetInt(int id, out int value)
        {
            value = default;
            return false;
        }

        public void SetInt(int id, int value) { }

        public bool TryGetFloat(int id, out float value)
        {
            value = default;
            return false;
        }

        public void SetFloat(int id, float value) { }

        public bool TryGetBool(int id, out bool value)
        {
            value = default;
            return false;
        }

        public void SetBool(int id, bool value) { }

        public bool TryGetLong(int id, out long value)
        {
            value = default;
            return false;
        }

        public void SetLong(int id, long value) { }

        public bool TryGetDouble(int id, out double value)
        {
            value = default;
            return false;
        }

        public void SetDouble(int id, double value) { }

        public bool TryGetUtcDateTime(
            int id,
            out DateTime value)
        {
            value = default;
            return false;
        }

        public void SetUtcDateTime(
            int id,
            DateTime value) { }

        public bool TryGetTimeSpan(
            int id,
            out TimeSpan value)
        {
            value = default;
            return false;
        }

        public void SetTimeSpan(
            int id,
            TimeSpan value) { }

        public bool TryGetEnum<T>(
            int id,
            out T value)
        {
            value = default!;
            return false;
        }

        public void SetEnum<T>(
            int id,
            T value) { }

        public bool ContainsAny(int id)
        {
            return false;
        }

        public bool Remove(int id)
        {
            return false;
        }

        public bool HasEnumFlag<T>(
            int id,
            T value)
        {
            return false;
        }
    }
}

namespace DynamicProperty.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PropertyTypeAttribute : Attribute
    {
        public PropertyTypeAttribute(Type type) { }

        public PropertyTypeAttribute(
            Type type,
            int initialValue) { }

        public PropertyTypeAttribute(
            Type type,
            bool initialValue) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(string name) { }
    }

    public enum PropertyComponent
    {
        X,
        Y,
        Z,
        W,
        R,
        G,
        B,
        A
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GroupComponentAttribute
        : Attribute
    {
        public GroupComponentAttribute(
            PropertyComponent component) { }
    }
}

namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(
            float x,
            float y,
            float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(
            float x,
            float y,
            float z,
            float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(
            float r,
            float g,
            float b,
            float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
""";
}
