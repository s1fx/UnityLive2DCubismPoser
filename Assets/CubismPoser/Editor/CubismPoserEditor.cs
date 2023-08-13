using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Live2D.Cubism.Core;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace SF.Cubism.Poser
{
    [CustomEditor(typeof(CubismPoser))]
    public class CubismPoserEditor : Editor
    {
        #region CubismProperty [private helper class]
        private class CubismProperty
        {
            public readonly CubismParameter Parameter;
            public readonly SerializedObject SerializedObject;
            public readonly SerializedProperty SerializedProperty;

            public CubismProperty(CubismParameter parameter, SerializedObject serializedObject, SerializedProperty serializedProperty)
            {
                Parameter = parameter;
                SerializedObject = serializedObject;
                SerializedProperty = serializedProperty;
            }
        }
        #endregion

        private const string PLUGIN_ROOT = "Assets/CubismPoser";
        private const string FORCE_UPDATE_BTN_LABEL = "Force Model Update";
        private const string GROUPS_LABEL = "Parameters Groups";
        private const string KEYFRAMES_BTN_LABEL = "Create Keyframes For Selected";
        private const string KEYFRAMES_NOTOGGLED_HEADER = "No parameters selected";
        private const string KEYFRAMES_NOTOGGLED_MSG = "You need to select parameters first in order to create keyframes.";
        private const string OK_LABEL = "OK";
        private const string RESET_ALL_BTN_LABEL = "Reset All";
        private const string RESET_SELECTED_BTN_LABEL = "Reset Selected";
        private const string TOGGLE_GROUP_BTN_LABEL = "Toggle Group";

        private static CubismPoserGroups _groups;
        private static int _selectedGroup;

        private readonly Dictionary<string, CubismProperty> _modelProperties = new();
        private readonly HashSet<string> _toggledProps = new();

        private CubismPoser _target;
        private string[] _groupsOptions;

        private bool _toggleAll;
        private bool _creatingKeyframes;
        private bool _initialized;
        private CancellationToken _token;

        private void Awake()
        {
            _target = target as CubismPoser;
            _token = _target.Token;

            if (_groups == null)
                _groups = AssetDatabase.LoadAssetAtPath<CubismPoserGroups>($"{PLUGIN_ROOT}/Prefabs/PoserGroups.prefab");

            var parameters = _target.GetComponentsInChildren<CubismParameter>();

            foreach (CubismParameter par in parameters)
            {
                var name = par.name;
                var obj = new SerializedObject(par);
                var prop = obj.FindProperty("Value");

                _modelProperties.Add(name, new CubismProperty(par, obj, prop));
            }

            if (_groups != null && _groups.Groups.Count > 0)
            {
                _groupsOptions = new string[_groups.Groups.Count];

                for (int i = 0; i < _groups.Groups.Count; i++)
                {
                    var name = _groups.Groups[i].Name.Trim();
                    _groupsOptions[i] = name;
                }

                _selectedGroup = 0;
            }
            else
            {
                _groupsOptions = Array.Empty<string>();
                _selectedGroup = -1;
            }

            _initialized = true;
        }

        public override void OnInspectorGUI()
        {
            if (!_initialized)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(_creatingKeyframes);
            serializedObject.Update();

            GUILayout.Space(10);
            bool toggleAll = GUILayout.Toggle(_toggleAll, "", GUILayout.Width(20));
            GUILayout.Space(5);

            #region Model Properties Toggles
            bool needsUpdate = false;

            foreach (var item in _modelProperties)
            {
                EditorGUILayout.BeginHorizontal();

                var name = item.Key;
                var cubismParameter = item.Value.Parameter;
                var serializedObj = item.Value.SerializedObject;
                var serializedProp = item.Value.SerializedProperty;

                var isToggled = _toggledProps.Contains(name);

                bool propToggle = GUILayout.Toggle(isToggled, "", GUILayout.Width(20));
                GUILayout.Space(5);

                if (isToggled)
                    GUI.contentColor = Color.yellow;

                serializedObj.Update();

                var initValue = serializedProp.floatValue;
                GUILayout.Label(name, GUILayout.Width(250));
                serializedProp.floatValue = EditorGUILayout.Slider("", serializedProp.floatValue, cubismParameter.MinimumValue, cubismParameter.MaximumValue);

                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();

                serializedObj.ApplyModifiedProperties();

                if (propToggle && !_toggledProps.Contains(name) || !propToggle && initValue != serializedProp.floatValue)
                    _toggledProps.Add(name);
                else if (!propToggle && _toggledProps.Contains(name))
                    _toggledProps.Remove(name);

                if (initValue != serializedProp.floatValue)
                    needsUpdate = true;
            }
            #endregion

            #region Toggle All
            if (toggleAll != _toggleAll)
            {
                _toggledProps.Clear();

                if (toggleAll)
                    _toggledProps.UnionWith(_modelProperties.Keys);

                _toggleAll = toggleAll;
            }
            #endregion

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(!AnimationWindowPipe.IsRecording());
            bool keyframesBtn = GUILayout.Button(KEYFRAMES_BTN_LABEL, GUILayout.Width(200));
            EditorGUI.EndDisabledGroup();
            GUI.contentColor = Color.white;

            GUILayout.Space(10);
            bool forceUpdate = GUILayout.Button(FORCE_UPDATE_BTN_LABEL, GUILayout.Width(200));
            bool resetSelected = GUILayout.Button(RESET_SELECTED_BTN_LABEL, GUILayout.Width(200));
            bool resetAll = GUILayout.Button(RESET_ALL_BTN_LABEL, GUILayout.Width(200));
            GUILayout.Space(10);

            GUILayout.Label(GROUPS_LABEL);
            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(_groupsOptions.Length == 0); 
            _selectedGroup = EditorGUILayout.Popup(string.Empty, _selectedGroup, _groupsOptions, GUILayout.Width(200));
            bool toggleGroup = GUILayout.Button(TOGGLE_GROUP_BTN_LABEL, GUILayout.Width(200));
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                if (resetAll)
                    ResetAll();
                else if (resetSelected)
                    ResetSelected();
                else if (forceUpdate)
                    ForceUpdate();
                else if (keyframesBtn && !_creatingKeyframes)
                {
                    if (_toggledProps.Count == 0)
                    {
                        EditorUtility.DisplayDialog(KEYFRAMES_NOTOGGLED_HEADER, KEYFRAMES_NOTOGGLED_MSG, OK_LABEL);
                        return;
                    }

                    _ = CreateKeyframes(_toggledProps);
                }
                else if (toggleGroup && _selectedGroup >= 0)
                {
                    _toggledProps.Clear();
                    var propsList = new List<string>(_modelProperties.Keys);

                    foreach (var par in _groups.Groups[_selectedGroup].Parameters)
                    {
                        var parName = par.ToLowerInvariant();

                        if (propsList.Count > 0)
                        {
                            for (int i = propsList.Count - 1; i >= 0; i--)
                            {
                                var propKey = propsList[i];

                                if (propKey.ToLowerInvariant().StartsWith(parName))
                                {
                                    _toggledProps.Add(propKey);
                                    propsList.RemoveAt(i);
                                }
                            }

                            continue;
                        }

                        break;
                    }
                }
                else if (!_creatingKeyframes && needsUpdate)
                    ForceUpdate();
            }
        }

        private void ResetSelected()
        {
            foreach (var name in _toggledProps)
                ResetProperty(_modelProperties[name]);

            ForceUpdate();
        }

        private void ResetAll()
        {
            foreach (var item in _modelProperties)
                ResetProperty(item.Value);

            ForceUpdate();
        }

        private void ResetProperty(CubismProperty property)
        {
            var cubismParameter = property.Parameter;
            var serializedObj = property.SerializedObject;
            var serializedProp = property.SerializedProperty;

            serializedProp.floatValue = cubismParameter.DefaultValue;
            serializedObj.ApplyModifiedProperties();
            serializedObj.Update();
        }

        private void ForceUpdate() =>
            _target.Model.ForceUpdateNow();

        // Triggers keyframes creation for selected properties
        private async Task CreateKeyframes(IEnumerable<string> paramsNames)
        {
            _creatingKeyframes = true;

            try
            {
                foreach (var name in paramsNames)
                {
                    var item = _modelProperties[name];
                    var parameter = item.Parameter;
                    var serializedObj = item.SerializedObject;
                    var serializedProp = item.SerializedProperty;

                    float initValue = serializedProp.floatValue;

                    serializedObj.Update();
                    serializedProp.floatValue = serializedProp.floatValue == parameter.MinimumValue ? 
                        parameter.MaximumValue : parameter.MinimumValue;

                    serializedObj.ApplyModifiedProperties();

                    await Task.Delay(100, _token);

                    serializedProp.floatValue = initValue;
                    serializedObj.ApplyModifiedProperties();
                }
            }
            catch (OperationCanceledException) {}
            catch { throw; }
            finally
            {
                _creatingKeyframes = false;
            }
        }
    }
}