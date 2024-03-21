using DreamNet;
using UnityEngine;
public class DreamMetaData : MonoBehaviour
{
    private void Start()
    {
        MetaData metaData = new MetaData();
        metaData["Test"].AsString = "Hi";
        print(metaData["Test"].AsString);
        print(metaData["Test2"]["Boos"].AsStringDef("boooooos"));
    }
}