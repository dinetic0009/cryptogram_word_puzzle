using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PhraseSearchWindow : EditorWindow
{
    private string searchPhrase = "";
    private string resultMessage = "";
    private Vector2 scrollPosition;
    private List<List<char>> sameLetterGroups;

    [MenuItem("Tools/Phrase Search")]
    public static void ShowWindow()
    {
        GetWindow<PhraseSearchWindow>("Phrase Search");
    }

    private void OnGUI()
    {
        GUILayout.Label("Search for Phrase in Scriptable Objects", EditorStyles.boldLabel);

        // Text box for entering the search phrase
        searchPhrase = EditorGUILayout.TextArea(searchPhrase, GUILayout.Height(60));

        if (GUILayout.Button("Search"))
        {
            SearchForPhrase();
        }

        // Display result message
        EditorGUILayout.HelpBox(resultMessage, MessageType.Info);


        if (GUILayout.Button("Process Phrase"))
        {
            if(!string.IsNullOrEmpty(searchPhrase))
                InitGroups(searchPhrase);
        }

        // Scroll view for displaying groups
        GUILayout.Label("Character Groups:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        if (sameLetterGroups != null && sameLetterGroups.Count > 0)
        {
            foreach (var group in sameLetterGroups)
            {
                EditorGUILayout.LabelField($"{group[0]}    :    {group.Count}", EditorStyles.wordWrappedLabel);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No groups to display.", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.EndScrollView();
    }

    //private void SearchForPhrase()
    //{
    //    // Load all ScriptableObjects of type PhraseData from Resources
    //    LevelSO[] allPhrases = Resources.LoadAll<LevelSO>("_GameResources");

    //    // Loop through each PhraseData and check if it matches the search phrase
    //    bool foundMatch = false;
    //    foreach (LevelSO phraseData in allPhrases)
    //    {
    //        if (phraseData.phrase == searchPhrase)
    //        {
    //            resultMessage = $"Match found in '{phraseData.name}'!";
    //            foundMatch = true;
    //            break;
    //        }
    //    }

    //    if (!foundMatch)
    //    {
    //        resultMessage = "No matches found.";
    //    }
    //}

    private void SearchForPhrase()
    {
        // Load all ScriptableObjects of type LevelSO from Resources
        LevelSO[] allPhrases = Resources.LoadAll<LevelSO>("_GameResources");

        float maxSimilarity = 0f;
        string bestMatchName = "";

        // Loop through each LevelSO and check for similarity
        foreach (LevelSO phraseData in allPhrases)
        {
            float similarity = CalculateSimilarity(phraseData.phrase, searchPhrase);

            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                bestMatchName = phraseData.name;
            }
        }

        if (maxSimilarity > 0)
        {
            resultMessage = $"Best match: '{bestMatchName}' with {maxSimilarity * 100:F2}% similarity.";
        }
        else
        {
            resultMessage = "No matches found.";
        }
    }

    // Helper method to calculate similarity percentage
    private float CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
        {
            return 0f;
        }

        int matches = 0;
        int lengthToCheck = Mathf.Min(source.Length, target.Length);

        // Compare characters at the same index
        for (int i = 0; i < lengthToCheck; i++)
        {
            if (source[i] == target[i])
            {
                matches++;
            }
            else
            {
                break; // Break the loop if characters do not match
            }
        }

        // Calculate the similarity as a percentage
        return (float)matches / lengthToCheck;
    }


    public void InitGroups(string phrase)
    {
         sameLetterGroups = phrase
           .Where(x => !char.IsWhiteSpace(x) && !char.IsSymbol(x)) // Exclude spaces
           .GroupBy(x => x)                   // Group by character
           .Select(group => group.ToList())   // Convert each group to a list of characters
           .OrderByDescending(list => list.Count) // Order by frequency
           .ToList();

    }
}
