using UnityEngine;

public class TargetRecorder : MonoBehaviour
{
    public string fileName;

    private void FixedUpdate()
    {
        Recorder.LogData(fileName, Time.fixedTime, transform.localPosition.y - 0.5f);
    }
}