using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Sheep[] whiteSheeps;
    public Sheep[] blackSheeps;
    public GameObject[] sheepIcons;
    public Transform[] wSpawnPositions;
    public Transform[] bSpawnPositions;

    public float coolDown = 0.0f;
    public bool isPlaying = false;
    public bool isReady = false;
    public Stack sheeps;

    private Sheep currentSheep = null;
    private System.Random rand;


    public void Play(string tx)
    {
        isPlaying = true;
        isReady = true;
        coolDown = 0f;
        Debug.Log(string.Format("Play tx: {0}", tx));
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
        if (isReady)
        {
            SpawnSheeps(true, 2, laneIndex);
        }
        else
        {
            StartCoroutine(PrepareSheep(2, laneIndex));
        }
    }

    public IEnumerator PrepareSheep(int sheepIndex, int laneIndex)
    {
        if (currentSheep == null)
        {
            currentSheep = Instantiate<Sheep>(whiteSheeps[sheepIndex], wSpawnPositions[laneIndex].position, Quaternion.identity, wSpawnPositions[laneIndex]);
            yield return new WaitForSeconds(coolDown);
            currentSheep.BeSpawned();
            ResetCooldown();
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
            sheep.BeSpawned();
        }
        else
        {
            Sheep sheep = Instantiate<Sheep>(blackSheeps[sheepIndex], bSpawnPositions[laneIndex].position, Quaternion.identity, bSpawnPositions[laneIndex]);
            sheep.BeSpawned();
            sheep.direction = -1;
        }
        ResetCooldown();
    }

    public void ResetCooldown()
    {
        coolDown = 3.0f;
        isReady = false;
        currentSheep = null;
    }

    public int NextSheep()
    {
        return rand.Next();
    }
}
