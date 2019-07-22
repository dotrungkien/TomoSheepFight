﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Sheep[] whiteSheeps;
    public Sheep[] blackSheeps;
    public GameObject[] sheepIcons;
    public Transform[] wSpawnPositions;
    public Transform[] bSpawnPositions;

    public float coolDown = 2.0f;
    public bool isReady = false;

    private GameObject sheepIcon;

    public void StartGame()
    {
        isReady = true;
    }

    private void Update()
    {
        if (!isReady)
        {
            coolDown -= Time.deltaTime;
            if (coolDown <= 0f)
            {
                coolDown = 0;
                isReady = true;
            }
        }
    }

    public void SpawnLane(int laneIndex)
    {
        if (isReady)
        {
            SpawnSheeps(true, 2, laneIndex);
        }
        else
        {
            PrepareSheep(2, laneIndex);
        }
    }

    void PrepareSheep(int sheepIndex, int laneIndex)
    {
         = Instantiate(sheepIcons[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
        GameObject.Destroy(sheep, coolDown);
    }


    void SpawnSheeps(bool isWhite, int sheepIndex, int laneIndex)
    {
        if (isWhite)
        {
            Sheep sheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
        }
        else
        {
            Sheep sheep = Instantiate<Sheep>(blackSheeps[sheepIndex], bSpawnPositions[laneIndex].position, Quaternion.identity, bSpawnPositions[laneIndex]);
            sheep.direction = -1;
        }
    }

    public void CoolDownReset()
    {
        coolDown = 2.0f;
        isReady = false;
    }
}