using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class FractalAudioController : MonoBehaviour
{
    [Header("References")]
    public FractalExplorer fractal;
    public AudioMixer mixer;

    [Header("Audio")]
    public AudioSource source;

    [Header("Dynamics")]
    public float smoothing = 5f;

    [Header("Pitch")]
    public float minPitch = -6f;
    public float maxPitch = 6f;

    [Header("Distortion")]
    public float maxDistortion = 1f;

    private bool active = false;

    void Start()
    {
        source.loop = true;
        source.playOnAwake = false;

        mixer.SetFloat("MasterVolume", -80f);
    }

    void Update()
    {
        HandleInput();

        if (!active)
            return;

        UpdateFractalAudio();
    }

    //----------------------------------------
    // INPUT
    //----------------------------------------

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!source.isPlaying)
                source.Play();

            active = true;

            mixer.SetFloat("MasterVolume", 20f);
        }

        if (Input.GetMouseButtonUp(0))
        {
            active = false;

            mixer.SetFloat("MasterVolume", -80f);
        }

    }

    //----------------------------------------
    // FRACTAL AUDIO
    //----------------------------------------

    void UpdateFractalAudio()
    {
        //----------------------------------------
        // POSITION FRACTALE RÉELLE
        //----------------------------------------

        Vector2 mouseUV = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );

        Vector2 fractalPos =
            fractal.ScreenToFractal(mouseUV, fractal.zoom);

        //----------------------------------------
        // LOG SCALE
        //----------------------------------------

        float zoomLog =
            Mathf.Log10(fractal.zoom + 1f);

        //----------------------------------------
        // DISTANCE AU CENTRE
        //----------------------------------------

        float radius =
            fractalPos.magnitude;

        // Compression logarithmique
        float radiusLog =
            Mathf.Log(radius + 1f);

        //----------------------------------------
        // NORMALISATION
        //----------------------------------------

        float xNorm =
            Mathf.InverseLerp(-2f, 2f, fractalPos.x);

        float yNorm =
            Mathf.InverseLerp(-2f, 2f, fractalPos.y);

        float zoomNorm =
            Mathf.InverseLerp(0f, 6f, zoomLog);

        float radiusNorm =
            Mathf.InverseLerp(0f, 5f, radiusLog);

        //----------------------------------------
        // LOW PASS
        //----------------------------------------

        float lowPass =
            Mathf.Lerp(300f, 22000f, xNorm);

        mixer.SetFloat(
            "LowPassCutoff",
            SmoothMixer("LowPassCutoff", lowPass)
        );

        //----------------------------------------
        // HIGH PASS
        //----------------------------------------

        float highPass =
            Mathf.Lerp(10f, 6000f, yNorm);

        mixer.SetFloat(
            "HighPassCutoff",
            SmoothMixer("HighPassCutoff", highPass)
        );

        //----------------------------------------
        // DISTORTION
        //----------------------------------------

        float distortion =
            radiusNorm * maxDistortion;

        mixer.SetFloat(
            "DistortionLevel",
            SmoothMixer(
                "DistortionLevel",
                distortion
            )
        );

        //----------------------------------------
        // PITCH
        //----------------------------------------

        float pitch =
            Mathf.Lerp(
                minPitch,
                maxPitch,
                zoomNorm
            );
        float pitchNormalized =
    Mathf.InverseLerp(minPitch, maxPitch, pitch);

        lowPass *= Mathf.Lerp(
            1f,
            0.6f,
            pitchNormalized
        );

        mixer.SetFloat(
            "MasterPitch",
            SmoothMixer("MasterPitch", pitchNormalized)
        );

        //----------------------------------------
        // REVERB
        //----------------------------------------

        float reverb =
            Mathf.Lerp(
                -5000f,
                2000f,
                Mathf.PerlinNoise(
                    fractalPos.x * 0.2f,
                    fractalPos.y * 0.2f
                )
            );

        mixer.SetFloat(
            "ReverbLevel",
            SmoothMixer("ReverbLevel", reverb)
        );
    }

    //----------------------------------------
    // SMOOTHING
    //----------------------------------------

    float SmoothMixer(string parameter, float target)
    {
        float current;
        mixer.GetFloat(parameter, out current);

        return Mathf.Lerp(
            current,
            target,
            Time.deltaTime * smoothing
        );
    }
}