using UnityEngine;

public class FractalExplorer : MonoBehaviour
{
    [Header("References")]
    public Material fractalMaterial;

    [Header("Zoom")]
    public float zoom = 1f;
    public float zoomAcceleration = 8f;
    public float zoomDamping = 6f;
    public float minZoom = 0.000001f;
    public float maxZoom = 1000000f;

    [Header("Pan")]
    public float dragSensitivity = 1f;
    public float panSmooth = 12f;

    [Header("Iterations")]
    public int baseIterations = 100;
    public int maxIterations = 1000;

    [Header("Focus Dot")]
    public GameObject focusDotPrefab;
    private GameObject focusDotInstance;

    private Vector2 center = Vector2.zero;
    private float zoomVelocity = 0f;
    private Vector3 lastMousePos;
    private Vector2 panVelocity;

    // Main Mandelbrot largest bulb (cardioid)
    private Vector2 mainBulb = new Vector2(0.25f, 0f);

    void Update()
    {
        HandleZoomInput();
        HandlePan();
        ApplyZoomMomentum();
        ApplyPanSmoothing();
        UpdateShader();
        UpdateFocusDot();
    }

    #region Input & Movement
    void HandleZoomInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
            zoomVelocity += scroll * zoomAcceleration;
    }

    void ApplyZoomMomentum()
    {
        if (Mathf.Abs(zoomVelocity) < 0.001f)
            return;

        Vector2 mouse = Input.mousePosition;
        Vector2 uv = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);

        Vector2 before = ScreenToFractal(uv, zoom);
        float zoomFactor = Mathf.Exp(zoomVelocity * Time.deltaTime);
        zoom *= zoomFactor;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        Vector2 after = ScreenToFractal(uv, zoom);

        center += before - after;
        zoomVelocity = Mathf.Lerp(zoomVelocity, 0f, zoomDamping * Time.deltaTime);
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            float aspect = (float)Screen.width / Screen.height;

            Vector2 move = new Vector2(
                -delta.x / Screen.width * 4f * aspect / zoom,
                -delta.y / Screen.height * 4f / zoom
            );

            panVelocity = move * dragSensitivity / Time.deltaTime;
            lastMousePos = Input.mousePosition;
        }
    }

    void ApplyPanSmoothing()
    {
        center += panVelocity * Time.deltaTime;
        panVelocity = Vector2.Lerp(panVelocity, Vector2.zero, panSmooth * Time.deltaTime);
    }
    #endregion

    #region Shader & Fractal Mapping
    Vector2 ScreenToFractal(Vector2 uv, float currentZoom)
    {
        float aspect = (float)Screen.width / Screen.height;
        Vector2 pos = new Vector2(
            (uv.x - 0.5f) * 4f * aspect / currentZoom,
            (uv.y - 0.5f) * 4f / currentZoom
        );
        return center + pos;
    }

    void UpdateShader()
    {
        int iterations = Mathf.Clamp(
            baseIterations + Mathf.RoundToInt(Mathf.Log(zoom + 1f) * 50f),
            baseIterations,
            maxIterations
        );

        fractalMaterial.SetFloat("_Zoom", zoom);
        fractalMaterial.SetVector("_Center", center);
        fractalMaterial.SetInt("_Iterations", iterations);
    }
    #endregion

    #region Focus Dot
    // Given your Sprite has scale 18 in world units, centered at 0
    private Vector3 FractalToWorld(Vector2 fractalPos)
    {
        // Sprite scale in world units
        float spriteScale = 18f;

        // Shader uses 4 units in fractal space = full width of texture
        float scale = spriteScale / 4f;

        // Get offset in shader UV space
        Vector2 offset = fractalPos - center;

        // Apply zoom exactly like shader
        Vector2 worldOffset = offset * zoom * scale;

        return new Vector3(worldOffset.x, worldOffset.y, 0f);
    }

    private void UpdateFocusDot()
    {
        if (focusDotInstance == null && focusDotPrefab != null)
            focusDotInstance = Instantiate(focusDotPrefab);

        if (focusDotInstance != null)
        {
            // Keep the dot on the largest Mandelbrot bulb
            focusDotInstance.transform.position = FractalToWorld(mainBulb);
        }
    }
    #endregion
}