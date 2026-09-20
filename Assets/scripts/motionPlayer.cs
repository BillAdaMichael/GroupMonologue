using Unity.XR.CoreUtils;
using UnityEngine;

public class motionPlayer : MonoBehaviour
{
    public string[] motionValues;
    public GameObject headset, LController, RController;
    private float PassedTime = 0;
    private string[] closestRowsCache = new string[2], previousRowCache, nextRowCache;

    // line 0 is labels so ignore that

    //animation time,
    //headPosition x,headPosition y,headPosition z,
    //headRot (Quat) x,headRot (Quat) y,headRot (Quat) z,headRot (Quat) w,
    //RControllerPos x,RControllerPos y,RControllerPos z,
    //RControllerRot (Quat) x,RControllerRot (Quat) y,RControllerRot (Quat) z,RControllerRot (Quat) w,
    //LControllerPos x,LControllerPos y,LControllerPos z,
    //LControllerRot (Quat) x,LControllerRot (Quat) y,LControllerRot (Quat) z,LControllerRot (Quat) w,
    //modelID, texY

    // Update is called once per frame
    void Update()
    {
        // we have a 5 second long csv of coordinates but the timings aren't exactly matching
        // find the closest 2 rows to the PassedTime and use their lerped value as the transform values
        PassedTime += Time.deltaTime;
        PassedTime = PassedTime % 5f;

        closestRowsCache = closestRows(motionValues, PassedTime);
        previousRowCache = closestRowsCache[0].Split(',');
        nextRowCache = closestRowsCache[1].Split(',');

        Vector3 headPos = Vector3.Lerp(
            new Vector3(float.Parse(previousRowCache[1]), float.Parse(previousRowCache[2]), float.Parse(previousRowCache[3])),
            new Vector3(float.Parse(nextRowCache[1]), float.Parse(nextRowCache[2]), float.Parse(nextRowCache[3])),
            (PassedTime - float.Parse(previousRowCache[0])) / (float.Parse(nextRowCache[0]) - float.Parse(previousRowCache[0]))
        );
        Vector4 headRotation = Vector4.Lerp(
            new Vector4(float.Parse(previousRowCache[4]), float.Parse(previousRowCache[5]), float.Parse(previousRowCache[6]), float.Parse(previousRowCache[7])),
            new Vector4(float.Parse(nextRowCache[4]), float.Parse(nextRowCache[5]), float.Parse(nextRowCache[6]), float.Parse(nextRowCache[7])),
            (PassedTime - float.Parse(previousRowCache[0])) / (float.Parse(nextRowCache[0]) - float.Parse(previousRowCache[0]))
        );

        Vector3 rControllerPos = Vector3.Lerp(
            new Vector3(float.Parse(previousRowCache[8]), float.Parse(previousRowCache[9]), float.Parse(previousRowCache[10])),
            new Vector3(float.Parse(nextRowCache[8]), float.Parse(nextRowCache[9]), float.Parse(nextRowCache[10])),
            (PassedTime - float.Parse(previousRowCache[0])) / (float.Parse(nextRowCache[0]) - float.Parse(previousRowCache[0]))
        );
        Vector4 rControllerRotation = Vector4.Lerp(
            new Vector4(float.Parse(previousRowCache[11]), float.Parse(previousRowCache[12]), float.Parse(previousRowCache[13]), float.Parse(previousRowCache[14])),
            new Vector4(float.Parse(nextRowCache[11]), float.Parse(nextRowCache[12]), float.Parse(nextRowCache[13]), float.Parse(nextRowCache[14])),
            (PassedTime - float.Parse(previousRowCache[0])) / (float.Parse(nextRowCache[0]) - float.Parse(previousRowCache[0]))
        );

        Vector3 lControllerPos = Vector3.Lerp(
            new Vector3(float.Parse(previousRowCache[15]), float.Parse(previousRowCache[16]), float.Parse(previousRowCache[17])),
            new Vector3(float.Parse(nextRowCache[15]), float.Parse(nextRowCache[16]), float.Parse(nextRowCache[17])),
            (PassedTime - float.Parse(previousRowCache[0])) / (float.Parse(nextRowCache[0]) - float.Parse(previousRowCache[0]))
        );
        Vector4 lControllerRotation = Vector4.Lerp(
            new Vector4(float.Parse(previousRowCache[18]), float.Parse(previousRowCache[19]), float.Parse(previousRowCache[20]), float.Parse(previousRowCache[21])),
            new Vector4(float.Parse(nextRowCache[18]), float.Parse(nextRowCache[19]), float.Parse(nextRowCache[20]), float.Parse(nextRowCache[21])),
            (PassedTime - float.Parse(previousRowCache[0])) / (float.Parse(nextRowCache[0]) - float.Parse(previousRowCache[0]))
        );

        if (headPos.x.IsUndefined() || headPos.y.IsUndefined() || headPos.z.IsUndefined() ) return;

        headset.transform.position = headPos;
        headset.transform.rotation = new Quaternion(headRotation.x, headRotation.y, headRotation.z, headRotation.w);
        RController.transform.position = rControllerPos;
        RController.transform.rotation = new Quaternion(rControllerRotation.x, rControllerRotation.y, rControllerRotation.z, rControllerRotation.w);
        LController.transform.position = lControllerPos;
        LController.transform.rotation = new Quaternion(lControllerRotation.x, lControllerRotation.y, lControllerRotation.z, lControllerRotation.w);
    }

    string[] closestRows(string[] rows, float time)
    {
        string[] closest = new string[2];

        if (rows == null || rows.Length == 0)
            return closest;

        int firstDataIndex = 1; // skip header line

        if (rows.Length - firstDataIndex <= 0)
            return closest;

        // If there are only one or two data rows, return them (repeat last if needed)
        if (rows.Length - firstDataIndex <= 2)
        {
            int available = rows.Length - firstDataIndex;
            if (available >= 1) closest[0] = rows[firstDataIndex];
            if (available >= 2) closest[1] = rows[firstDataIndex + 1];
            if (available == 1) closest[1] = rows[firstDataIndex];
            return closest;
        }

        int prevIndex = -1;
        float prevTime = float.NegativeInfinity;
        int nextIndex = -1;
        float nextTime = float.PositiveInfinity;

        for (int i = firstDataIndex; i < rows.Length; i++)
        {
            string line = rows[i];
            string[] parts = line.Split(',');
            float rowTime = float.Parse(parts[0]);

            // candidate for previous with largest time
            if (rowTime <= time && rowTime > prevTime)
            {
                prevTime = rowTime;
                prevIndex = i;
            }

            // candidate for next with smallest time
            if (rowTime >= time && rowTime < nextTime)
            {
                nextTime = rowTime;
                nextIndex = i;
            }
        }

        // If no previous found, use first data
        if (prevIndex == -1)
            prevIndex = firstDataIndex;

        // If no next found, use last data
        if (nextIndex == -1)
            nextIndex = rows.Length - 1;

        closest[0] = rows[Mathf.Clamp(prevIndex, firstDataIndex, rows.Length - 1)];
        closest[1] = rows[Mathf.Clamp(nextIndex, firstDataIndex, rows.Length - 1)];
        return closest;
    }
}
