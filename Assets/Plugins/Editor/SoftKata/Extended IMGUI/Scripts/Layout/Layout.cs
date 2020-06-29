using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace SoftKata.UnityEditor {
    /*!
        @brief Main static class providing automatic layout utility.

        Such as layout groups management & rect querying. Rect querying works both inside and outside layout scopes.

        @warning Layout functionality must be used only inside IMGUI loop.
    */
    public static partial class Layout {
        internal const float FlexibleWidth = -1;

        internal static LayoutGroup _currentGroup;

        /*!
            Current automatic content width.
            @return Width in pixels
        */
        public static float CurrentContentWidth {
            get => _currentGroup?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);
        }

        // Native Unity layout system get rect call
        internal static Rect GetRectFromUnityLayout(float width, float height) {
            var rect = GUILayoutUtility.GetRect(width, height);
            rect.width = width;
            return rect;
        }
        internal static Rect GetRectFromUnityLayout(float height) {
            return GetRectFromUnityLayout(EditorGUIUtility.currentViewWidth - 2, height);
        }

        /*!
            Begins layout scope.
            @param[in] group Target layout group
            @return bool flag if group and it's scope is valid


            Visible groups create "layout scope", each GetRect call belongs to currently active scope. 
            Not all groups are visible thus not all groups must be closed.

            It's strongly advised to use Begin-End pair this way:
            @code
                if(Layout.BeginLayoutScope(group)) {
                    // your code
                    Layout.EndCurrentScope();
                }
            @endcode

            @warning All visible groups must be closed with EndLayoutGroup
        */
        public static bool BeginLayoutScope(LayoutGroup group) {
            return 
                Event.current.type == EventType.Layout 
                ? group.BeginLayout()
                : group.BeginNonLayout();
        }
        /*!
            Ends layout group.
            @throws System.NullReferenceException if called without appropriate opening BeginLayoutScope(LayoutGroup group) call
        */
        public static void EndCurrentScope() {
            var group = _currentGroup;
            if(Event.current.type == EventType.Layout) {
                group.EndLayout();
            }
            else {
                group.EndNonLayout();
            }
            _currentGroup = group.Parent;
        }
    
        /*!
            Main method to get rect from layout system.
            @param[in] width Width
            @param[in] height Height
            @param[out] rect Initialized rect with requested size if true is returned, undefined otherwise
            @return bool flag if requested rect is valid

            @warning Consider rect invalid if this method has returned false and **do not** use it in GUI methods

            It's strongly advised to use it this way:
            @code
                if(Layout.GetRect(width: 160, height: 18, out var rect)) {
                    EditorGUI.DrawRect(rect, Color.Black);
                }
            @endcode
        */
        public static bool GetRect(float width, float height, out Rect rect) {
            if(_currentGroup != null) {
                return _currentGroup.GetRect(width, height, out rect);
            }
            rect = GetRectFromUnityLayout(width, height);
            return true;
        }
        /*!
            Main method to get rect from layout
            @param[in] height Height
            @param[out] rect Initialized rect with requested size if true is returned, undefined otherwise
            @return bool flag if requested rect is valid

            @warning Consider rect invalid if this method has returned false and **do not** use it in GUI methods

            Same as GetRect(float width, float height, out Rect rect) but uses automatic width.
        */
        public static bool GetRect(float height, out Rect rect) {
            if(_currentGroup != null) {
                return _currentGroup.GetRect(height, out rect);
            }
            rect = GetRectFromUnityLayout(height);
            return true;
        }
        /*!
            Main method to get rect from layout
            @param[in] width Width
            @param[in] height Height
            @return Initialized Rect with requested size

            Same idea as GetRect(float width, float height, out Rect rect) but this method does not return bool flag.
            Use this for "cleaner code" or when you have spare CPU time and don't want to use if statement:
            @code
                float Speed = 10;
                EditorGUI.FloatField(Layout.GetRect(width: 160, height: 18), "Speed", Speed);
            @endcode
        */
        public static Rect GetRect(float width, float height) {
            return _currentGroup?.GetRect(width, height) ?? GetRectFromUnityLayout(width, height);
        }
        /*!
            Main method to get rect from layout
            @param[in] width Width
            @param[in] height Height
            @return Initialized Rect with requested size

            Same as GetRect(float width, float height) but uses automatic width. 
        */
        public static Rect GetRect(float height) {
            return _currentGroup?.GetRect(height) ?? GetRectFromUnityLayout(height);
        }
    }
}