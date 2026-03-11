using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;
using TMPro;

[CustomEditor(typeof(BoardData), false)]
[CanEditMultipleObjects]
[System.Serializable]
public class BoardDataDrawer : Editor
{
    private BoardData gameDataInstance => target as BoardData;
    private ReorderableList _dataList;

    private void OnEnable()
    {
        _dataList = new ReorderableList(serializedObject, serializedObject.FindProperty("board"), true, true, true, true);
        _dataList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Board");
        };

        _dataList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = _dataList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawColumnsRowsInputFields();

        EditorGUILayout.Space();

        if (gameDataInstance.board != null && gameDataInstance.columns > 0 && gameDataInstance.rows > 0)
        {
            DrawBoradTable();
        }

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(gameDataInstance);
        }
    }

    private void DrawColumnsRowsInputFields()
    {
        var columnsTemp = gameDataInstance.columns;
        var rowsTemp = gameDataInstance.rows;

        gameDataInstance.columns = EditorGUILayout.IntField("Columns", gameDataInstance.columns);
        gameDataInstance.rows = EditorGUILayout.IntField("Rows", gameDataInstance.rows);

        if ((gameDataInstance.columns != columnsTemp || gameDataInstance.rows != rowsTemp) 
            && gameDataInstance.columns > 0 
            && gameDataInstance.rows > 0)
        {
            gameDataInstance.CreateNewBoard();
        }
    }

    private void DrawBoradTable()
    {
        var tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var headerColumnStyle = new GUIStyle();
        headerColumnStyle.fixedWidth = 35;

        var columnsStyle = new GUIStyle();
        columnsStyle.fixedWidth = 50;

        var rowsStyle = new GUIStyle();
        rowsStyle.fixedHeight = 25;
        rowsStyle.fixedWidth = 40;
        rowsStyle.alignment = TextAnchor.MiddleCenter;

        var textFieldStyle = new GUIStyle();
        textFieldStyle.normal.background = Texture2D.grayTexture;
        textFieldStyle.normal.textColor = Color.white;
        textFieldStyle.fontStyle = FontStyle.Bold;
        textFieldStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.BeginHorizontal(tableStyle);
        for (var x = 0; x < gameDataInstance.columns; x++)
        {
            EditorGUILayout.BeginVertical(columnsStyle);

            for (var y = 0; y < gameDataInstance.rows; y++)
            {
                EditorGUILayout.BeginHorizontal(rowsStyle);

                if (gameDataInstance.board[x] != null && gameDataInstance.board[x].row != null)
                {
                    string value = gameDataInstance.board[x].row[y] ?? "";

                    var character = EditorGUILayout.TextArea(value, textFieldStyle);

                    if (character.Length > 1)
                        character = character.Substring(0, 1);

                    gameDataInstance.board[x].row[y] = character;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
}
