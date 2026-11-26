using System;
using System.Linq;
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
        ///     Replaces the built-in add/remove footer with the toolkit footer: reset, + and - in the bottom-right
        ///     corner. Every list in the preferences page uses this, so the footer looks and behaves the same.
        /// </summary>
        internal static void AddFooterButtons(this ListView list, Action add, Action remove, Action reset,
            string resetTooltip)
        {
            if (list.parent == null) return;
            if (list.parent.Q<VisualElement>(className: "yky-list-footer") != null) return;

            list.showAddRemoveFooter = false;

            var footer = new VisualElement();
            footer.AddToClassList("yky-list-footer");

            var resetButton = new Button(reset) { tooltip = resetTooltip };
            resetButton.AddToClassList("yky-icon-button");
            resetButton.AddToClassList("yky-button-icon--reset");

            var addButton = new Button(add) { text = "+" };
            addButton.AddToClassList("yky-icon-button");

            var removeButton = new Button(remove) { text = "-" };
            removeButton.AddToClassList("yky-icon-button");

            footer.Add(resetButton);
            footer.Add(addButton);
            footer.Add(removeButton);

            list.parent.Insert(list.parent.IndexOf(list) + 1, footer);

            void UpdateRemoveButton() => removeButton.SetEnabled(list.selectedIndices.Any());
            list.selectionChanged += _ => UpdateRemoveButton();
            UpdateRemoveButton();
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