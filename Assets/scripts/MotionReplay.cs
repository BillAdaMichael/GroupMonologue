using System.IO;
using System.Linq;
using UnityEngine;

public class MotionReplay : MonoBehaviour
{
    private string folderPath;

    // TODO: there's an error at application quit

    void Start()
    {
        folderPath = Path.Combine(Application.dataPath, "../testFolder");

        // Get all CSV files
        string[] files = Directory.GetFiles(folderPath, "*.csv");

        // Sort files by the number in their filename
        files = files.OrderBy(file =>
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                return int.Parse(filename);
            }).ToArray();

        Debug.Log($"Found {files.Length} CSV files.");

        // Process files in order
        for (int i = 0; i < files.Length; i++)
        {
            string contents = File.ReadAllText(files[i]);
            Debug.Log($"Loaded file {i}: {Path.GetFileName(files[i])}");

            ProcessCSV(contents);
        }
    }

    void ProcessCSV(string contents)
    {
        // Parse/process your CSV here
    }
}
