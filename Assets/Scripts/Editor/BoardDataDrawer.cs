using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

[CustomEditor(typeof(BoardData), false)]
[CanEditMultipleObjects]
[System.Serializable]
public class BoardDataDrawer : Editor
{
    private BoardData gameDataInstance => target as BoardData;
    private ReorderableList _dataList;

    private void OnEnable()
    {
        InitializeReordableList(ref _dataList, "searchWords", "Search Words");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        gameDataInstance.timeInSeconds = 
            EditorGUILayout.FloatField("Max Game Time (in Seconds)", gameDataInstance.timeInSeconds);
            
        DrawColumnsRowsInputFields();

        EditorGUILayout.Space();

        ConvertToUpperButton();

        if (gameDataInstance.board != null && gameDataInstance.columns > 0 && gameDataInstance.rows > 0)
        {
            DrawBoradTable();
        }

        GUILayout.BeginHorizontal();
        ClearBoardButton();
        FillUpWithRandomLettersButton();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        _dataList.DoLayoutList();

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

    private void InitializeReordableList(ref ReorderableList list, string propertyName, string listLabel)
    {
        list = new ReorderableList (serializedObject, serializedObject.FindProperty(propertyName), 
            true, true, true, true);

        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, listLabel);
        };

        var l = list;

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect (rect.x, rect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("word"), GUIContent.none
                );
        };
    }

    private void ConvertToUpperButton()
    {
        if (GUILayout.Button("To Upper"))
        {
            for (var i = 0; i < gameDataInstance.columns; i++)
            {
                for (var j = 0; j < gameDataInstance.rows; j++)
                {
                    var errorCounter = Regex.Matches(gameDataInstance.board[i].row[j], @"[a-z]").Count;

                    if (errorCounter > 0)
                    {
                        gameDataInstance.board[i].row[j] = gameDataInstance.board[i].row[j].ToUpper();
                    }
                }
            }

            foreach (var searchWord in gameDataInstance.searchWords)
            {
                var errorCounter = Regex.Matches(searchWord.word, @"[a-z]").Count;

                if (errorCounter > 0)
                {
                    searchWord.word = searchWord.word.ToUpper();
                }
            }
        }
    }

    private void ClearBoardButton()
    {
        if (GUILayout.Button("Clear Board"))
        {
            for (int i = 0; i < gameDataInstance.columns; i++)
            {
                for (int j = 0; j < gameDataInstance.rows; j++)
                {
                    gameDataInstance.board[i].row[j] = " ";
                }
            }
        }
    }

    private void FillUpWithRandomLettersButton()
    {
        if (GUILayout.Button("Fill Up With Random Letters"))
        {
            for (int i = 0; i < gameDataInstance.columns; i++)
            {
                for (int j = 0; j < gameDataInstance.rows; j++)
                {
                    int errorCounter = Regex.Matches(gameDataInstance.board[i].row[j], @"[a-zA-Z]").Count;
                    string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    int index = Random.Range(0, letters.Length);

                    if (errorCounter == 0)
                    {
                        gameDataInstance.board[i].row[j] = letters[index].ToString();
                    }
                }
            }
        }
    }
}
