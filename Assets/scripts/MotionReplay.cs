using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MotionReplay : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    private string folderPath;
    private string[] files;
    private List<GameObject> pastPlayers =  new List<GameObject>();

    void Start()
    {
        folderPath = Path.Combine(Application.dataPath, "../testFolder");

        files = getFiles();
        Debug.Log($"Found {files.Length} CSV files.");

        // Process files in order
        for (int i = 0; i < files.Length; i++)
        {
            string[] contents = File.ReadAllLines(files[i]);
            Debug.Log($"Loaded file {i}: {Path.GetFileName(files[i])}");

            updateGameObject(contents);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < pastPlayers.Count; i++)
        {
            Destroy(pastPlayers[i]);
        }
        pastPlayers.Clear();

        files = getFiles();
        Debug.Log($"Found {files.Length} CSV files.");

        // Process files in order
        for (int i = 0; i < files.Length; i++)
        {
            string[] contents = File.ReadAllLines(files[i]);
            Debug.Log($"Loaded file {i}: {Path.GetFileName(files[i])}");

            updateGameObject(contents);
        }
    }

    private string[] getFiles()
    {
        // Get all CSV files
        string[] _files = Directory.GetFiles(folderPath, "*.csv");

        // Sort files by the number in their filename
        return _files.OrderBy(file =>
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            return int.Parse(filename);
        }).ToArray();
    }

    //give each player object a position updater script
    // this spawns a player object, and passes its script the coordinate data (all the "contents").
    // line 0 is labels so ignore that

    //animation time,
    //headPosition x,headPosition y,headPosition z,
    //headRot (Quat) x,headRot (Quat) y,headRot (Quat) z,headRot (Quat) w,
    //RControllerPos x,RControllerPos y,RControllerPos z,
    //RControllerRot (Quat) x,RControllerRot (Quat) y,RControllerRot (Quat) z,RControllerRot (Quat) w,
    //LControllerPos x,LControllerPos y,LControllerPos z,
    //LControllerRot (Quat) x,LControllerRot (Quat) y,LControllerRot (Quat) z,LControllerRot (Quat) w,
    //modelID, texY


    void updateGameObject(string[] contents)
    {
        string[] values = contents[1].Split(',');
        GameObject oldPlayerContainer = Instantiate(playerPrefabs[int.Parse(values[22])]); // spawn a player model matching the one in the save file
        motionPlayer mp = oldPlayerContainer.GetComponent<motionPlayer>();
        Renderer r = mp.headset.GetComponent<Renderer>();
        Material mat = new Material(r.material);
        r.material = mat;
        mat.mainTextureOffset = new Vector2(0, float.Parse(values[23]));
        oldPlayerContainer.GetComponent<motionPlayer>().motionValues = contents; // pass the csv values to get replayed
        pastPlayers.Add(oldPlayerContainer); // save in a list to delete later
    }
}
