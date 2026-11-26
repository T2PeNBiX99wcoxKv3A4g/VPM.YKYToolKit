namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     The expression list used by the typing suggestions and the help window. Keeping one table here means a new
    ///     expression is added in a single place and both places show it.
    /// </summary>
    internal static class TransformExpressionHelp
    {
        internal enum Group
        {
            Basic,
            Order,
            Random
        }

        internal readonly struct Entry
        {
            internal readonly Group Group;
            internal readonly string Syntax;
            internal readonly string Alt;
            internal readonly string DescriptionKey;
            internal readonly string Sample;
            internal readonly string Signature;

            internal Entry(Group group, string syntax, string alt, string descriptionKey, string sample,
                string signature = "")
            {
                Group = group;
                Syntax = syntax;
                Alt = alt;
                DescriptionKey = descriptionKey;
                Sample = sample;
                Signature = signature;
            }
        }

        // Keep in sync with TransformInputParser. The syntax column shows the short form; the alt column shows the
        // function spelling that is accepted as well. Samples are what a click puts on the clipboard.
        internal static readonly Entry[] Entries =
        {
            new(Group.Basic, "=x", "E(x)", "label.enhanced_transform_inspector.help_expr_absolute", "=0"),
            new(Group.Basic, "+x", "A(x)", "label.enhanced_transform_inspector.help_expr_additive", "+15"),
            new(Group.Basic, "*x", "M(x)", "label.enhanced_transform_inspector.help_expr_multiply", "*2"),
            new(Group.Basic, "/x", "D(x)", "label.enhanced_transform_inspector.help_expr_division", "/2"),

            new(Group.Order, "L(0, 1)", "", "label.enhanced_transform_inspector.help_expr_linear", "L(0, 1)",
                "L(start, step)"),
            new(Group.Order, "I(0, 10)", "", "label.enhanced_transform_inspector.help_expr_interpolate", "I(0, 10)",
                "I(start, end)"),
            new(Group.Order, "i(0, 10)", "", "label.enhanced_transform_inspector.help_expr_interpolate_reverse",
                "i(0, 10)", "i(start, end)"),
            new(Group.Order, "RR(0, 1)", "", "label.enhanced_transform_inspector.help_expr_mirror", "RR(0, 1)",
                "RR(center, step)"),
            new(Group.Order, "TT(0, 3)", "", "label.enhanced_transform_inspector.help_expr_step", "TT(0, 3)",
                "TT(start, count)"),
            new(Group.Order, "P(0, 1)", "", "label.enhanced_transform_inspector.help_expr_pingpong", "P(0, 1)",
                "P(start, end)"),
            new(Group.Order, "NN(0, 1)", "", "label.enhanced_transform_inspector.help_expr_distance", "NN(0, 1)",
                "NN(origin, step)"),
            new(Group.Order, "AA(0, 90)", "", "label.enhanced_transform_inspector.help_expr_angle", "AA(0, 90)",
                "AA(centerAngle, step)"),

            new(Group.Random, "R(0, 10)", "", "label.enhanced_transform_inspector.help_expr_random", "R(0, 10)",
                "R(min, max)"),
            new(Group.Random, "LL(0, 10)", "", "label.enhanced_transform_inspector.help_expr_clamp", "LL(0, 10)",
                "LL(min, max)"),
            new(Group.Random, "N(0.5, 0, 1)", "", "label.enhanced_transform_inspector.help_expr_noise",
                "N(0.5, 0, 1)", "N(scale, min, max)")
        };

        internal static string GroupKey(Group group) => group switch
        {
            Group.Basic => "label.enhanced_transform_inspector.help_group_basic",
            Group.Order => "label.enhanced_transform_inspector.help_group_order",
            _ => "label.enhanced_transform_inspector.help_group_random"
        };
    }
}
