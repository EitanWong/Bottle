using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Smart
{
    using GL = GUILayout;
    using static EditorGUISmart;
    using static EditorGUIUtility;

    public partial class Inspector
	{
        bool DisplayComponentButton()
        {
            // Exit
            if (open) { return false; }
            // Exit
            if (null == gameObjects) { return false; }
            // Exit
            if (0 == gameObjects.Length) { return false; }
            // Exit
            if(!hasGameObject) { return false; }
            // Exit
            //if (mouseDragged) { return false; }

            return true;
        }
        void ADD_COMPONENT_BUTTON()
        {
            if (!DisplayComponentButton()) { return; }

            GL.Space(0);

            DRAW_LINE();

            BeginCenterArea();
            if (GL.Button("Add Component", "AC Button", GL.MaxWidth(230)))
            {
                OpenComponentWindow();
                e.Use();
                Repaint();
            }
            EndCenterArea();
        }

        void OpenComponentWindow()
        {
            Type type = Type.GetType("UnityEditor.AddComponent.AddComponentWindow, UnityEditor");
            MethodInfo method = type.GetMethod("Show", BindingFlags.Static | BindingFlags.NonPublic);

            editorRect.y += standardVerticalSpacing;

            object[] args = new object[2]
            {
                editorRect,
                gameObjects
            };

            method.Invoke(null, args);
        }
    }
}
