using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Player_Controller : MonoBehaviour
{
    [Header("Global")]
    public Game_Manager gm;
    public Camera cam;
    [Space(10)]

    [Header("Touch Info")]
    public int touchState = 0;
    [SerializeField]
    private Vector2 touchStartPos;
    [SerializeField]
    private Vector2 touchActivePos;
    [SerializeField]
    private Vector2 touchEndPos;

    [Header("Ball & Aim Line")]
    public Rigidbody2D ballRigid;
    private GameObject ballObj;

    public LineRenderer aimLine;
    private GameObject aimLineObj;
    public Vector2 aimDir;
    [SerializeField]
    public float maxPull;
    private float pullDistClamped;


    private GameObject cameraObj;
    private float cameraOffset;
    private Vector2 activeCameraVector2;
    private Vector2 intialCameraVector2;
    private Vector2 touchStartCameraOffset;

    private bool isPaused = false;

    [Header("Sounds")]
    public List<AudioClip> ballKickSounds;
    public AudioSource ballKickAudioSource;


    void Start()
    {
        ballObj = ballRigid.GameObject;
        aimLineObj = aimLine.GameObject;
        cameraOffset = ballObj.transform.position.y;
        ballKickAudioSource = ballObj.GetComponent<AudioSource>();
        cameraObj = cam.GameObject;
    }

    void OnEnable()
    {
        // Subscribe to Pause Event
        gm.onGamePaused += PauseGameToggle;
    }

    void OnDisable()
    {
        // Unubscribe from Pause Event
        gm.onGamePaused -= PauseGameToggle;
    }

    void PauseGameToggle()
    {
        if (!isPaused) // pause the game
        {
            ballRigid.simulated = false;
            aimLine.enabled = false;
            ballKickAudioSource.enabled = false;

            isPaused = true;
        }
        else // unpause the game
        {
            ballRigid.simulated = true;
            aimLine.enabled = true;
            ballKickAudioSource.enabled = true;

            isPaused = false;
        }
    }

    // Debug Gizmos
    private void OnDrawGizmos()
    {
        // initial touch position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(touchStartPos + touchStartCameraOffset, 0.2f);

        // active touch position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(touchActivePos, 0.2f);

        // end touch position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(touchEndPos, 0.2f);

    }

    void Update()
    {
        activeCameraVector2 = new Vector2(cameraObj.transform.position.x, cameraObj.transform.position.y);

        // initial touch
        if (Input.GetMouseButtonDown(0))
        {
            if (touchState == 0)
                touchState = 1;

            touchStartPos = cam.ScreenToWorldPoint(Input.mousePosition);
            intialCameraVector2 = new Vector2(cameraObj.transform.position.x, cameraObj.transform.position.y);
            aimLineObj.SetActive(true);
        }

        // Account for a moving level by getting the offset between the Active camera pos & Initial camera pos
        touchStartCameraOffset = (activeCameraVector2 - intialCameraVector2);

        // active touch
        if (Input.GetMouseButton(0))
        {
            if (touchState == 1)
                touchState = 2;

            touchActivePos = cam.ScreenToWorldPoint(Input.mousePosition);

            // Manipulate aimline
            var ballObjVector2 = new Vector2(ballObj.transform.position.x, ballObj.transform.position.y);
            aimDir = ((touchStartPos + touchStartCameraOffset) - touchActivePos).normalized;
            pullDistClamped = Mathf.Clamp(Vector2.Distance((touchStartPos + touchStartCameraOffset), touchActivePos), 0, maxPull);
            aimLine.SetPosition(0, ballObjVector2);
            aimLine.SetPosition(1, ballObjVector2 + aimDir * pullDistClamped);

        }

        // end touch
        if (Input.GetMouseButtonUp(0))
        {
            if (touchState == 2)
                touchState = 3;
            touchEndPos = cam.ScreenToWorldPoint(Input.mousePosition);
            aimLineObj.SetActive(false);

            // Add Force and Torque to the ball
            ballRigid.velocity = ((touchStartPos + touchStartCameraOffset) - touchEndPos).normalized * Mathf.Clamp(Vector2.Distance((touchStartPos + touchStartCameraOffset), touchEndPos) * 5, 0, maxPull * 8);
            ballRigid.AddTorque(-((touchStartPos + touchStartCameraOffset).x - touchEndPos.x) * 400);

            // play a ball kick sound
            var kickRandom = Random.Range(0, ballKickSounds.Count);
            ballKickAudioSource.volume = Game_Manager.map(pullDistClamped, 1, 2.5f, 0f, 0.3f);
            ballKickAudioSource.PlayOneShot(ballKickSounds[kickRandom]);

            if (touchState == 3)
                touchState = 0;
        }
    }
}
