using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "ScriptableObjects/Level", order = 1)]
public class LevelSO : ScriptableObject
{
    [TextArea] public string phrase;
    public string autherName;

    [Header("")]
    public int visibilityPercentage;
    public int singleLockCount;
    public int dualLockCount;

}//