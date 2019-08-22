using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class GameController : MonoBehaviour, IListener
{
    public Sheep[] whiteSheeps;
    public Sheep[] blackSheeps;
    public Transform[] wSpawnPositions;
    public Transform[] bSpawnPositions;

    public float coolDown = 0.0f;
    public bool isPlaying = false;
    public bool isReady = false;

    [HideInInspector]
    public List<int> sheeps;
    public SheepIcon[] icons;

    public NetworkManager networkManager;

    private float maxCooldown;
    private Sheep currentSheep = null;
    private System.Random rand;

    private void Start()
    {
        maxCooldown = GameManager.Instance.maxCooldown;
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);
    }

    public void UpdateIcons()
    {
        for (int i = 0; i < 3; i++)
        {
            icons[i].SwitchSheep(sheeps[i]);
        }
    }

    IEnumerator MockBlackSheepSpawn()
    {
        System.Random mockRand = new System.Random();
        while (true)
        {
            int sheepIdx = mockRand.Next() % 5;
            int laneIdx = mockRand.Next() % 5;
            SpawnSheeps(false, sheepIdx, laneIdx);
            icons[3].SwitchSheep(sheepIdx);
            yield return new WaitForSeconds(maxCooldown);
        }
    }

    public void SpawnBlackSheep(int sheepIdx, int laneIdx)
    {
        SpawnSheeps(false, sheepIdx, laneIdx);
        icons[3].SwitchSheep(sheepIdx);
    }

    public void NextTurn()
    {
        sheeps.RemoveAt(0);
        UpdateIcons();
        ResetCooldown();
    }

    public void Play(string tx)
    {
        isPlaying = true;
        // StartCoroutine(MockBlackSheepSpawn());
        isReady = true;
        coolDown = 0f;
        // var subTx = tx.Substring(0, 8);
        // int seed = Convert.ToInt32(subTx, 16);
        // rand = new System.Random(seed);
        rand = new System.Random();
        sheeps = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            sheeps.Add(rand.Next() % 5);
        }
        UpdateIcons();
    }

    public void AddNewSheep()
    {
        sheeps.Add(rand.Next() % 5);
    }

    private void Update()
    {
        if (!isPlaying) return;
        if (!isReady)
        {
            coolDown -= Time.deltaTime;
            if (coolDown <= 0f)
            {
                coolDown = 0.0f;
                isReady = true;
                // AddNewSheep();
            }
        }
    }

    public void SpawnLane(int laneIndex)
    {
        StartCoroutine(PrepareSheep(sheeps[0], laneIndex));
    }

    public IEnumerator PrepareSheep(int sheepIndex, int laneIndex)
    {
        if (currentSheep == null)
        {
            currentSheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
            yield return new WaitForSeconds(coolDown);
            currentSheep.BeSpawned(laneIndex);
            networkManager.SendTurn(sheepIndex, laneIndex);
            NextTurn();
        }
        else
        {
            currentSheep.transform.position = wSpawnPositions[laneIndex].position;
        }
    }


    void SpawnSheeps(bool isWhite, int sheepIndex, int laneIndex)
    {
        if (isWhite)
        {
            Sheep sheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
            sheep.BeSpawned(laneIndex);
            networkManager.SendTurn(sheepIndex, laneIndex);
        }
        else
        {
            Sheep sheep = Instantiate<Sheep>(blackSheeps[sheepIndex], bSpawnPositions[laneIndex].position, Quaternion.identity, bSpawnPositions[laneIndex]);
            // sheep.direction = -1;
            sheep.BeSpawned(laneIndex);
        }

    }

    public void ResetCooldown()
    {
        coolDown = maxCooldown;
        isReady = false;
        currentSheep = null;
    }

    public void GameOver()
    {
        isPlaying = false;
        StopAllCoroutines();
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.GAMEOVER:
                GameOver();
                break;
        }
    }
}
