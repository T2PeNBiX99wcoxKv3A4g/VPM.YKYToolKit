using System;
using io.github.ykysnk.Localization.Editor;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     The empty-state label of a <see cref="ListView" /> is created by Unity with the English text "List is empty".
    ///     It cannot be reached from the UXML, so the toolkit replaces its text with a localized one.
    /// </summary>
    [PublicAPI]
    internal static class ListViewExtensions
    {
        private const string EmptyLabelClass = "unity-list-view__empty-label";
        private const string EmptyLabelKey = "label.list_empty";
        private const string LocalizedClass = "yky-localized-empty-label";

        internal static void LocalizeEmptyLabel(this ListView list)
        {
            Apply(list);

            // The label is created when the list first lays out (and again if the list is recycled), which is later
            // than the window's own creation, so keep looking until it exists.
            list.schedule.Execute(() => Apply(list)).Every(500);
        }

        /// <summary>
        ///     Adds an icon button at the left end of the list's built-in footer, so it lines up with the add/remove
        ///     buttons without a wrapper or absolute positioning.
        /// </summary>
        internal static void AddFooterReset(this ListView list, string name, string tooltip, Action onClick)
        {
            IVisualElementScheduledItem? item = null;
            item = list.schedule.Execute(() =>
            {
                if (list.Q<Button>(name) != null)
                {
                    item?.Pause();
                    return;
                }

                var footer = list.Q<VisualElement>(className: "unity-list-view__footer");
                if (footer == null) return;

                var button = new Button
                {
                    name = name,
                    tooltip = tooltip
                };
                button.AddToClassList("yky-icon-button");
                button.AddToClassList("yky-button-icon--reset");
                button.AddToClassList("yky-list-reset");
                button.clicked += onClick;

                // The footer lays its children out right to left. A spacer between the built-in buttons and this one
                // pins it to the left end while add/remove stay on the right.
                var spacer = new VisualElement();
                spacer.AddToClassList("yky-footer-spacer");
                footer.Add(spacer);
                footer.Add(button);
                item?.Pause();
            }).Every(200);
        }

        private static bool Apply(ListView list)
        {
            if (list.Q<Label>(className: EmptyLabelClass) is not { } label) return false;
            if (label.ClassListContains(LocalizedClass)) return true;

            label.AddToClassList(LocalizedClass);
            label.AddToClassList("yky-list-empty");
            label.text = EmptyLabelKey.S();

            var registration = EmptyLabelKey.Register((text, _) => label.text = text);
            label.RegisterCallback<DetachFromPanelEvent>(_ => registration.Dispose());

            return true;
        }
    }
}