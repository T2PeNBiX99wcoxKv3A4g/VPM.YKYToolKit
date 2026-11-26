using io.github.ykysnk.Localization.Editor;
using io.github.ykysnk.ykyToolkit.Editor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    public partial class EnhancedTransformInspector
    {
        private enum HelpGroup
        {
            Basic,
            Order,
            Random
        }

        private readonly struct HelpEntry
        {
            public readonly HelpGroup Group;
            public readonly string Syntax;
            public readonly string Alt;
            public readonly string DescriptionKey;
            public readonly string Sample;
            public readonly string Signature;

            public HelpEntry(HelpGroup group, string syntax, string alt, string descriptionKey, string sample,
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
        private static readonly HelpEntry[] HelpEntries =
        {
            new(HelpGroup.Basic, "=x", "E(x)", "label.enhanced_transform_inspector.help_expr_absolute", "=0"),
            new(HelpGroup.Basic, "+x", "A(x)", "label.enhanced_transform_inspector.help_expr_additive", "+15"),
            new(HelpGroup.Basic, "*x", "M(x)", "label.enhanced_transform_inspector.help_expr_multiply", "*2"),
            new(HelpGroup.Basic, "/x", "D(x)", "label.enhanced_transform_inspector.help_expr_division", "/2"),

            new(HelpGroup.Order, "L(0, 1)", "", "label.enhanced_transform_inspector.help_expr_linear", "L(0, 1)",
                "L(start, step)"),
            new(HelpGroup.Order, "I(0, 10)", "", "label.enhanced_transform_inspector.help_expr_interpolate", "I(0, 10)",
                "I(start, end)"),
            new(HelpGroup.Order, "i(0, 10)", "", "label.enhanced_transform_inspector.help_expr_interpolate_reverse",
                "i(0, 10)", "i(start, end)"),
            new(HelpGroup.Order, "RR(0, 1)", "", "label.enhanced_transform_inspector.help_expr_mirror", "RR(0, 1)",
                "RR(center, step)"),
            new(HelpGroup.Order, "TT(0, 3)", "", "label.enhanced_transform_inspector.help_expr_step", "TT(0, 3)",
                "TT(start, count)"),
            new(HelpGroup.Order, "P(0, 1)", "", "label.enhanced_transform_inspector.help_expr_pingpong", "P(0, 1)",
                "P(start, end)"),
            new(HelpGroup.Order, "NN(0, 1)", "", "label.enhanced_transform_inspector.help_expr_distance", "NN(0, 1)",
                "NN(origin, step)"),
            new(HelpGroup.Order, "AA(0, 90)", "", "label.enhanced_transform_inspector.help_expr_angle", "AA(0, 90)",
                "AA(centerAngle, step)"),

            new(HelpGroup.Random, "R(0, 10)", "", "label.enhanced_transform_inspector.help_expr_random", "R(0, 10)",
                "R(min, max)"),
            new(HelpGroup.Random, "LL(0, 10)", "", "label.enhanced_transform_inspector.help_expr_clamp", "LL(0, 10)",
                "LL(min, max)"),
            new(HelpGroup.Random, "N(0.5, 0, 1)", "", "label.enhanced_transform_inspector.help_expr_noise",
                "N(0.5, 0, 1)", "N(scale, min, max)")
        };

        private static string HelpGroupKey(HelpGroup group) => group switch
        {
            HelpGroup.Basic => "label.enhanced_transform_inspector.help_group_basic",
            HelpGroup.Order => "label.enhanced_transform_inspector.help_group_order",
            _ => "label.enhanced_transform_inspector.help_group_random"
        };

        /// <summary>
        ///     Builds the cheat sheet from <see cref="HelpEntries" /> so every expression is one row and a new
        ///     expression only has to be added in one place. Rows are copied to the clipboard when clicked.
        /// </summary>
        private static void BuildHelp(VisualElement container)
        {
            container.Clear();

            var hint = new Label("label.enhanced_transform_inspector.help_hint");
            hint.AddToClassList("transform-help__hint");
            container.Add(hint);

            foreach (var group in new[] { HelpGroup.Basic, HelpGroup.Order, HelpGroup.Random })
            {
                var section = new VisualElement();
                section.AddToClassList("transform-help__group");

                var title = new Label(HelpGroupKey(group));
                title.AddToClassList("transform-help__group-title");
                section.Add(title);

                foreach (var entry in HelpEntries)
                    if (entry.Group == group)
                        section.Add(CreateHelpRow(entry));

                container.Add(section);
            }

            var copyHint = new Label("label.enhanced_transform_inspector.help_copy_hint");
            copyHint.AddToClassList("transform-help__copy-hint");
            container.Add(copyHint);

            // The rows are created after the tree's localization pass, so localize them here. It also registers
            // them, which keeps the text in sync when the language changes.
            container.UILocalize(false);
        }

        private static VisualElement CreateHelpRow(HelpEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("transform-help__row");
            row.tooltip = string.IsNullOrEmpty(entry.Signature) ? entry.Sample : entry.Signature;

            var syntax = new Label(entry.Syntax);
            syntax.AddToClassList("transform-help__syntax");
            row.Add(syntax);

            var description = new Label(entry.DescriptionKey);
            description.AddToClassList("transform-help__description");
            row.Add(description);

            if (!string.IsNullOrEmpty(entry.Alt))
            {
                var alt = new Label(entry.Alt);
                alt.AddToClassList("transform-help__alt");
                row.Add(alt);
            }

            var copied = false;

            row.RegisterCallback<ClickEvent>(_ =>
            {
                if (copied) return;
                copied = true;

                EditorGUIUtility.systemCopyBuffer = entry.Sample;

                var original = description.text;
                description.text = "label.enhanced_transform_inspector.help_copied".S();
                description.AddToClassList("transform-help__copied");

                row.schedule.Execute(() =>
                {
                    description.text = original;
                    description.RemoveFromClassList("transform-help__copied");
                    copied = false;
                }).StartingIn(900);
            });

            return row;
        }

        /// <summary>Shows the expression hint when the field is hovered.</summary>
        private static void AttachExpressionHint(Vector3FieldExtra field)
        {
            // The hint is its own localization key, which is what the 'localize-tooltip' class tells UILocalize
            // when the field's label is translated below.
            field.AddToClassList("localize-tooltip");
            field.tooltip = "label.enhanced_transform_inspector.help_hint";
        }
    }
}
