using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks, IListener
{
    public GameUI gameUI;

    public Sheep[] whiteSheeps;
    public Sheep[] blackSheeps;
    public Transform[] wSpawnPositions;
    public Transform[] bSpawnPositions;
    public SheepIcon[] icons;

    [HideInInspector]
    public float coolDown = 0.0f;
    [HideInInspector]
    public bool isPlaying = false;
    [HideInInspector]
    public bool isReady = false;
    [HideInInspector]
    public List<int> sheeps;


    private float maxCooldown;
    private Sheep currentSheep = null;
    private System.Random rand;
    private string gameVersion = "1";
    private byte maxPlayersPerRoom = 2;
    private string playTx;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        maxCooldown = GameManager.Instance.maxCooldown;
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);
        Play();
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

    public void Play()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Not in room");
            return;
        }
        isPlaying = true;
        isReady = true;
        coolDown = 0f;
        int seed = GameManager.Instance.currentSeed;
        // rand = new System.Random(seed);
        rand = new System.Random();
        sheeps = new List<int>();
        for (int i = 0; i < 200; i++)
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
            currentSheep.laneIndex = laneIndex;
            yield return new WaitForSeconds(coolDown);
            currentSheep.BeSpawned(currentSheep.laneIndex);
            gameUI.SendTurn(sheepIndex, currentSheep.laneIndex);
            NextTurn();
        }
        else
        {
            currentSheep.transform.position = wSpawnPositions[laneIndex].position;
            currentSheep.laneIndex = laneIndex;
        }
    }

    void SpawnSheeps(bool isWhite, int sheepIndex, int laneIndex)
    {
        if (isWhite)
        {
            Sheep sheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
            sheep.BeSpawned(laneIndex);
            gameUI.SendTurn(sheepIndex, laneIndex);
        }
        else
        {
            Sheep sheep = Instantiate<Sheep>(blackSheeps[sheepIndex], bSpawnPositions[laneIndex].position, Quaternion.identity, bSpawnPositions[laneIndex]);
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
