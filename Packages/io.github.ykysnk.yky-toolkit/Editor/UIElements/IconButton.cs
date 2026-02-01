using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor.UIElements
{
    public class IconButton : VisualElement
    {
        public IconButton()
        {
            AddToClassList("icon-button");
            AddToClassList("localize-tooltip");
            RegisterCallback<ClickEvent>(_ =>
            {
                ExtrasAnimations.ForEach(action => action());
                AddToClassList("flash");
                clicked?.Invoke();
                schedule.Execute(() => RemoveFromClassList("flash")).ExecuteLater(120);
            });

            pickingMode = PickingMode.Position;
            transform.scale = Vector3.one;
        }

        private List<Action> ExtrasAnimations { get; } = new();

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public event Action? clicked;

        [PublicAPI]
        public void AddExtraAnimation(Action action) => ExtrasAnimations.Add(action);

        public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}