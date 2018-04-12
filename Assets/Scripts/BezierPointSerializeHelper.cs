using System.Collections;
using System.Collections.Generic;
using TasiYokan.Utilities.Serialization;
using UnityEngine;

public class BezierPointSerializeHelper : MonoBehaviour
{
    public BezierCurve curve;

    // Use this for initialization
    void Start()
    {
        JsonSerializationHelper.WriteJsonList<BezierPoint>(Application.dataPath+"/Datas/Data.json", curve.Points);

        Invoke("LoadData", 4);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadData()
    {
        List<BezierPoint> list = JsonSerializationHelper.ReadJsonList<BezierPoint>(Application.dataPath + "/Datas/Data.json");
        curve.Points = list;
        print("Data loaded!");
    }
}
