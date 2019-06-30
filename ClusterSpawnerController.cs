using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner_Controller : MonoBehaviour
{
    public Game_Manager gm;
    private GameObject level;

    [Space(10)]
    public List<Vector2> coinSpots = new List<Vector2>();
    private int coinSpawnProbability;
    private int coinTypeToSpawn;
    private int percentChanceOf_1Coin;
    private int percentChanceOf_3Coins;
    private int percentChanceOf_10Coins;
    public GameObject coinClusterPrefab;
    private GameObject coinCluster;

    [Space(10)]
    public List<Vector2> powerUpSpots = new List<Vector2>();
    private int powerUpSpawnProbability;
    private int coinMagnet_Probability;
    private int heart_Probability;
    public GameObject coinMagnetPrefab;
    private GameObject coinMagnet;
    private bool coinMagnetActive = false;
    public GameObject heartPrefab;
    private GameObject heart;



    // Use this for initialization
    void Start()
    {
        Invoke("SpawnCoins", 0.2f);
        Invoke("SpawnPowerUps", 0.2f);

        level = this.transform.parent.gameObject;
    }

    public void TriggerCoinSpawns()
    {
        coinCluster.GetComponent<CoinCluster_Controller>().StartCoroutine("SpawnCoins");
    }

    void GetProbabilities()
    {
        coinMagnetActive = gm.coinMagentActive;

        if (coinMagnetActive) // increase coin spawn probability & amount when Magnet is active
        {
            coinSpawnProbability = 100;
            percentChanceOf_1Coin = 20;
            percentChanceOf_3Coins = 30;
            percentChanceOf_10Coins = 50;

        }

        if (!coinMagnetActive)
        {
            coinSpawnProbability = 80;
            percentChanceOf_1Coin = 60;
            percentChanceOf_3Coins = 25;
            percentChanceOf_10Coins = 15;

            // if the player has seen a 10 Coin Cluster within 8 levels, decrease probability
            if ((gm.currentLevel - PlayerPrefs.GetInt("LLTS_10Coins")) < 8)
            {
                coinSpawnProbability = 80;
                percentChanceOf_1Coin = 70;
                percentChanceOf_3Coins = 25;
                percentChanceOf_10Coins = 5;
            }
        }

        if (percentChanceOf_1Coin + percentChanceOf_3Coins + percentChanceOf_10Coins != 100)
        {
            Debug.Log("Error: Percent chances of coins do no equal 100");
            return;
        }
    }

    public void SpawnCoins()
    {
        // Roll to see if Coin Cluster should spawn
        var spawnRoll = UnityEngine.Random.Range(0, 101);

        // Spawn Cluster
        if (spawnRoll <= coinSpawnProbability)
        {
            // Roll to see which Coin Amount to spawn
            var coinTypeRoll = UnityEngine.Random.Range(0, 101);

            if (coinTypeRoll <= percentChanceOf_1Coin)
                coinTypeToSpawn = 0;
            if (coinTypeRoll > percentChanceOf_1Coin && coinTypeRoll <= percentChanceOf_1Coin + percentChanceOf_3Coins)
                coinTypeToSpawn = 1;
            if (coinTypeRoll > percentChanceOf_1Coin + percentChanceOf_3Coins)
            {
                coinTypeToSpawn = 2;
                PlayerPrefs.SetInt("LLTS_10Coins", gm.currentLevel);
            }

            // Roll to chose Cluster Position
            var positionRoll = UnityEngine.Random.Range(0, coinSpots.Count);

            coinCluster = Instantiate(coinClusterPrefab, level.transform);
            coinCluster.GetComponent<CoinCluster_Controller>().coinSpawnPos = coinSpots[positionRoll];

            if (coinTypeToSpawn == 0)
                coinCluster.GetComponent<CoinCluster_Controller>().coinsToSpawn = 1;

            if (coinTypeToSpawn == 1)
                coinCluster.GetComponent<CoinCluster_Controller>().coinsToSpawn = 3;

            if (coinTypeToSpawn == 2)
                coinCluster.GetComponent<CoinCluster_Controller>().coinsToSpawn = 10;

            coinCluster.GetComponent<CoinCluster_Controller>().StartCoroutine("SpawnCoins");
        }
    }

    public void SpawnPowerUps()
    {
        // Weigh Power Up spawn probability based on the Last Level To See a Power Up
        powerUpSpawnProbability = Mathf.Clamp((gm.currentLevel - PlayerPrefs.GetInt("LLTS_PowerUp")) * 2, 0, 50);

        if (gm.canSpawnHearts)
        {
            coinMagnet_Probability = 50;
            heart_Probability = 50;

            if (gm.coinMagentActive)
            {
                coinMagnet_Probability = 0;
                heart_Probability = 100;
            }
        }

        if (!gm.canSpawnHearts)
        {
            coinMagnet_Probability = 100;
            heart_Probability = 0;

            if (gm.coinMagentActive)
            {
                coinMagnet_Probability = 0;
                heart_Probability = 0;
            }
        }

        // Roll to see if a Power Up should spawn
        var powerUpSpawnRoll = UnityEngine.Random.Range(0, 101);

        if (powerUpSpawnRoll <= powerUpSpawnProbability)
        {
            PlayerPrefs.SetInt("LLTS_PowerUp", gm.currentLevel);

            var positionRoll = UnityEngine.Random.Range(0, powerUpSpots.Count);
            var powerUpTypeRoll = UnityEngine.Random.Range(0, 101);

            if (powerUpTypeRoll <= coinMagnet_Probability)
            {
                coinMagnet = Instantiate(powerUp_CoinMagnet, level.transform);

                if (level.transform.localEulerAngles.y == 180)
                    coinMagnet.transform.localEulerAngles = new Vector3(0, 180, 0);

                coinMagnet.transform.localPosition = powerUpSpots[positionRoll];
            }

            if (heart_Probability != 0 && powerUpTypeRoll > heart_Probability)
            {
                heart = Instantiate(powerUp_Heart, level.transform);

                if (level.transform.localEulerAngles.y == 180)
                    heart.transform.localEulerAngles = new Vector3(0, 180, 0);

                heart.transform.localPosition = powerUpSpots[positionRoll];
            }
        }
    }

    // Draw debug Gizmos
    private void OnDrawGizmos()
    {
        // Show Coin Spawn Locations
        Gizmos.color = Color.yellow;
        Vector2 spawnerPos = new Vector2(this.transform.position.x, this.transform.position.y);

        for (int i = 0; i < coinSpots.Count; i++)
        {
            Gizmos.DrawWireSphere(coinSpots[i] + spawnerPos, 0.8f);
        }


        // Show Power Up Spawn Locations
        Gizmos.color = Color.red;

        for (int i = 0; i < powerUpSpots.Count; i++)
        {
            Gizmos.DrawWireSphere(powerUpSpots[i] + spawnerPos, 0.3f);
        }
    }
}
