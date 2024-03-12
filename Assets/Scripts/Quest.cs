using System.Collections.Generic;
using UnityEngine;

public readonly struct Quest
{
    // TODO: Implement sub-quests. Main quest will have its _mainQuests of own sub-quests. GetNext() will get next sub-quest; if there is none, get next main quest.
    //       add _subQuests 

    private readonly string _name;    
    private readonly string _description;
    private readonly string _accomplishedText;
    public readonly GameObject QuestTarget;
    public readonly float QuestTargetRadius;
    public readonly float QuestTargetRadiusSqr;
    // private bool _done;
    private static int _currentQuestIndex = -1;  // index of active quest
    // add type?
    // add parent GameObject?

    public static Quest Current;
    // private static readonly Quest[] MainQuests;
    // List<int> termsList = new List<int>();
    private static readonly List<Quest> MainQuests = new List<Quest>();
    // private readonly GameObject _ui;
    // public readonly List<SpawnData> SpawnSequence;

    public static void Init()
    {
        // Must be called, so the static constructor is executed. Just to fuck with me.
        //https://stackoverflow.com/questions/59938630/why-the-static-constructor-in-a-struct-gets-called-when-calling-a-non-static-met
        
        ShowCurrent();
    }

    // dynamic constructor (has a static context)
    private Quest(string name, string description, string accomplishedText, float questTargetRadius)
    {
        _name = name;
        _description = description;
        _accomplishedText = accomplishedText;
        QuestTarget = GameObject.Find("questTargets").transform.GetChild(MainQuests.Count).gameObject;
        QuestTargetRadius = questTargetRadius;
        QuestTargetRadiusSqr = Mathf.Pow(QuestTargetRadius, 2);
        // _done = false;
    }

    static Quest()  // static constructor
    {
        //https://stackoverflow.com/questions/59938630/why-the-static-constructor-in-a-struct-gets-called-when-calling-a-non-static-met

        // Initialize quests
        MainQuests.Add(new Quest(
            "Fly there! (#1)",
            "Locate and reach target destination.",
            "First destination reached!",
            10));

        /*MainQuests.Add(new Quest(
            "Now fly there! (#2)",
            "Locate and reach this target destination.",
            "Send destination reached!"));

        MainQuests.Add(new Quest(
            "And now fly there! (#3)",
            "Reach and investigate target destination.",
            "Third destination reached!"));
        
        MainQuests.Add(new Quest(
            "No quest",
            "You have finished all quests. Now you can fuck off.",
            "Fuck off."));*/

        Next();
    }

    private static void Next()
    {
        if (_currentQuestIndex == MainQuests.Count - 1)  // this is the last quest
            return;
        Current = MainQuests[++_currentQuestIndex];
        ShowCurrent();
    }

    public static void Complete()
    {
        // _current._done = true;
        Current.QuestTarget.SetActive(false);
        // TODO: Show _accomplishedText
        Next();
    }

    // private static Quest GetCurrent()
    // {
    //     return  MainQuests[_currentQuestIndex];
    // }

    private static void ShowCurrent()  // https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
    {
        Current.QuestTarget.SetActive(true);

        // ▶▷▸▹▻◆◈◇
        // UI.ShowQuestText(Current._name, Current._description);

        // TargetLocationMarker.transform.position = quest._questTarget;
    }
}