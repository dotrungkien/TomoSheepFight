using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int wScore;
    public int bScore;

    public Queue<int> whiteSheeps;
    public Queue<int> blackSheeps;
}
