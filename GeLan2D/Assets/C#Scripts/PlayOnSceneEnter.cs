using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnSceneEnter : MonoBehaviour
{
    public AudioClip clip;

    void Start()
    {
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }
}
