using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor.UIElements
{
    public class IconButton : BindableElement, INotifyValueChanged<bool>
    {
        private bool _value;

        public IconButton()
        {
            AddToClassList("icon-button");
            AddToClassList("localize-tooltip");

            RegisterCallback<ClickEvent>(_ =>
            {
                ExtrasAnimations.ForEach(action => action());
                AddToClassList("flash");
                value = !value;
                clicked?.Invoke();
                schedule.Execute(() => RemoveFromClassList("flash")).ExecuteLater(120);
            });

            pickingMode = PickingMode.Position;
            transform.scale = Vector3.one;
        }

        private List<Action> ExtrasAnimations { get; } = new();

        public bool value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                var previous = _value;
                _value = value;

                using var evt = ChangeEvent<bool>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
                UpdateVisualState();
            }
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _value = newValue;
            UpdateVisualState();
        }

        [PublicAPI]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public event Action? clicked;

        [PublicAPI]
        public void AddExtraAnimation(Action action) => ExtrasAnimations.Add(action);

        private void UpdateVisualState()
        {
            EnableInClassList("on", _value);
            EnableInClassList("off", !_value);
        }

        public new class UxmlFactory : UxmlFactory<IconButton, UxmlTraits>
        {
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlBoolAttributeDescription _value = new()
            {
                name = "value"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var field = (IconButton)ve;
                field.SetValueWithoutNotify(_value.GetValueFromBag(bag, cc));
            }
        }
    }
}