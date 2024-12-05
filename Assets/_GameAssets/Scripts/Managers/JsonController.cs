using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using Random = UnityEngine.Random;

public class JsonController : Singleton<JsonController>
{
    public PlayerData playerData;

    [Space(5)]
    [Header(" ----- For convert level to jason file -----")]
    public LevelSO SourceLevel;
    public string MainFolderName;
    public string InnerFolderName;

    [Space(5)]
    [Header(" ----- Jason data -----")]
    [SerializeField] JasonPlayerData JasonFiledata;
    [SerializeField] TextAsset _jasonfile;

    [Space(10)]
    [Header("# empty_____ $ lock_____ & Double Locked")]
    [SerializeField, TextArea] string EditedPhrase;


    [Header(" --- debug things --- ")]
    public bool fileDataSame;
    public string WrongFilename;
    public JasonPlayerData testjasondata;

    public void OnEnable()
    {
        //var content = PlayerPrefs.GetString("user_data");

        //if (!string.IsNullOrEmpty(content))
        //    playerData = JsonUtility.FromJson<PlayerData>(content);
        //else
        //    playerData = new PlayerData();
    }


    public void SaveCodes(List<Entry> codes)
    {
        playerData.Codes = codes;
        playerData.hasCodes = true;
        SaveToJson();
    }


    public void SetPlayerData(TextAsset jason)
    {
        JasonFiledata = JsonUtility.FromJson<JasonPlayerData>(jason.ToString());
        if (JasonFiledata == null || JasonFiledata.States == null || JasonFiledata.States.Count == 0)
            return;

        playerData = new PlayerData();
        playerData.Codes = JasonFiledata.Codes;
        playerData.States = JasonFiledata.States;
        playerData.hasCodes = true;
        playerData.hasStates = true;
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

    List<int> SetPhraseStates()
    {
        List<int> states = new List<int>();

        for (int i = 0; i < EditedPhrase.Length; i++)
        {
            if (!char.IsLetter(EditedPhrase[i]) && (EditedPhrase[i] == '#' || EditedPhrase[i] == '$' || EditedPhrase[i] == '&'))
            {
                switch (EditedPhrase[i])
                {
                    case '#':
                        states.Add(1); // empyt state
                        break;
                    case '$':
                        states.Add(3);   // locked state
                        break;
                    case '&':
                        states.Add(4); // Doublelocked state
                        break;
                    default:
                        break;
                }
            }
            else if (char.IsLetter(EditedPhrase[i]))
            {
                states.Add(0); // fill state
            }
            else
            {
                states.Add(2); // blank or symbol  state
            }
        }

        return states;

    }

    [ButtonMethod]
    public void CreateJasonData()
    {
        if (MainFolderName.Length == 0)
        {
            Debug.LogError("your main folder name is empty");
            return;
        }

        if (InnerFolderName.Length == 0)
        {
            Debug.LogError("your Inner folder name is empty");
            return;
        }

        if (SourceLevel == null)
        {
            Debug.LogError("your source level is null");
            return;
        }

        JasonFiledata = new JasonPlayerData();

        if (EditedPhrase == null || EditedPhrase.Length == 0)
        {
            EditedPhrase = SourceLevel.phrase.ToLower();
            Debug.LogError("Set Your Edited Phrase");
        }

        JasonFiledata.Phrase = SourceLevel.phrase;
        JasonFiledata.EditedPhrase = EditedPhrase;


        if (SourceLevel.phrase.ToLower() == EditedPhrase)
            return;

        SetPhraseStates();



        string _phrase = SourceLevel.phrase;
        _phrase = _phrase.ToLower();
        var distinctList = _phrase.Where(char.IsLetter).Distinct().ToList();
        List<Entry> codes = new();
        Vector2Int simpleRange = new(1, 20), vowelRange = new(19, 25);

        distinctList.ForEach(letter =>
        {
            int _code;
            var range = letter.IsVowel() ? vowelRange : simpleRange;
            do
            {
                _code = Random.Range(range.x, range.y);
            }
            while (codes.Any(x => x._code == _code));

            codes.Add(new(letter, _code));
        });

        JasonFiledata.Phrase = _phrase;

        JasonFiledata.Codes = codes;

        JasonFiledata.States = SetPhraseStates();

    }

    [ButtonMethod]
    public void CreateJasonFile()
    {
        if (JasonFiledata == null || JasonFiledata.States == null || JasonFiledata.States.Count == 0)
        {
            Debug.LogError("Need to Set jason class Data");
            return;
        }

        string json = JsonUtility.ToJson(JasonFiledata, true);

        // Define path using Application.persistentDataPath (not Resources)

        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Resources", MainFolderName, InnerFolderName);


        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Construct the file path
        string filePath = Path.Combine(folderPath, SourceLevel.name + ".json");

        // Write the JSON data to the file
        File.WriteAllText(filePath, json);


        string resourcePath = MainFolderName + "/" + InnerFolderName + "/" + SourceLevel.name;

        _jasonfile = Resources.Load<TextAsset>(resourcePath);

        UnityEditor.AssetDatabase.Refresh();

        //Debug.Log($"JSON file saved to: {filePath}" + "                      --------------  " + $"jason file pathe : {resourcePath}");
    }


    [ButtonMethod]
    public void UpdateJasondataByFile()
    {
        JasonFiledata = JsonUtility.FromJson<JasonPlayerData>(_jasonfile.ToString());
        EditedPhrase = JasonFiledata.EditedPhrase;
    }

    public void ResetJasonData()
    {
        PlayerPrefs.SetString("user_data", "");
    }


    [ButtonMethod]
    public void IsPhraseDataSame()
    {
        fileDataSame = true;
        WrongFilename = "";
        testjasondata = new JasonPlayerData();
        List<LevelSO> _levels = new List<LevelSO>();
        List<TextAsset> _levelJasons = new List<TextAsset>();

        _levels.AddRange(Resources.LoadAll<LevelSO>("_GameResources/Update_1")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());
        _levelJasons.AddRange(Resources.LoadAll<TextAsset>("LevelJasonFile/Update_1")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());


        _levels.AddRange(Resources.LoadAll<LevelSO>("_GameResources/Levels")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());
        _levelJasons.AddRange(Resources.LoadAll<TextAsset>("LevelJasonFile/Levels")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());


        _levels.AddRange(Resources.LoadAll<LevelSO>("_GameResources/Update_2")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());
        _levelJasons.AddRange(Resources.LoadAll<TextAsset>("LevelJasonFile/Update_2")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());

        if (_levels.Count != _levelJasons.Count)
            Debug.Log("count of file  is not equal");

        for (int i = 0; i < _levels.Count; i++)
        {

            if (i < _levelJasons.Count)
            {
                testjasondata = JsonUtility.FromJson<JasonPlayerData>(_levelJasons[i].ToString());
                if (testjasondata.EditedPhrase.Length != _levels[i].phrase.Length)
                {
                    fileDataSame = false;
                    WrongFilename = _levels[i].name;
                    break;
                }
            }
        }

        if (fileDataSame)
            testjasondata = new JasonPlayerData();
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
public class JasonPlayerData
{
    public string Phrase;
    public string EditedPhrase;

    public List<Entry> Codes;
    public List<int> States;
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
