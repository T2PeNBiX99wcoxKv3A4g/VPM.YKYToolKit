using io.github.ykysnk.utils.Extensions;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [PublicAPI]
    public enum TransformInputMode
    {
        None,
        Absolute, // =x, E(x)
        Additive, // +x, A(x)
        Multiply, // *x, M(x)
        Division, // /x, D(x)
        Linear, // L(start, step)
        Random, // R(min, max)
        Interpolate, // I(start, end)
        InterpolateRev // i(start, end)
    }

    [PublicAPI]
    public struct TransformInputParseResult
    {
        public TransformInputMode Mode;
        public float A, B, C;

        public bool Success => Mode != TransformInputMode.None;
    }

    [PublicAPI]
    public static class TransformInputParser
    {
        public static TransformInputParseResult Parse(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return default;

            s = s!.Trim();

            return s switch
            {
                _ when s.StartsWith("=") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("E(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new()
                    {
                        Mode = TransformInputMode.Absolute,
                        A = v
                    },

                _ when s.StartsWith("+") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("A(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new()
                    {
                        Mode = TransformInputMode.Additive,
                        A = v
                    },

                _ when s.StartsWith("*") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("M(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new()
                    {
                        Mode = TransformInputMode.Multiply,
                        A = v
                    },

                _ when s.StartsWith("/") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("D(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new()
                    {
                        Mode = TransformInputMode.Division,
                        A = v
                    },

                _ when TryParseTwo("L", s, out var a, out var b)
                    => new()
                    {
                        Mode = TransformInputMode.Linear,
                        A = a,
                        B = b
                    },

                _ when TryParseTwo("R", s, out var a, out var b)
                    => new()
                    {
                        Mode = TransformInputMode.Random,
                        A = a,
                        B = b
                    },

                _ when TryParseTwo("I", s, out var a, out var b)
                    => new()
                    {
                        Mode = TransformInputMode.Interpolate,
                        A = a,
                        B = b
                    },

                _ when TryParseTwo("i", s, out var a, out var b)
                    => new()
                    {
                        Mode = TransformInputMode.InterpolateRev,
                        A = a,
                        B = b
                    },

                _ => default
            };
        }

        private static bool TryParseTwo(string prefix, string s, out float a, out float b)
        {
            a = b = 0;

            if (!s.StartsWith(prefix + "(") || !s.EndsWith(")"))
                return false;

            var inner = s.MiddlePath('(', ')')!;
            var parts = inner.Split(',');

            if (parts.Length != 2)
                return false;

            return float.TryParse(parts[0], out a) &&
                   float.TryParse(parts[1], out b);
        }
    }
}