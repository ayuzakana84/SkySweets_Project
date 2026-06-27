using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpawnableBlock
{
    public GameObject prefab;
    [Header("出やすさ（数字が大きいほど出やすい）")]
    public int weight = 10;
}

public class BlockSpawner : MonoBehaviour
{
    [Header("流してくるブロックの種類")]
    [SerializeField] private SpawnableBlock[] spawnableBlocks;

    [Header("生成する高さの範囲")]
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 4f;

    [Header("生成ペースの範囲")]
    [SerializeField] private float minSpawnInterval = 2.0f;
    [SerializeField] private float maxSpawnInterval = 3.0f;

    private float timer = 0f;
    private float currentInterval; //次にブロックを出すまでの時間を記憶する変数
    private int totalWeight = 0; //ブロックの出やすさの合計値を記憶する変数

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;

        //ブロックの出やすさの合計を求める
        foreach (var block in spawnableBlocks)
            totalWeight += block.weight;

        SetNextInterval();
    }

    void Update()
    {
        //ポーズ中、ゲーム終了演出中、またはプレイヤーが存在しない場合は止める
        if (gm.IsPaused || gm.CurrentState == GameManager.GameState.Ending || gm.Player == null)
            return;

        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            SpawnBlock();
            timer = 0f;

            SetNextInterval();
        }
    }

    private void SpawnBlock()
    {
        if (spawnableBlocks == null || spawnableBlocks.Length == 0)
            return;

        //ブロックの種類をランダムに選ぶ
        int randomValue = Random.Range(0, totalWeight);

        GameObject selectedPrefab = null;
        int currentWeight = 0;

        //ランダムで選ばれた数値のブロックを探す
        foreach (var block in spawnableBlocks)
        {
            currentWeight += block.weight;

            if (randomValue < currentWeight)
            {
                selectedPrefab = block.prefab;
                break;
            }
        }

        if (selectedPrefab == null) return;

        //高さをランダムに決める
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(transform.position.x, randomY, 0f);

        //ブロックを生成する
        Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
    }

    //目標時間をランダムに決定して記憶する関数
    private void SetNextInterval()
    {
        currentInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
