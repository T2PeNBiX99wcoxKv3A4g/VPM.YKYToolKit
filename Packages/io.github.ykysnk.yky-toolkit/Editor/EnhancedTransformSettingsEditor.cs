using io.github.ykysnk.Localization.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     The component is a storage marker; its values are edited from the Transform inspector, so the component
    ///     inspector only explains where to change them.
    /// </summary>
    [CustomEditor(typeof(EnhancedTransformSettings))]
    public class EnhancedTransformSettingsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var hint = new Label("label.enhanced_transform_settings.info");
            hint.AddToClassList("yky-hint");
            root.Add(hint);

            root.UILocalize(false);
            return root;
        }
    }
}
