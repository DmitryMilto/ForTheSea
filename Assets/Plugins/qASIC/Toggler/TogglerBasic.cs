using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace qASIC.Toggling
{
    public class TogglerBasic : Toggler
    {
#if ENABLE_INPUT_SYSTEM
        public Key key = Key.F2;
#else
        [KeyCodeListener]
        public KeyCode key = KeyCode.F2;
#endif

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current[key].wasPressedThisFrame)
#else
            if (Input.GetKeyDown(key))
#endif
                KeyToggle();
        }
    }
}