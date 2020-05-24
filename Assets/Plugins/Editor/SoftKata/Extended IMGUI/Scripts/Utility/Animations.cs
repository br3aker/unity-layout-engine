using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.UnityEditor.Animations {
    public abstract class TweenValueBase<T> {// : ISerializationCallbackReceiver {
        protected T _origin;
        protected T _target;

        private double _lastTime;
        
        public float Speed {get; set;} = 1;
        public bool IsAnimating { get; private set; }

        public T Target {
            get => _target;
            set {
                if(!_target.Equals(value)) {
                    _origin = Value;
                    _target = value;
                    StartAnimation();
                }
            }
        }
        public T Value {
            get => GetValue();
            set {
                if(IsAnimating) {
                    EditorApplication.update -= Update;
                    IsAnimating = false;
                }
                _origin = value;
                _target = value;
                _progress = 1;
            }
        }
        
        private double _progress;
        public double LerpPosition {
            get => _progress * _progress * (3f - 2f * _progress);
        }

        public event UnityAction OnUpdate;
        public event UnityAction OnStart;
        public event UnityAction OnFinish;

        protected TweenValueBase(T value) {
            _origin = value;
            _target = value;
        }

        private void Update() {
            double current = EditorApplication.timeSinceStartup;
            double delta = current - _lastTime;
            _lastTime = current;

            _progress += delta * Speed;
            
            OnUpdate?.Invoke();

            if(_progress >= 1) {
                _origin = _target;
                _progress = 0;

                EditorApplication.update -= Update;
                IsAnimating = false;

                OnFinish?.Invoke();
            }
        }

        private void StartAnimation() {        
            if(!IsAnimating) {
                EditorApplication.update += Update;
                IsAnimating = true;
            }

            _progress = 0;
            _lastTime = EditorApplication.timeSinceStartup;

            OnStart?.Invoke();
        }

        protected abstract T GetValue();

        public static implicit operator T(TweenValueBase<T> tween) => tween.Value;

        // void ISerializationCallbackReceiver.OnAfterDeserialize() {
        //     Debug.Log("BaseTweenValue.OnAfterDeserialize");
        // }

        // void ISerializationCallbackReceiver.OnBeforeSerialize() {
        //     Debug.Log("BaseTweenValue.OnBeforeSerialize");
        // }

        public virtual void DebugGUI() {
            Speed = global::UnityEditor.EditorGUI.FloatField(Layout.GetRect(16), "Speed", Speed);
            global::UnityEditor.EditorGUI.LabelField(Layout.GetRect(16), $"isAnimating: {IsAnimating}");
            global::UnityEditor.EditorGUI.LabelField(Layout.GetRect(16), $"LerpPosition: {LerpPosition}");
            global::UnityEditor.EditorGUI.LabelField(Layout.GetRect(16), $"_start: {_origin}");
            global::UnityEditor.EditorGUI.LabelField(Layout.GetRect(16), $"_target: {_target}");
            global::UnityEditor.EditorGUI.LabelField(Layout.GetRect(16), $"Value: {Value}");
        }
    }

    public class TweenFloat : TweenValueBase<float> {
        public TweenFloat(float value = 0) : base(value) {}

        protected override float GetValue() => Mathf.Lerp(_origin, _target, (float)LerpPosition);


        public override void DebugGUI() {
            var newTarget = global::UnityEditor.EditorGUI.DelayedFloatField(Layout.GetRect(16), "New target", _target);
            base.DebugGUI();

            if(newTarget != _target) {
                Target = newTarget;
            }
        }
    }

    public class TweenBool : TweenValueBase<bool> {
        public TweenBool(bool value = false) : base(value) {}

        protected override bool GetValue() => Fade > 0;

        public float Fade {
            get {
                var start = _origin ? 1 : 0;
                var end = 1 - start;

                return Mathf.Lerp(start, end, (float)LerpPosition);
            }
        }


        public override void DebugGUI() {
            var newTarget = global::UnityEditor.EditorGUI.Toggle(Layout.GetRect(16), "New target", _target);
            global::UnityEditor.EditorGUI.LabelField(Layout.GetRect(16), $"Fade: {Fade}");
            base.DebugGUI();

            if(newTarget != _target) {
                Target = newTarget;
            }
        }
    }
}