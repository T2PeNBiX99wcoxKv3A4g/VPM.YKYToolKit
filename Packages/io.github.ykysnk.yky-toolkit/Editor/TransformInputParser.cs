using io.github.ykysnk.utils.Extensions;
using JetBrains.Annotations;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    [PublicAPI]
    public enum TransformInputMode
    {
        None,

        // Basic
        Absolute, // =x, E(x)
        Additive, // +x, A(x)
        Multiply, // *x, M(x)
        Division, // /x, D(x)

        // Unity-like
        Linear, // L(start, step)
        Random, // R(min, max)
        Interpolate, // I(start, end)
        InterpolateRev, // i(start, end)

        // Extended
        Clamp, // LL(min, max)
        Mirror, // RR(center, step)
        Step, // TT(start, count)
        PingPong, // P(start, end)
        Distance, // NN(origin, step)
        Angle, // AA(centerAngle, step)
        Noise // N(scale, min, max)
    }

    [PublicAPI]
    public readonly struct TransformInputParseResult
    {
        public readonly TransformInputMode Mode;
        public readonly float A, B, C;

        public bool Success => Mode != TransformInputMode.None;

        public TransformInputParseResult(TransformInputMode mode, float a, float b = 0, float c = 0)
        {
            Mode = mode;
            A = a;
            B = b;
            C = c;
        }
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
                // Basic
                _ when s.StartsWith("=") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("E(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new(TransformInputMode.Absolute, v),

                _ when s.StartsWith("+") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("A(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new(TransformInputMode.Additive, v),

                _ when s.StartsWith("*") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("M(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new(TransformInputMode.Multiply, v),

                _ when s.StartsWith("/") && float.TryParse(s[1..], out var v) ||
                       s.StartsWith("D(") && float.TryParse(s.MiddlePath('(', ')'), out v)
                    => new(TransformInputMode.Division, v),

                // Unity-like
                _ when TryParseTwo("L", s, out var a, out var b)
                    => new(TransformInputMode.Linear, a, b),

                _ when TryParseTwo("R", s, out var a, out var b)
                    => new(TransformInputMode.Random, a, b),

                _ when TryParseTwo("I", s, out var a, out var b)
                    => new(TransformInputMode.Interpolate, a, b),

                _ when TryParseTwo("i", s, out var a, out var b)
                    => new(TransformInputMode.InterpolateRev, a, b),

                // Extended
                _ when TryParseTwo("LL", s, out var a, out var b)
                    => new(TransformInputMode.Clamp, a, b),

                _ when TryParseTwo("RR", s, out var a, out var b)
                    => new(TransformInputMode.Mirror, a, b),

                _ when TryParseTwo("TT", s, out var a, out var b)
                    => new(TransformInputMode.Step, a, b),

                _ when TryParseTwo("P", s, out var a, out var b)
                    => new(TransformInputMode.PingPong, a, b),

                _ when TryParseTwo("NN", s, out var a, out var b)
                    => new(TransformInputMode.Distance, a, b),

                _ when TryParseTwo("AA", s, out var a, out var b)
                    => new(TransformInputMode.Angle, a, b),

                _ when TryParseThree("N", s, out var a, out var b, out var c)
                    => new(TransformInputMode.Noise, a, b, c),

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

        private static bool TryParseThree(string prefix, string s, out float a, out float b, out float c)
        {
            a = b = c = 0;

            if (!s.StartsWith(prefix + "(") || !s.EndsWith(")"))
                return false;

            var inner = s.MiddlePath('(', ')')!;
            var parts = inner.Split(',');

            if (parts.Length != 3)
                return false;

            return float.TryParse(parts[0], out a) &&
                   float.TryParse(parts[1], out b) &&
                   float.TryParse(parts[2], out c);
        }
    }
}