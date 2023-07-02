using TARGET = UnityEngine.Video.VideoPlayer;

namespace Lean.Transition.Method
{

    /// <summary>This component allows you to transition the VideoPlayer's alpha value.</summary>
    [UnityEngine.HelpURL(LeanTransition.HelpUrlPrefix + "LeanVideoPlayerAlpha")]
    [UnityEngine.AddComponentMenu(LeanTransition.MethodsMenuPrefix + "VideoPlayer/VideoPlayer.alpha" +
                                  LeanTransition.MethodsMenuSuffix + "(LeanVideoPlayerAlpha)")]

    public class LeanVideoPlayerAlpha : LeanMethodWithStateAndTarget
    {
        public override System.Type GetTargetType()
        {
            return typeof(TARGET);
        }

        public override void Register()
        {
            PreviousState = Register(GetAliasedTarget(Data.Target), Data.Value, Data.Duration, Data.Ease);
        }

        public static LeanState Register(TARGET target, float value, float duration, LeanEase ease = LeanEase.Smooth)
        {
            var state = LeanTransition.SpawnWithTarget(State.Pool, target);

            state.Value = value;
			
            state.Ease = ease;

            return LeanTransition.Register(state, duration);
        }

        [System.Serializable]
        public class State : LeanStateWithTarget<TARGET>
        {
            [UnityEngine.Tooltip("The alpha value will transition to this.")]
            [UnityEngine.Serialization.FormerlySerializedAs("Alpha")][UnityEngine.Range(0.0f, 1.0f)]public float Value = 1.0f;

            [UnityEngine.Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [System.NonSerialized] private float oldValue;

            public override int CanFill => Target != null && Target.targetCameraAlpha != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.targetCameraAlpha;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.targetCameraAlpha;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.targetCameraAlpha = UnityEngine.Mathf.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
            }

            public static System.Collections.Generic.Stack<State> Pool = new System.Collections.Generic.Stack<State>(); public override void Despawn() { Pool.Push(this); }
        }

        public State Data;
    }
}

namespace Lean.Transition
{
    public static partial class LeanExtensions
    {
        public static TARGET alphaTransition(this TARGET target, float value, float duration, LeanEase ease = LeanEase.Smooth)
        {
            Method.LeanVideoPlayerAlpha.Register(target, value, duration, ease); return target;
        }
    }
}