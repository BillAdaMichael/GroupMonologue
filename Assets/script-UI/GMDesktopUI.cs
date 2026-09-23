using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GMDesktopUI : MonoBehaviour
{
    public GameObject CanvasInstructions;
    public GameObject PleasePutOntheheadset;
    public GameObject GetAction;
    public GameObject ImageYouhave5sec;
    public GameObject timepic;
    public GameObject cutpic;
    public MotionCapture moCapEvent;

   
    void Start()
    {
        PleasePutOntheheadset.SetActive(false);
        GetAction.SetActive(false);
        ImageYouhave5sec.SetActive(false);
        timepic.SetActive(false);
        cutpic.SetActive(false);
    }

    void Update()
    {

        moCapEvent.recordingEnded += FinishRecordUI;
        moCapEvent.reRecordTimeout += TimeoutUI;
    }

    private void Awake()
    {

        moCapEvent = new MotionCapture();
        moCapEvent.recordingStarted += StartRecordUI;

    }

    private void StartRecordUI()
    {
        GetAction.SetActive(false);

        StartCoroutine(SwitchUI());
    }

    private IEnumerator SwitchUI()
    {
        yield return new WaitForSeconds(5f); // wait 5 sec

        if (GetAction != null) // ui1 disapear
        {
            GetAction.SetActive(false);
        }

        if (ImageYouhave5sec != null) // ui2 show
        {
            ImageYouhave5sec.SetActive(true);
            timepic.SetActive(true);
        }
    }

    private void FinishRecordUI()
    {
        PleasePutOntheheadset.SetActive(false);
        GetAction.SetActive(false);
        ImageYouhave5sec.SetActive(false);
        timepic.SetActive(false);
        cutpic.SetActive(true);
    }

    private void TimeoutUI() 
    {
        PleasePutOntheheadset.SetActive(false);
        GetAction.SetActive(true);
        ImageYouhave5sec.SetActive(false);
        timepic.SetActive(false);
        cutpic.SetActive(false);
    }


}
