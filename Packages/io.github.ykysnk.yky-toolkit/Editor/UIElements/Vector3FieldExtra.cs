using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor.UIElements
{
    public class Vector3FieldExtra : Vector3Field
    {
        [PublicAPI] internal static readonly StyleBackground LinkedIcon =
            new(EditorGUIUtility.FindTexture("linked"));

        [PublicAPI] internal static readonly StyleBackground RandomIcon =
            new(EditorGUIUtility.FindTexture("preaudioloopoff"));

        [PublicAPI] internal static readonly StyleBackground ResetIcon =
            new(EditorGUIUtility.FindTexture("refresh"));

        [PublicAPI] internal static readonly StyleBackground UnlinkedIcon =
            new(EditorGUIUtility.FindTexture("unlinked"));

        [PublicAPI] internal static readonly StyleBackground CopyIcon =
            new(AssetDatabase.LoadAssetAtPath<Texture2D>(
                AssetDatabase.GUIDToAssetPath("f8379f706ae3d4841838ff5cc924da0a")));

        [PublicAPI] internal static readonly StyleBackground PasteIcon =
            new(AssetDatabase.LoadAssetAtPath<Texture2D>(
                AssetDatabase.GUIDToAssetPath("3d32b439cc5c09f4f93312cf317a8019")));

        private bool _showCopyPasteButtons;

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
            LinkButton.RegisterValueChangedCallback(_ => SetLinked(LinkButton.value));

            ExtraSlot.Add(LinkButton);

            CopyButton = new()
            {
                name = "copyButton",
                style =
                {
                    backgroundImage = CopyIcon
                }
            };

            ExtraSlot.Add(CopyButton);

            PasteButton = new()
            {
                name = "pasteButton",
                style =
                {
                    backgroundImage = PasteIcon
                }
            };

            ExtraSlot.Add(PasteButton);

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
        [PublicAPI] public IconButton CopyButton { get; }
        [PublicAPI] public IconButton PasteButton { get; }

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
        public bool showCopyPasteButtons
        {
            get => _showCopyPasteButtons;
            set
            {
                _showCopyPasteButtons = value;
                CopyButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                PasteButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
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
        public string linkedButtonTooltip
        {
            get => LinkButton.tooltip;
            set => LinkButton.tooltip = value;
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string copyButtonTooltip
        {
            get => CopyButton.tooltip;
            set => CopyButton.tooltip = value;
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string pasteButtonTooltip
        {
            get => PasteButton.tooltip;
            set => PasteButton.tooltip = value;
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string linkedButtonBindingPath
        {
            get => LinkButton.bindingPath;
            set => LinkButton.bindingPath = value;
        }

        private void SetLinked(bool linked)
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
            private readonly UxmlStringAttributeDescription _copyButtonTooltipAttr = new()
            {
                name = "copy-button-tooltip"
            };

            private readonly UxmlStringAttributeDescription _linkButtonBindingPathAttr = new()
            {
                name = "link-button-binding-path"
            };

            private readonly UxmlStringAttributeDescription _linkedIconTooltipAttr = new()
            {
                name = "linked-icon-tooltip"
            };

            private readonly UxmlStringAttributeDescription _pasteButtonTooltipAttr = new()
            {
                name = "paste-button-tooltip"
            };

            private readonly UxmlStringAttributeDescription _randomButtonTooltipAttr = new()
            {
                name = "random-button-tooltip"
            };

            private readonly UxmlStringAttributeDescription _resetButtonTooltipAttr = new()
            {
                name = "reset-button-tooltip"
            };

            private readonly UxmlBoolAttributeDescription _showCopyPasteButtonsAttr = new()
            {
                name = "show-copy-paste-buttons"
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
                field.showCopyPasteButtons = _showCopyPasteButtonsAttr.GetValueFromBag(bag, cc);
                field.resetButtonTooltip = _resetButtonTooltipAttr.GetValueFromBag(bag, cc);
                field.randomButtonTooltip = _randomButtonTooltipAttr.GetValueFromBag(bag, cc);
                field.linkedButtonTooltip = _linkedIconTooltipAttr.GetValueFromBag(bag, cc);
                field.linkedButtonBindingPath = _linkButtonBindingPathAttr.GetValueFromBag(bag, cc);
                field.copyButtonTooltip = _copyButtonTooltipAttr.GetValueFromBag(bag, cc);
                field.pasteButtonTooltip = _pasteButtonTooltipAttr.GetValueFromBag(bag, cc);
            }
        }
    }
}