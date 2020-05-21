using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.ExtendedEditorGUI.Animations {
    public abstract class BaseTweenValue<T> {// : ISerializationCallbackReceiver {
        protected T _origin;
        protected T _target;

        private double _lastTime;
        
        public float Speed {get; set;} = 1;
        public bool IsAnimating { get; private set; }

        public T Target {
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
                LerpPosition = 1;
            }
        }
        

        protected double LerpPosition { get; private set; } = 1;

        public event UnityAction OnUpdate;
        public event UnityAction OnBegin;
        public event UnityAction OnFinish;

        protected BaseTweenValue(T value) {
            _origin = value;
        }

        private void Update() {
            double current = EditorApplication.timeSinceStartup;
            double delta = current - _lastTime;
            _lastTime = current;

            LerpPosition += delta * Speed;
            
            OnUpdate?.Invoke();

            if(LerpPosition >= 1f) {
                OnFinish?.Invoke();
                
                _origin = _target;
                LerpPosition = 1f;

                EditorApplication.update -= Update;
                IsAnimating = false;
            }
        }

        private void StartAnimation() {        
            if(!IsAnimating) {
                EditorApplication.update += Update;
                IsAnimating = true;
            }

            LerpPosition = 0;
            _lastTime = EditorApplication.timeSinceStartup;

            OnBegin?.Invoke();
        }

        protected abstract T GetValue();

        public static implicit operator T(BaseTweenValue<T> tween) => tween.Value;

        // void ISerializationCallbackReceiver.OnAfterDeserialize() {
        //     Debug.Log("BaseTweenValue.OnAfterDeserialize");
        // }

        // void ISerializationCallbackReceiver.OnBeforeSerialize() {
        //     Debug.Log("BaseTweenValue.OnBeforeSerialize");
        // }

        public virtual void DebugGUI() {
            Speed = EditorGUI.FloatField(Layout.GetRect(16), "Speed", Speed);
            EditorGUI.LabelField(Layout.GetRect(16), $"isAnimating: {IsAnimating}");
            EditorGUI.LabelField(Layout.GetRect(16), $"LerpPosition: {LerpPosition}");
            EditorGUI.LabelField(Layout.GetRect(16), $"_start: {_origin}");
            EditorGUI.LabelField(Layout.GetRect(16), $"_target: {_target}");
            EditorGUI.LabelField(Layout.GetRect(16), $"Value: {Value}");
        }
    }

    public class FloatTween : BaseTweenValue<float> {
        public FloatTween(float value = 0) : base(value) {}

        protected override float GetValue() => Mathf.Lerp(_origin, _target, (float)LerpPosition);


        public override void DebugGUI() {
            var newTarget = EditorGUI.DelayedFloatField(Layout.GetRect(16), "New target", _target);
            base.DebugGUI();

            if(newTarget != _target) {
                Target = newTarget;
            }
        }
    }

    public class BoolTween : BaseTweenValue<bool> {
        public BoolTween(bool value = false) : base(value) {}

        protected override bool GetValue() {
            var start = _origin ? 0f : 1f;
            var end = 1f - start;

            return Mathf.Lerp(start, end, (float)LerpPosition) > 0;
        }


        public override void DebugGUI() {
            var newTarget = EditorGUI.Toggle(Layout.GetRect(16), "New target", _target);
            base.DebugGUI();

            if(newTarget != _target) {
                Target = newTarget;
            }
        }
    }
}