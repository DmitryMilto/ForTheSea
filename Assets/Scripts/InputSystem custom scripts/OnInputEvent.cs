using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class OnInputEvent : MonoBehaviour
{
    [InfoBox("Currently InputSystem is working only with Keyboard (PC).", InfoMessageType.Warning)] 
    [InfoBox("InputManager works as expected.")] 
    public List<OnInputEventMapper> Events;
    void Update()
    {
        foreach (var key in Events.Where(key => key.WasPressedThisFrame)) key.OnEvent.Invoke();
    }

    [Serializable]
    public class OnInputEventMapper
    {
#if ENABLE_INPUT_SYSTEM
        public Key Key;
        public bool WasPressedThisFrame => Keyboard.current[Key].wasPressedThisFrame;
#else
        public KeyCode Key;
        public bool WasPressedThisFrame => Input.GetKeyDown(Key);
#endif
        public UnityEvent OnEvent;
    }
}
