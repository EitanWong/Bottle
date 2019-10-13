using UnityEditor;
using UnityEngine;

namespace Smart
{
    using GL = GUILayout;

	public partial class Inspector
    {
        void PREVIEW_BUTTON(Editor[] previews)
        {
            if (!HasPreview(previews)) { return; }

            GL.FlexibleSpace();

            if (GL.Button("(Experimental) Preview", "PreButton"))
            {
                preview = Preview.Open(previews);
            }
            GL.Space(3);
        }

        bool HasPreview(Editor[] editors)
        {
            for(int i = 0; i < editors.Length; i++)
            {
                if (editors[i].HasPreviewGUI()) { return true; }
            }

            return false;
        }
    }
}
