// -----------------------------------------------------------------------
// Copyright (c) Takashi Yoshinaga, [現在の年]. 
// All rights reserved.
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class MeshCopyScript : MonoBehaviour
{
    [SerializeField]
    GameObject _MeshObject;
    [SerializeField]
    Interactable _Button;
    MeshFilter _MeshFilter;
    MeshCollider _MeshCollider;
    // Start is called before the first frame update
    void Start()
    {
        GameObject meshObject = Instantiate(_MeshObject);
        _MeshFilter = meshObject.GetComponent<MeshFilter>();
        _MeshCollider = meshObject. GetComponent<MeshCollider>();
        _Button.OnClick.AddListener(() => {
            var list=GameObject.FindObjectsOfType<OVRSceneVolumeMeshFilter>();
            for(int i=0;i<list.Length;i++)
            {
                if(i==0)
                {
                    GameObject room=list[i].gameObject;
                    MeshFilter roomMeshFilter=room.GetComponent<MeshFilter>();
                    _MeshFilter.mesh = roomMeshFilter.mesh;
                    _MeshCollider.sharedMesh = roomMeshFilter.mesh;
                    meshObject.transform.rotation=room.transform.rotation;
                    meshObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    _Button.gameObject.SetActive(false);
                }
            } 
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
