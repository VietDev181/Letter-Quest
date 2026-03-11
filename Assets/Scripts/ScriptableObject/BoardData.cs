using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "BoardData", menuName = "ScriptableObjects/BoardData")]
public class BoardData : ScriptableObject
{
    [System.Serializable]
    public class SearchingWord
    {
        public string word;
    }

    [System.Serializable]
    public class BoardRow
    {
        public int size;
        public string[] row;

        public BoardRow()
        {
            
        }

        public BoardRow(int size)
        {
            CreateRow(size);
        }

        public void CreateRow(int size)
        {
            this.size = size;
            row = new string[size];
        }

        public void ClearRow()
        {
            for (int i = 0; i < size; i++)
            {
                row[i] = " ";
            }
        }
    }

    public float timeInSeconds;
    public int columns = 0;
    public int rows = 0;

    public BoardRow[] board;
    public List<SearchingWord> searchWords = new List<SearchingWord> ();

    public void ClearWithEmptyString()
    {
        for (int i = 0; i < columns; i++)
        {
            board[i].ClearRow();
        }
    }

    public void CreateNewBoard()
    {
        board = new BoardRow[columns];
        for (int i = 0; i < columns; i++)
        {
            board[i] = new BoardRow(rows);
        }
    }
}
