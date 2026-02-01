using System.Diagnostics.CodeAnalysis;
using io.github.ykysnk.utils.Editor;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor.UIElements
{
    public class Vector3FieldExtra : Vector3Field
    {
        [PublicAPI] internal static readonly StyleBackground LinkedIcon =
            new(EditorGUIUtils.IconTexture("linked") as Texture2D);

        [PublicAPI] internal static readonly StyleBackground RandomIcon =
            new(EditorGUIUtils.IconTexture("preaudioloopoff") as Texture2D);

        [PublicAPI] internal static readonly StyleBackground ResetIcon =
            new(EditorGUIUtils.IconTexture("refresh") as Texture2D);

        [PublicAPI] internal static readonly StyleBackground UnlinkedIcon =
            new(EditorGUIUtils.IconTexture("unlinked") as Texture2D);

        private bool _showLinkButton;

        public Vector3FieldExtra()
        {
            ExtraSlot = new()
            {
                name = "extraSlot"
            };

            ExtraSlot.AddToClassList("row");
            ExtraSlot.AddToClassList("extra-slot");

            Insert(0, ExtraSlot);

            LinkButton = new()
            {
                name = "linkButton",
                style =
                {
                    backgroundImage = UnlinkedIcon
                }
            };

            ExtraSlot.Add(LinkButton);

            RandomButton = new()
            {
                name = "randomButton",
                style =
                {
                    backgroundImage = RandomIcon
                }
            };

            ExtraSlot.Add(RandomButton);

            ResetButton = new()
            {
                name = "resetButton",
                style =
                {
                    backgroundImage = ResetIcon
                }
            };

            ExtraSlot.Add(ResetButton);
        }

        [PublicAPI] public VisualElement ExtraSlot { get; }
        [PublicAPI] public IconButton ResetButton { get; }
        [PublicAPI] public IconButton RandomButton { get; }
        [PublicAPI] public IconButton LinkButton { get; }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool showLinkButton
        {
            get => _showLinkButton;
            set
            {
                _showLinkButton = value;
                LinkButton.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string resetButtonTooltip
        {
            get => ResetButton.tooltip;
            set => ResetButton.tooltip = value;
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string randomButtonTooltip
        {
            get => RandomButton.tooltip;
            set => RandomButton.tooltip = value;
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string linkedIconTooltip
        {
            get => LinkButton.tooltip;
            set => LinkButton.tooltip = value;
        }

        [PublicAPI]
        public void SetLinked(bool linked)
        {
            LinkButton.style.backgroundImage = linked ? LinkedIcon : UnlinkedIcon;
            LinkButton.tooltip = linked
                ? "label.enhanced_transform_inspector.scale_link_disable".S()
                : "label.enhanced_transform_inspector.scale_link_enable".S();
        }

        public new class UxmlFactory : UxmlFactory<Vector3FieldExtra, UxmlTraits>
        {
        }

        public new class UxmlTraits : Vector3Field.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _linkedIconTooltipAttr = new()
            {
                name = "linked-icon-tooltip"
            };

            private readonly UxmlStringAttributeDescription _randomButtonTooltipAttr = new()
            {
                name = "random-button-tooltip"
            };

            private readonly UxmlStringAttributeDescription _resetButtonTooltipAttr = new()
            {
                name = "reset-button-tooltip"
            };

            private readonly UxmlBoolAttributeDescription _showLinkButtonAttr = new()
            {
                name = "show-link-button"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var field = (Vector3FieldExtra)ve;
                field.showLinkButton = _showLinkButtonAttr.GetValueFromBag(bag, cc);
                field.resetButtonTooltip = _resetButtonTooltipAttr.GetValueFromBag(bag, cc);
                field.randomButtonTooltip = _randomButtonTooltipAttr.GetValueFromBag(bag, cc);
                field.linkedIconTooltip = _linkedIconTooltipAttr.GetValueFromBag(bag, cc);
            }
        }
    }
}