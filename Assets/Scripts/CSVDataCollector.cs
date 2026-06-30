using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVDataCollector : MonoBehaviour
{
    public static CSVDataCollector csvInstance;
    public int isDaysCorrect;
    public int colorsFalseCount;
    public int isOcrCorrect;
    public int isVoskCorrect;
    public int bigWarningCount;

    public int classOfStudent;
    // Start is called before the first frame update
    void Awake()
    {
        if (csvInstance != null && csvInstance != this)
            Destroy(gameObject);
        else
        {
            csvInstance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    public int ReceiveDaysResult(bool result)
    {
        isDaysCorrect = result ? 1 : 0;
        return isDaysCorrect;
    }
    public int ReceiveColorsResult(int result) 
    {
        colorsFalseCount = result;
        return colorsFalseCount;
    }
    public int ReceiveOCRResult(bool result)
    {
        isOcrCorrect = result ? 1 : 0;
        return isOcrCorrect;
    }
    public int ReceiveVoskResult(bool result)
    {
        isVoskCorrect = result ? 1 : 0;
        return isVoskCorrect;
    }
    public int ReceiveEyeTrackingResult(int result) 
    {
        bigWarningCount = result;
        return bigWarningCount;
    }
    public int ReceiveStudentsClass(int result)
    {
        classOfStudent = result;
        return classOfStudent;
    }
}
