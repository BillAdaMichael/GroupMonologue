using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class MikeManager : MonoBehaviour
{
    bool AnimationPlaying = false;
    int AnimationID = 0;
    int TextIndex = 0;
    int TotalRecordings = 0;

    int GameState = 0; // 0: Intro Video Playing, 1: Gameplay Mode, 2: End Video Playing.

    [SerializeField] GameObject img_Bg;
    [SerializeField] Text txt_Caption;
    [SerializeField] Text txt_TotalRecordings;
    [SerializeField] Transform DesktopCam;
    [SerializeField] Transform Stage_Zero;
    [SerializeField] Transform Stage_Orbit;
    [SerializeField] Transform Stage_POV;
    [SerializeField] Transform Stage_ReviewClip;

    [SerializeField] AudioClip snd_IntroBGM;
    [SerializeField] AudioClip snd_OutroBGM;
    [SerializeField] AudioClip snd_Lv1;
    [SerializeField] AudioClip snd_Lv2;
    [SerializeField] AudioClip snd_Lv3;
    [SerializeField] AudioClip snd_Lv4;
    [SerializeField] AudioClip snd_Lv5;
    [SerializeField] AudioSource src;

    [SerializeField] GameObject pnl_Slideshow;
    [SerializeField] GameObject pnl_LookAtScreen;
    [SerializeField] GameObject pnl_PutOnHeadset;
    [SerializeField] GameObject pnl_PressAToStartRecording;
    [SerializeField] GameObject pnl_TimeRemaining;
    [SerializeField] GameObject pnl_Cut;
    [SerializeField] UnityEngine.UI.Image img_TimeProgress;

    [SerializeField] MotionCapture MC;

    float StartRecordTimestamp = 0f;

    float[] IntroTimestamps =
    {
        0f, 2.6f, 5.3f, 10.6f, 11.20f, 15.8f, 20.11f, 22.16f, 25.23f, 28.14f, 30.16f, 34.02f, 37.20f, 41.20f, 45.11f, 49.06f, 51.17f, 61.07f, 62f
    };

    float [] OutroTimestamps =
    {
        0f, 1.20f, 5.13f, 7.23f, 23.22f, 26.03f, 28.16f, 31.21f, 79.09f
    };

    string[] IntroStrings =
    {
        "Welcome to ACTION REFLEX!",
        "Today YOU are going to be the stars.",
        "That's right. In just 5 seconds of work, you're going to get 15 seconds of fame.",
        "The process is simple.",
        "Every person is going to take turns using the VR headset.",
        "During your turn, all you have to do is record a short animation.",
        "This animation can be anything.",
        "Try winding up your arm and throwing an imaginary ball.",
        "Or maybe try start cooking your best dish.",
        "Anything goes really.",
        "The fun starts when you hand your headset to the next person.",
        "They are going to see your animation continuously looping.",
        "So, it's up to them to reply, or add to the scene.",
        "After 12 recordings, we'll have enough material to wrap it up.",
        "So, let's get started. I can't wait to see what you do!",
        "Because at the end of the day, it's all just...",
        "ACTION REFLEX",
        "",
    };

    string[] OutroStrings =
    {
        "AND THAT'S A WRAP!",
        "It's not easy to put yourself on the spot like that, but you've done good.",
        "So, let's take a look at how it turned out!",
        "",
        "<color='black'>We hope you enjoyed playing ACTION REFLEX.</color>",
        "<color='black'>It's been a great time participating in this jam.</color>",
        "<color='black'>Thank you all so much for coming, and have a great night.</color>",
        "",
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        src.loop = false;
        MC.gameObject.SetActive(false);
        ClearGameplayUI();
        MC.recordingStarted += OnStartRecordUI;
        MC.recordingEnded += OnFinishRecordUI;
        MC.reRecordTimeout += OnTimeoutUI;
        BeginAnimation(0);
    }

    void OnStartRecordUI()
    {
        ClearGameplayUI();
        pnl_TimeRemaining.SetActive(true);
        src.Pause();
        StartRecordTimestamp = Time.timeSinceLevelLoad;
    }

    void OnFinishRecordUI()
    {
        ClearGameplayUI();
        pnl_Cut.SetActive(true);
        
        if (TotalRecordings == 1)
        {
            src.clip = snd_Lv2;
            src.Stop();
        }
        else if (TotalRecordings == 2)
        {
            src.clip = snd_Lv3;
            src.Stop();
        }
        else if (TotalRecordings == 3)
        {
            src.clip = snd_Lv4;
            src.Stop();
        }
        else if (TotalRecordings == 4)
        {
            src.clip = snd_Lv5;
            src.Stop();
        }

        src.Play();
    }
    void OnTimeoutUI()
    {
        ClearGameplayUI();
        pnl_PressAToStartRecording.SetActive(true);
        ++TotalRecordings;

        txt_TotalRecordings.text = $"{TotalRecordings} of 5 recordings";

        if (TotalRecordings == 4)
        {
            txt_TotalRecordings.text = $"{TotalRecordings} of 5 recordings\nOnly one left!";
        }

        if (TotalRecordings == 5)
        {
            GameState = 2;
            ClearGameplayUI();
            BeginAnimation(1);
        }
    }

    void ClearGameplayUI()
    {
        pnl_Slideshow.SetActive(false);
        pnl_Cut.SetActive(false);
        pnl_LookAtScreen.SetActive(false);
        pnl_PressAToStartRecording.SetActive(false);
        pnl_PutOnHeadset.SetActive(false);
        pnl_Slideshow.SetActive(false);
        pnl_TimeRemaining.SetActive(false);
    }

    public void BeginAnimation(int index)
    {
        src.loop = false;
        pnl_Slideshow.SetActive(true);
        AnimationPlaying = true;
        AnimationID = index;
        TextIndex = 0;

        if (AnimationID == 0)
        {
            src.clip = snd_IntroBGM;
            src.Stop();
            src.Play();
            txt_Caption.text = IntroStrings[TextIndex];
        }
        else if (AnimationID == 1)
        {
            src.clip = snd_OutroBGM;
            src.Stop();
            src.Play();
            txt_Caption.text = OutroStrings[TextIndex];
        }
        
    }

    public void EndAnimation()
    {
        AnimationPlaying = false;
        if (GameState == 0)
        {
            GameState = 1;
            // Enable the other UI stuff.
            src.loop = true;
            ClearGameplayUI();
            pnl_Slideshow.SetActive(false);
            pnl_PressAToStartRecording.SetActive(true);
            MC.gameObject.SetActive(true);
            src.clip = snd_Lv1;
            src.Stop();
            src.Play();
        }
    }

    void UpdateAnimation()
    {
        if (!AnimationPlaying) return;

        float now = src.time;

        if (AnimationID == 0)
        {
            float nextTimestamp = IntroTimestamps[TextIndex];
            if (now > nextTimestamp)
            {
                txt_Caption.text = IntroStrings[TextIndex];
                ++TextIndex;
                if (TextIndex >= IntroStrings.Length)
                {
                    EndAnimation();
                    return;
                }
            }
        }
        else if (AnimationID == 1)
        {
            float nextTimestamp = OutroTimestamps[TextIndex];
            if (now > nextTimestamp)
            {
                txt_Caption.text = OutroStrings[TextIndex];

                if (TextIndex == 3) // Flyaround Animation
                {
                    pnl_Slideshow.SetActive(false);
                    DesktopCam.SetParent(Stage_Orbit, false);
                }
                else if (TextIndex == 4) // Slides back again, no bg though.
                {
                    img_Bg.SetActive(false);
                    pnl_Slideshow.SetActive(true);
                }

                ++TextIndex;
                if (TextIndex >= OutroStrings.Length)
                {
                    EndAnimation();
                    return;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((GameState == 0) || (GameState == 2))
        {
            UpdateAnimation();
        }
        else
        {
            if (img_TimeProgress.gameObject.activeInHierarchy)
            {
                float now = Time.timeSinceLevelLoad;
                img_TimeProgress.fillAmount = 1 - ((now - StartRecordTimestamp) / 5f);
            }
        }
    }
}
