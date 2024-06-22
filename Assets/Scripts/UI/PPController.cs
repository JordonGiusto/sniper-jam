using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PPController : MonoBehaviour
{

    public static PPController Singleton;
    public Volume volume;
    public Vignette vignette;
    // Start is called before the first frame update
    void Start()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out Vignette _vignette);
        vignette = _vignette;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
