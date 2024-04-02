using System;
using System.Collections.Generic;
using DreamNet;
using UnityEngine;
public class DreamMetaDataTest : MonoBehaviour
{
    private void Start()
    {
        MetaData metaData = new MetaData("Root",null);
        metaData["Test2"]["Boos"]["Nader"].AsString = "hi";
        List<string> address = metaData["Test2"]["Boos"]["Nader"].ModifiedAddress();

        MetaData test=new MetaData("Root",null);
        
        foreach (var item in address)
        {
            test = test[item];
        }

        test.AsString = "hi";
        print("test"+test.AsString);
    }

    public void OnUpdate(MetaData metaData)
    {
        print("meta updated :  "+metaData.AsString);
    }
}