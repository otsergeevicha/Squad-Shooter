using UnityEngine;

namespace Watermelon
{
    /*public static class Control
    {
        private static IControlBehavior currentControl;
        public static IControlBehavior CurrentControl => currentControl;
        public static void SetControl(IControlBehavior controlBehavior)
        {
            currentControl = controlBehavior;
        }
        public static void Enable()
        {
#if UNITY_EDITOR
            if(currentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");
                return;
            }
#endif
            currentControl.EnableControl();
        }
        public static void Disable()
        {
#if UNITY_EDITOR
            if (currentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");
                return;
            }
#endif
            currentControl.DisableControl();
        }
    }*/
    public enum InputType
    {
        Keyboard = 0,
        UIJoystick = 1
    }
    public static class Control
    {
        private static IControlBehavior currentControl;
        public static IControlBehavior CurrentControl => currentControl;
        public static void SetControl(IControlBehavior controlBehavior)
        {
            currentControl = controlBehavior;
        }
        public static void Enable()
        {
#if UNITY_EDITOR
            if (currentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");
                return;
            }
#endif
            currentControl.EnableControl();
        }
        public static void Disable()
        {
#if UNITY_EDITOR
            if (currentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");
                return;
            }
#endif
            currentControl.DisableControl();
        }
        public static InputType GetCurrentInputType()
        {
#if UNITY_EDITOR
#if UNITY_ANDROID || UNITY_IOS
            return InputType.UIJoystick;
#else
            return InputType.Keyboard;
#endif
#else
#if UNITY_ANDROID || UNITY_IOS
            return InputType.UIJoystick;
#elif UNITY_WEBGL
            return Application.isMobilePlatform ? InputType.UIJoystick : InputType.Keyboard;
#else
            return InputType.Keyboard;
#endif
#endif
        }
    }
}