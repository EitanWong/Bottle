using UnityEditor;
using UnityEngine;

namespace Smart
{
    using GL = GUILayout;
    using static EditorGUILayout;

	public partial class Inspector
    {
        bool AssertEditor(Editor editor)
        {
            if (null == editor) { return false; }
            if (null == editor.target) { return false; }
            if (null == editor.targets) { return false; }

            return true;
        }

        void DRAW_LINE()
        {
            GUIStyle line = new GUIStyle("In Title");
            GL.Box(GUIContent.none, line, GL.Height(1));
        }

        void DRAW_MULTI_EDITING()
        {
            if (!multiEditing) { return; }
            
            DRAW_LINE();
            GUIStyle label = "label";
            label.wordWrap = true;
            GL.Label("Components that are only on some of the selected objects cannot be multi-edited", label);
        }
    }
}
