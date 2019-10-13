using UnityEditor;
using UnityEngine;

namespace Smart
{
    using static GUILayout;
    using static EditorGUILayout;
    using static EditorGUISmart;

    using GLE = EditorGUILayout;

    public partial class Inspector
    {
        void SETTINGS(bool drawSelectedSettings)
        {
            GLE.BeginVertical();
            if (drawSelectedSettings)
            {
                DrawSelectedSettings();
            }

            DrawSearchBar();

            //displaySettings = GL.Toggle(displaySettings, "Settings", "Foldout");

            DrawSettingsContent();
            GLE.EndVertical();
        }

        void DrawSelectedSettings()
        {
            GLE.BeginHorizontal(ExpandWidth(true));
            {
                if (Button("Close", "ButtonLeft"))
                {
                    CLOSE();
                    
                    return;
                }
                stackEditors = Toggle(stackEditors, "Stack", "ButtonMid");
                locked = Toggle(locked, "Lock", "ButtonRight");
            }
            GLE.EndHorizontal();
        }

        void DrawSearchBar()
        {
            //GLE.Space();
            GLE.BeginHorizontal();
            {
                filter = TextField(GUIContent.none, filter, "SearchTextField");
                if(Button(GUIContent.none, "SearchCancelButton"))
                { filter = ""; EditorGUI.FocusTextInControl(null); }
                displaySettings = Toggle(displaySettings, "Settings", DontExpandWidth);
            }
            GLE.EndHorizontal();
            //GLE.Space();
        }

        void DrawSettingsContent()
        {
            if (!displaySettings) { return; }

            GLE.BeginVertical("In BigTitle");
            {
#if NAME_FIELD
                displayName = Toggle(displayName, "Display Names", "radio");
#endif
                displayReference = Toggle(displayReference, "Display References", "radio");
                drawMaterialInspector = Toggle(drawMaterialInspector, "Display Materials", "Radio");
                moveButtons = Toggle(moveButtons, "Move Buttons", "Radio");
                matchWord = Toggle(matchWord, "Match Word", "Radio");
                hideUnfiltered = Toggle(hideUnfiltered, "Hide Unfiltered", "Radio");
                editorStyle = (int)(SmartEditorStyles)EnumPopup("Style", (SmartEditorStyles)editorStyle);
                editorButtonStyle = (int)(SmartButtonStyle)EnumPopup("Button Style", (SmartButtonStyle)editorButtonStyle);
#if DEV_SETTINGS
                DeleteEditors();
#endif
            }
            GLE.EndVertical();
        }
    }
}
