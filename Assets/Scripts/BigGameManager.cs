using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGameManager : MonoBehaviour
{
    private void Awake()
    {
        Object.DontDestroyOnLoad(this);
    }
}