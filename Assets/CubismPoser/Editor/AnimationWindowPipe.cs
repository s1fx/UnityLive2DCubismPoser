using UnityEngine;
using System.Reflection;
using System;
using Object = UnityEngine.Object;

namespace SF.Cubism.Poser
{
    public static class AnimationWindowPipe
    {
        #region OpenedAnimationWindow [private helper class]
        private class OpenedAnimationWindow
        {
            public Object Window;
            public Type AnimEditorType;
            public object AnimEditor;
            public Type WindowStateType;
            public object WindowStateObject;
        }
        #endregion

        private static readonly BindingFlags s_flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        private static Type s_animationWindowType = null;

        public static bool IsRecording() =>
            GetWindowProperty<bool>("recording");

        private static T GetWindowProperty<T>(string name)
        {
            var window = GetOpenedAnimationWindow();

            if (window != null)
            {
                var controlInterface = window.WindowStateType.GetProperty("controlInterface").GetValue(window.WindowStateObject);
                return (T)controlInterface.GetType().GetProperty(name).GetValue(controlInterface);
            }

            return default;
        }

        private static OpenedAnimationWindow GetOpenedAnimationWindow()
        {
            Object[] openAnimationWindows = Resources.FindObjectsOfTypeAll(GetAnimationWindowType());

            if (openAnimationWindows.Length == 0)
                return null;

            var window = openAnimationWindows[0];

            FieldInfo animEditor = GetAnimationWindowType().GetField("m_AnimEditor", s_flags);

            Type animEditorType = animEditor.FieldType;
            object animEditorObject = animEditor.GetValue(window);
            FieldInfo animWindowState = animEditorType.GetField("m_State", s_flags);
            Type windowStateType = animWindowState.FieldType;

            var openedWindow = new OpenedAnimationWindow()
            {
                Window = window,
                AnimEditorType = animEditorType,
                AnimEditor = animEditorObject,
                WindowStateType = windowStateType,
                WindowStateObject = animWindowState.GetValue(animEditorObject)
            };

            return openedWindow;
        }

        private static Type GetAnimationWindowType()
        {
            s_animationWindowType ??= Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
            return s_animationWindowType;
        }
    }
}