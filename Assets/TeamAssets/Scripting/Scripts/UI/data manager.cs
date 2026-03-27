using UnityEngine;
using System.IO;
using System;

public class datamanager
{
    private GameData m_gameData;
    private LevelData m_data;
    private static string m_filePath = Path.Combine(Application.persistentDataPath, "level_data.json");

    public datamanager(int numOfLevels, int level = 0)
    {
        m_gameData = new GameData();
        for (int i = 0; i < numOfLevels; i++)
        {
            AddLevel();
        }
        m_data = new LevelData();
        m_data.levelNum = level;
    }

    public void AddLevel()
    {
        if(m_gameData == null)
        {
            m_gameData = new GameData();
        }

        LevelData data = new LevelData();
        Array.Resize(ref m_gameData.levels, m_gameData.levels.Length + 1);
        m_gameData.levels[m_gameData.levels.Length - 1] = data;
    }

    public void SetLevelNum(int index, int level)
    {
        if(m_gameData == null)
        {
            m_gameData = new GameData();
            AddLevel();
        }

        if(index >= 0 && index < m_gameData.levels.Length)
        {
            m_gameData.levels[index].levelNum = level;
        }
        else
        {
            Debug.Log("index is not in array of levels");
        }
    }

    public void SetCompleted(int index, bool completed)
    {
        if(m_gameData == null)
        {
            m_gameData = new GameData();
            AddLevel();
        }

        if (index >= 0 && index < m_gameData.levels.Length)
        {
            m_gameData.levels[index].completed = completed;
        }
        else
        {
            Debug.Log("index is not in array of levels");
        }
        //if (m_data == null)
        //{
        //    m_data = new LevelData();
        //}

        //m_data.completed = completed;
    }

    public LevelData GetData() {  return m_data; }

    public GameData GetGameData() { return m_gameData; }

    public void SaveData()
    {
        using(StreamWriter w =  new StreamWriter(m_filePath))
        {
            string dataToWrite = JsonUtility.ToJson(m_data);
            w.Write(dataToWrite);
            w.Close();
        }
    }

    public void SaveGameData()
    {
        using(StreamWriter w = new StreamWriter(m_filePath))
        {
            string dataToWrite = JsonUtility.ToJson(m_gameData);
            w.Write(dataToWrite);
            w.Close();
        }
    }

    public void LoadData()
    {
        using(StreamReader r = new StreamReader(m_filePath))
        {
            string dataRead = r.ReadToEnd();
            m_data = JsonUtility.FromJson<LevelData>(dataRead);
            r.Close();
        }
    }

    public void LoadGameData()
    {
        using (StreamReader r = new StreamReader(m_filePath))
        {
            string dataRead = r.ReadToEnd();
            m_gameData = JsonUtility.FromJson<GameData>(dataRead);
            r.Close();
            //Debug.Log(m_gameData.levels.Length);
        }
    }
}

[System.Serializable]
public class LevelData
{
    public int bestScore;
    public bool completed;
    public int levelNum;
}

[System.Serializable]
public class GameData
{
    public LevelData[] levels;

    public GameData()
    {
        levels = new LevelData[] { };
    }
}