using System.Collections;
using UnityEngine;

public class LevelTransmitter : GoalTransmitor {

    public int levelID;

    protected virtual void OnTrigger() {
        Object.FindObjectOfType<GameManager>().EnterLevel(levelID);
    }
}