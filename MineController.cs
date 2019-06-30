using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine_Controller : MonoBehaviour
{
    public Game_Manager gm;
    private GameObject loadedChunk;
    private AudioLowPassFilter lowpass;
    private bool isPaused;

    [Header("Mine Properties")]
    public GameObject mineObj;
    public float speed;
    public float waitTime;
    private Light mineLight;
    public bool mineActive = true;
    public Transform pointHolder;
    private Vector2[] waypoints;


    void Start()
    {
        loadedChunk = this.transform.parent.gameObject;
        mineLight = this.GetComponentInChildren<Light>();
        gm = Game_Manager.instance;

        CreateWaypoints();
        StartCoroutine(FollowPath(waypoints));

        // Subscribe to Pause Event
        gm.onGamePaused += PauseGameToggle;
    }

    void OnDisable()
    {
        // Unsubscribe to Pause Event
        gm.onGamePaused -= PauseGameToggle;
    }

    void PauseGameToggle()
    {
        if (isPaused)
        {
            Destroy(lowpass);
            isPaused = false;
        }
        else
        {
            lowpass = mineObj.AddComponent<AudioLowPassFilter>();
            lowpass.cutoffFrequency = 500;
            isPaused = true;
        }

    }

    IEnumerator FollowPath(Vector2[] waypoints)
    {
        var RandomStart = Random.Range(0, waypoints.Length - 1);
        mineObj.transform.position = waypoints[RandomStart];

        int targetWaypointIndex = RandomStart + 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];

        while (mineActive)
        {
            mineObj.transform.position = Vector3.MoveTowards(mineObj.transform.position, targetWaypoint, speed * Time.deltaTime);
            while (isPaused)
                yield return null;

            if (mineObj.transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
            }
            yield return null;
        }
    }

    void CreateWaypoints()
    {
        waypoints = new Vector2[pointHolder.childCount];

        // if level is flipped, flip waypoints
        if (loadedChunk.transform.localEulerAngles == new Vector3(0, 180, 0))
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = pointHolder.GetChild(i).position;
                waypoints[i] = new Vector2(-waypoints[i].x, waypoints[i].y);
            }
        }
        // otherwise keep them as is
        else
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = pointHolder.GetChild(i).position;
                waypoints[i] = new Vector2(waypoints[i].x, waypoints[i].y);
            }
        }
    }
}
