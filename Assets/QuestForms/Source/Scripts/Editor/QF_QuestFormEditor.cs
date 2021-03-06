using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using QuestForms;

namespace QuestForms.Internal
{
    [CustomEditor(typeof(QF_QuestForm))]
    public class QF_QuestFormEditor : Editor
    {
        // Variables to change
        private SerializedProperty questAsset;
        private SerializedProperty saveDataLocally;
        private SerializedProperty usePersistentDataPath;
        private SerializedProperty exportType;
        private SerializedProperty savePath;
        private SerializedProperty fileName;
        private SerializedProperty form;

        private QF_QuestForm formMono;
        
        // Style stuff
        private Texture2D headerBackgroundTex;
        private GUIStyle titleLabel;
        private bool pageFoldout;
        private int displayPageIndex = 0;

        private void OnEnable()
        {
            questAsset = serializedObject.FindProperty("questionnaire");
            form = serializedObject.FindProperty("form");
            formMono = target as QF_QuestForm;

            exportType = serializedObject.FindProperty("exportType");
            saveDataLocally = serializedObject.FindProperty("saveDataLocally");
            usePersistentDataPath = serializedObject.FindProperty("usePersistentDataPath");
            savePath = serializedObject.FindProperty("savePath");
            fileName = serializedObject.FindProperty("fileName");

            Color c = new Color(74f / 255f, 74f / 255f, 74f / 255f);
            headerBackgroundTex = QF_QuestionnaireEditor.MakeTex(Screen.width, 1, c);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            titleLabel = new GUIStyle(EditorStyles.boldLabel);
            titleLabel.fontSize += 3;
            titleLabel.normal.background = headerBackgroundTex;

            GUIStyle title = new GUIStyle(EditorStyles.boldLabel);
            title.fontSize += 10;
            title.alignment = TextAnchor.UpperCenter;

            GUILayout.Space(10);
            GUILayout.Label("Quest Forms", title);
            GUILayout.Space(10);

            // Update selected quest asset
            EditorGUILayout.PropertyField(form);

            EditorGUILayout.LabelField("Setup", titleLabel);
            // Buttons to interface with the manager
            FormButtons();

            EditorGUILayout.LabelField("Navigation", titleLabel);
            // Dropdown with pageSelection
            Navigation();

            EditorGUILayout.LabelField("Export Settings", titleLabel);
            // Exporter
            ExportSelection();

            serializedObject.ApplyModifiedProperties();
        }

        private void ExportSelection()
        {
            EditorGUILayout.PropertyField(saveDataLocally);

            if (!saveDataLocally.boolValue) return;

            EditorGUILayout.PropertyField(usePersistentDataPath);

            if (usePersistentDataPath.boolValue)
            {
                savePath.stringValue = Application.persistentDataPath;
            }

            EditorGUILayout.PropertyField(exportType);
            EditorGUILayout.PropertyField(fileName);

            GUI.enabled = !usePersistentDataPath.boolValue;
            GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(savePath);

            if (GUILayout.Button("Select Path", GUILayout.MaxWidth(100)) 
                && !usePersistentDataPath.boolValue)
            {
                string startFolder = string.IsNullOrEmpty(savePath.stringValue) ? "" : savePath.stringValue;

                string p = EditorUtility.OpenFolderPanel(
                    "Save Questionnaire Data Folder", startFolder, ""
                );

                if (!string.IsNullOrEmpty(p))
                {
                    savePath.stringValue = p;
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = !GUI.enabled;
        }

        private void Navigation()
        {
            pageFoldout = EditorGUILayout.Foldout(pageFoldout, "Pages");

            int lastID = displayPageIndex;
            if (pageFoldout)
            {
                for (int i = 0; i < formMono.Pages.Count; i++)
                {
                    Page p = formMono.QuestSource.pages[i];
                    if (GUILayout.Button(p.ID))
                    {
                        displayPageIndex = i;
                    }
                }
            }

            if (displayPageIndex != lastID) 
            {
                formMono.SetPage(lastID, false);
                formMono.SetPage(displayPageIndex, true);
            }
        }

        private void FormButtons()
        {
            Color original = GUI.color;
            if (questAsset.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Select a QF_Questionnaire in order to generate a new Questionnaire", MessageType.Warning, true);
                GUI.backgroundColor = Color.red;
            }
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate UI", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(40)) &&
                questAsset.objectReferenceValue != null)
            {
                if (formMono.transform.childCount > 0)
                {

                    if (EditorUtility.DisplayDialog("Generate UI",
                        "Create new form from Questionnaire? This will clear any changes to the current one you have", "Confirm", "No"))
                    {
                        formMono.PrintData();
                        formMono.LoadQuestionnaire();
                    }
                }
                else
                {
                    formMono.PrintData();
                    formMono.LoadQuestionnaire();
                }
            }

            if (GUILayout.Button("Clear UI", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(40)) &&
                questAsset.objectReferenceValue != null)
            {
                if (EditorUtility.DisplayDialog("Clear UI",
                    "Clear UI? This will delete any changes to the current one you have", "Yes", "No"))
                {
                    formMono.CleanUp();
                }
            }

            if (GUILayout.Button("Hide", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(40)) &&
                questAsset.objectReferenceValue != null)
            {
                for (int i = 0; i < formMono.Pages.Count; i++)
                {
                    formMono.SetPage(formMono.Pages[i], false);
                }
            }

            if (GUILayout.Button("Show", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(40)) &&
                questAsset.objectReferenceValue != null)
            {
                formMono.SetPage(formMono.Pages[0], true);
            }

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = original;
            EditorGUILayout.EndHorizontal();
        }
    }
}
