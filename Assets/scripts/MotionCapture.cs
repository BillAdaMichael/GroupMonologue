using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.InputSystem;

class CsvLogger
{
    StreamWriter file;

    public CsvLogger(string filename)
    {
        file = new StreamWriter(filename);
    }

    public CsvLogger(string filename, params string[] columnNames) : this(filename)
    {
        file.WriteLine(string.Join(",", columnNames));
    }

    public void AddRow(params double[] values)
    {
        file.WriteLine(string.Join(",", values.Select(f => f.ToString())));
    }

    public void Close()
    {
        file.Close();
    }
}

public class MotionCapture : MonoBehaviour
{
    public GameObject headset, leftController, rightController;
    public string folderName = "testFolder";
    public int maxRecordingCount, playerModelID;
    public float modelTextureY;
    public MotionReplay motionReplay;
    private int playerNumber;
    private float timePassed;
    private CsvLogger logger;
    private bool recording = false, reRecordWaiting = false, recordingButtonDown = false;
    private NewInputs inputActions;


    public event Action recordingStarted;
    public event Action recordingEnded;
    public event Action reRecordTimeout;
    // subscriber needs to do the following:
    // MotionCapture mc;
    // mc. recordingStarted += foo();
    // AND ALSO!
    // onDestory() { mc.recordingStarted -= foo(); } to avoid memory leaks

    private void Awake()
    {
        inputActions = new NewInputs();
        inputActions.Player.StartRecording.started += HandleStartRecordingButton;
        //inputActions.Player.StartRecording.performed += HandleStartRecordingButton;
        //inputActions.Player.StartRecording.canceled += HandleStartRecordingButton;
        inputActions.Player.Enable();
    }

    private void HandleStartRecordingButton(InputAction.CallbackContext context)
    {
        recordingButtonDown = context.ReadValueAsButton();

        //if (recordingButtonDown)
        //{
        //    OnButtonPressed.Invoke();
        //}
    }

    private void OnDestroy()
    {
        inputActions.Player.StartRecording.started -= HandleStartRecordingButton;
        //inputActions.Player.StartRecording.performed -= HandleStartRecordingButton;
        //inputActions.Player.StartRecording.canceled -= HandleStartRecordingButton;
        inputActions.Player.Disable();
    }

    void Start()
    {
        playerNumber = 0;
        //string saveLoc = folderName + "/" + playerNumber.ToString() + ".csv";
        //logger = openNewFile(saveLoc);
    }

    // wait for a user to press the record button
    // when pressed, check if a file with that player number exists. If it does, overwrite it.
    // At the end of the 5 second clip, if the time-out happens, increment the player number

    void Update()
    {
        if (recordingButtonDown && !recording)
        {
            recording = true;
            reRecordWaiting = false;
            recordingButtonDown = false;
            timePassed = 0;

            string saveLoc = folderName + "/" + playerNumber.ToString() + ".csv";
            logger = openNewFile(saveLoc);
            Debug.Log("[MotionCapture] recording started");

            recordingStarted?.Invoke();
        }

        if (recording && !reRecordWaiting)
        {
            recordMotion(timePassed, playerModelID, modelTextureY);
            timePassed += Time.deltaTime;

            // animation time-out. End it and ask if they want to re-record it.
            if (timePassed > 5f)
            {
                reRecordWaiting = true;
                recording = false;
                timePassed = 0;
                Debug.Log($"[MotionCapture] recording ended for {playerNumber}");
                recordingEnded?.Invoke();
                logger.Close();
                motionReplay.enabled = true;
            }
        }
        else if (reRecordWaiting)
        {
            timePassed += Time.deltaTime;
            Debug.Log($"[MotionCapture] waiting to reRecord: {timePassed}");
            motionReplay.enabled = false;

            if (timePassed > 10f)
            {
                reRecordWaiting = false;
                recording = false;

                playerNumber++;

                if(playerNumber < maxRecordingCount)
                { 
                    //string saveLoc = folderName + "/" + playerNumber.ToString() + ".csv";
                    //logger = openNewFile(saveLoc);
                    Debug.Log($"[MotionCapture] time-out. Waiting on player {playerNumber} recording start");
                }
                else
                {
                    Debug.Log($"[MotionCapture] {maxRecordingCount} recordings parsed. Move on!");
                    gameObject.SetActive(false);
                }
                reRecordTimeout?.Invoke();
            }
            // they want to record again. Go back to start
            
        }
    }

    private void recordMotion(float timeSinceActive, int modelID, float texY)
    {
        if(logger == null) {
            string saveLoc = folderName + "/" + playerNumber.ToString() + ".csv";
            logger = openNewFile(saveLoc);
        };
        logger.AddRow(  timeSinceActive,
                        headset.transform.position.x, headset.transform.position.y, headset.transform.position.z,
                        headset.transform.rotation.x, headset.transform.rotation.y, headset.transform.rotation.z, headset.transform.rotation.w,
                        rightController.transform.position.x, rightController.transform.position.y, rightController.transform.position.z,
                        rightController.transform.rotation.x, rightController.transform.rotation.y, rightController.transform.rotation.z, rightController.transform.rotation.w,
                        leftController.transform.position.x, leftController.transform.position.y, leftController.transform.position.z,
                        leftController.transform.rotation.x, leftController.transform.rotation.y, leftController.transform.rotation.z, leftController.transform.rotation.w,
                        modelID, texY
                    );
        //Debug.Log($"[MotionCapture] user {playerNumber} recording time: {timeSinceActive}");
    }

    private CsvLogger openNewFile(string _saveLoc)
    {
        // make sure the directory exists
        if (!Directory.Exists(folderName))
            Directory.CreateDirectory(folderName);

        // time, headset transform, right controller transform, left controller transform
        return new CsvLogger(_saveLoc,
            "animation time",
            "headPosition x", "headPosition y", "headPosition z",
            "headRot (Quat) x", "headRot (Quat) y", "headRot (Quat) z", "headRot (Quat) w",
            "RControllerPos x", "RControllerPos y", "RControllerPos z",
            "RControllerRot (Quat) x", "RControllerRot (Quat) y", "RControllerRot (Quat) z", "RControllerRot (Quat) w",
            "LControllerPos x", "LControllerPos y", "LControllerPos z",
            "LControllerRot (Quat) x", "LControllerRot (Quat) y", "LControllerRot (Quat) z", "LControllerRot (Quat) w",
            "modelID", "texY");
    }

    void OnApplicationQuit()
    {
        if (logger != null)
        {
            logger.Close();
        }
    }
}
