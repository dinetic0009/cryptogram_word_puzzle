using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class JsonController : Singleton<JsonController>
{
    public PlayerData playerData;

    public void OnEnable()
    {
        var content = PlayerPrefs.GetString("user_data");

        if (!string.IsNullOrEmpty(content))
            playerData = JsonUtility.FromJson<PlayerData>(content);
        else
            playerData = new PlayerData();
    }

    public void SaveCodes(List<Entry> codes)
    {
        playerData.Codes = codes;
        playerData.hasCodes = true;
        SaveToJson();
    }

    public void SaveStates(List<Letter> allSlots)
    {
        var list = new List<int>();
        allSlots.ForEach(x => list.Add((int)x.state));

        playerData.States = list;
        playerData.hasStates = true;
        SaveToJson();
    }

    public void SaveToJson()
    {
        PlayerPrefs.SetString("user_data", JsonUtility.ToJson(playerData));
    }

    public void ResetData()
    {
        playerData.hasCodes = false;
        playerData.hasStates = false;
        playerData.Codes = new();
        playerData.States = new();
        SaveToJson();
    }

}//Class


[System.Serializable]
public class PlayerData
{
    public bool hasCodes;
    public bool hasStates;

    public List<Entry> Codes;
    public List<int> States;

    public PlayerData()
    {
        hasCodes = false;
        hasStates = false;
        Codes = new();
        States = new();
    }
}

[System.Serializable]
public class Entry
{
    public char _char;
    public int _code;

    public Entry(char _char, int _code)
    {
        this._char = _char;
        this._code = _code;
    }
}
