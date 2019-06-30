using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin_Controller : MonoBehaviour
{
    private Game_Manager gm;
    private gameObject loadedChunk;
    private Text coinCounterUI;

    private GameObject target;
    public GameObject coinTrailPrefab;
    private GameObject coinTrail;
    public GameObject coinParticlePrefab;
    private GameObject coinParticle;
    
    public bool rewardCoins = false;
    private bool inCoinMagnetRadius = false;
    private bool startTrackingPos = false;
    
    public List<AudioClip> coinPickupSounds;
    public AudioSource coinAudioSource;


    void Start()
    {
        // if the loaded chunk is flipped, flip the coin to face the correct direciton
        if (loadedChunk.transform.localEulerAngles == new Vector3(0, 180, 0))
            this.transform.localEulerAngles = new Vector3(0, 180, 0);

        // randomly shift the coin on the Z axis to avoid overlap/clipping
        this.transform.localPosition += new Vector3(0, 0, Random.Range(-1.2f, -0.65f));

        gm = Game_Manager.instance;
        coinCounterUI = gm.coinAmount_txt;
        target = gm.coinCountPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if the ball hits the coin
        if (collision.gameObject == gm.ballCoinCollider)
        {
            TapticManager.Impact(ImpactFeedback.Light);

            // if coinCount is being tracked (via a daily challenge), add one more to the count.
            if (PlayerPrefs.GetInt("trackCoinAmount", 1) == 0)
                PlayerPrefs.SetInt("coinsGathered", PlayerPrefs.GetInt("coinsGathered", 0) + 1);

            CoinParticle(this.transform);
            startTrackingPos = true;
            StartCoroutine("CoinCollect");

            coinTrail = Instantiate(coinTrailPrefab, this.transform);
        }
        // if the ball has an active Coin Magnet
        if (collision.gameObject.GetComponent<coinMagnet_Controller>() != null)
        {
            inCoinMagnetRadius = true;
            StartCoroutine("CoinMagnetCollect");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<coinMagnet_Controller>() != null)
            inCoinMagnetRadius = false;
    }

    IEnumerator CoinCollect()
    {
        PlayCoinPickupSound();

        if (coinTrail == null)
            coinTrail = Instantiate(coinTrailPrefab, this.transform);

        if (rewardCoins) // coins earned through a challenge shouldn't have a trail
            coinTrail.GetComponent<TrailRenderer>().sortingOrder = 101;

        var elapsedTime = 0f;
        var startingPos = this.transform.position;
        var timeToCollect = 0.2f;
        Vector2 arcPoint = startingPos + new Vector3(target.transform.position.x - startingPos.x, target.transform.position.y - startingPos.y + Random.Range(-1f, 2.5f), 0);

        while (elapsedTime < timeToCollect)
        {
            var timer = (elapsedTime / timeToCollect);

            // moves the coins in an arc with a lerp between two lerps, lerpception?
            Vector2 m1 = Vector2.Lerp(startingPos, arcPoint, timer);
            Vector2 m2 = Vector2.Lerp(arcPoint, target.transform.position, timer);
            var move = Vector2.Lerp(m1, m2, timer);

            this.transform.position = move;


            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (elapsedTime >= timeToCollect)
        {
            PlayerPrefs.SetFloat("Coins", PlayerPrefs.GetFloat("Coins") + 1);
            gm.RefreshCoinCount();

            CoinParticle(target.transform);

            coinTrail.GetComponent<TrailRenderer>().emitting = false;

            this.GetComponent<SpriteRenderer>().enabled = false;
            gm.StartCoroutine("CoinCountBump");
            Destroy(this.gameObject, 0.3f);
        }
    }

    IEnumerator CoinMagnetCollect()
    {
        var startingPos = this.transform.position;

        // bring coin to ball, which then initiates normal CoinCollect
        while (inCoinMagnetRadius)
        {
            this.transform.position = Vector2.Lerp(this.transform.position, gm.ballObj.transform.position, Time.deltaTime * 7);
            yield return null;
        }
    }

    void CoinParticle(Transform pos)
    {
        coinParticle = Instantiate(coinParticlePrefab);
        coinParticle.SetActive(true);
        coinParticle.transform.position = new Vector3(pos.position.x, pos.position.y, 0);
        coinParticle.transform.SetParent(gm.cameraObj.transform);
        coinParticle.GetComponent<ParticleSystem>().Emit(5);
        Destroy(coinParticle, 0.7f);
    }

    public void PlayCoinPickupSound()
    {
        var coinPickupSoundRandom = Random.Range(0, coinPickupSounds.Count);
        coinAudioSource.PlayOneShot(coinPickupSounds[coinPickupSoundRandom]);
    }
}
