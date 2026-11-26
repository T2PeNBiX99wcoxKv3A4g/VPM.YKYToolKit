using System;
using io.github.ykysnk.ykyToolkit;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace io.github.ykysnk.ykyToolkit.Editor
{
    /// <summary>
    ///     Reads and writes the per-object transform settings. The values are kept in memory while the inspector is
    ///     open and written to a component on the next editor tick: adding a component rebuilds the inspector, which
    ///     would otherwise interrupt a slider drag. A component is only created when a value is non-default.
    /// </summary>
    internal sealed class EnhancedTransformSettingsAccessor
    {
        private const string UndoName = "Transform Settings";

        private readonly Transform _target;
        private EnhancedTransformSettings? _settings;

        private bool _initialized;
        private bool _flushScheduled;
        private bool _locked;
        private int _position;
        private int _rotation;
        private int _scale;

        private EnhancedTransformSettingsAccessor(Transform target)
        {
            _target = target;
            _settings = target.GetComponent<EnhancedTransformSettings>();
        }

        internal static EnhancedTransformSettingsAccessor Get(Transform target) => new(target);

        public int positionDecimalPrecision
        {
            get
            {
                Initialize();
                return _position;
            }
            set
            {
                Initialize();
                _position = value;
                ScheduleFlush();
            }
        }

        public int rotationDecimalPrecision
        {
            get
            {
                Initialize();
                return _rotation;
            }
            set
            {
                Initialize();
                _rotation = value;
                ScheduleFlush();
            }
        }

        public int scaleDecimalPrecision
        {
            get
            {
                Initialize();
                return _scale;
            }
            set
            {
                Initialize();
                _scale = value;
                ScheduleFlush();
            }
        }

        public bool lockTransform
        {
            get
            {
                Initialize();
                return _locked;
            }
            set
            {
                Initialize();
                _locked = value;
                ScheduleFlush();
            }
        }

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _position = _settings?.positionDecimalPrecision ?? -1;
            _rotation = _settings?.rotationDecimalPrecision ?? -1;
            _scale = _settings?.scaleDecimalPrecision ?? -1;
            _locked = _settings?.lockTransform ?? false;
        }

        private void ScheduleFlush()
        {
            if (_flushScheduled) return;
            _flushScheduled = true;
            EditorApplication.delayCall += Flush;
        }

        private void Flush()
        {
            _flushScheduled = false;
            if (_target == null) return;

            _settings ??= _target.GetComponent<EnhancedTransformSettings>();

            // Everything is back to the default: nothing has to live on the object.
            if (_position == -1 && _rotation == -1 && _scale == -1 && !_locked)
            {
                if (_settings == null) return;
                Undo.DestroyObjectImmediate(_settings);
                _settings = null;
                return;
            }

            if (_settings == null)
            {
                _settings = Undo.AddComponent<EnhancedTransformSettings>(_target.gameObject);
                _settings.hideFlags = HideFlags.DontSaveInBuild;
                MoveBelowTransform(_settings);
            }

            Undo.RecordObject(_settings, UndoName);
            _settings.positionDecimalPrecision = _position;
            _settings.rotationDecimalPrecision = _rotation;
            _settings.scaleDecimalPrecision = _scale;
            _settings.lockTransform = _locked;
            EditorUtility.SetDirty(_settings);
        }

        private static void MoveBelowTransform(EnhancedTransformSettings settings)
        {
            // The Transform is always first; move up until the component sits directly under it.
            for (var i = 0; i < 64; i++)
                if (!ComponentUtility.MoveComponentUp(settings))
                    break;
        }
    }
}
