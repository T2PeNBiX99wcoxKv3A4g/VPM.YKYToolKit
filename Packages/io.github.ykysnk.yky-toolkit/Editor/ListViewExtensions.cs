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