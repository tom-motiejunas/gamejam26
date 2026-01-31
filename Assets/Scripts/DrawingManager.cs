using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DrawingManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Components")]
    public RawImage drawingArea;
    public Image feedbackFlash; // For red flash on failure
    
    [Header("Settings")]
    public int outputResolution = 32; // Comparisons happen at 32x32
    public int visualResolution = 256; // Drawing happens at 256x256 for smoothness
    public Color drawColor = Color.white;
    public Color backgroundColor = Color.clear;
    public int brushRadius = 6; // Increased from 2 to ensure visibility when downsampled
    
    [Header("Cursor Settings")]
    public Texture2D cursorTexture;
    public Vector2 cursorHotspot = new Vector2(16, 16); // Center of a 32x32 cursor roughly

    private Texture2D _drawingTexture;
    private RectTransform _rectTransform;

    void Start()
    {
        if (drawingArea == null) drawingArea = GetComponent<RawImage>();
        _rectTransform = drawingArea.transform as RectTransform;

        // Initialize high-res texture for visuals
        _drawingTexture = new Texture2D(visualResolution, visualResolution, TextureFormat.RGBA32, false);
        _drawingTexture.filterMode = FilterMode.Bilinear; 
        ClearCanvas();
        
        drawingArea.texture = _drawingTexture;
    }

    public void ClearCanvas()
    {
        Color[] colors = new Color[visualResolution * visualResolution];
        for (int i = 0; i < colors.Length; i++) colors[i] = backgroundColor;
        _drawingTexture.SetPixels(colors);
        _drawingTexture.Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ClearCanvas(); 
        DrawPoint(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DrawPoint(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameController.Instance.CheckDrawing(GetTexture());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void DrawPoint(Vector2 screenPos)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, null, out localPoint))
        {
            Rect rect = _rectTransform.rect;
            float u = (localPoint.x - rect.x) / rect.width;
            float v = (localPoint.y - rect.y) / rect.height;

            int px = Mathf.Clamp((int)(u * visualResolution), 0, visualResolution - 1);
            int py = Mathf.Clamp((int)(v * visualResolution), 0, visualResolution - 1);

            // Draw brush 
            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                for (int x = -brushRadius; x <= brushRadius; x++)
                {
                    int dx = px + x;
                    int dy = py + y;
                    if (dx >= 0 && dx < visualResolution && dy >= 0 && dy < visualResolution)
                    {
                         // Simple circle check for smoother brush
                         if (x*x + y*y <= brushRadius*brushRadius)
                            _drawingTexture.SetPixel(dx, dy, drawColor);
                    }
                }
            }
            _drawingTexture.Apply();
        }
    }

   /// <summary>
    /// Returns a copy of the drawing, auto-cropped and scaled to 32x32.
    /// This provides position and size invariance.
    /// </summary>
    public Texture2D GetTexture()
    {
        // 1. Find Bounds of the drawing
        int minX = visualResolution;
        int maxX = 0;
        int minY = visualResolution;
        int maxY = 0;

        bool hasContent = false;

        for (int y = 0; y < visualResolution; y++)
        {
            for (int x = 0; x < visualResolution; x++)
            {
                if (_drawingTexture.GetPixel(x, y).a > 0.1f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    hasContent = true;
                }
            }
        }

        Texture2D scaled = new Texture2D(outputResolution, outputResolution, TextureFormat.RGBA32, false);
        
        if (!hasContent)
        {
             // Rerturn empty
             Color[] clear = new Color[outputResolution * outputResolution];
             scaled.SetPixels(clear);
             scaled.Apply();
             return scaled;
        }

        // Add a small padding to the bounds to avoid cutting off edges effectively
        // scaling the drawing slightly smaller than full frame
        int padding = 2;
        minX = Mathf.Max(0, minX - padding);
        maxX = Mathf.Min(visualResolution - 1, maxX + padding);
        minY = Mathf.Max(0, minY - padding);
        maxY = Mathf.Min(visualResolution - 1, maxY + padding);

        int boundW = maxX - minX + 1;
        int boundH = maxY - minY + 1;

        // 2. Scale the Bounded Region to Output Resolution (32x32)
        // We use non-uniform scaling (stretch to fill) to match sprites which usually fill the square
        
        Color[] newPixels = new Color[outputResolution * outputResolution];

        for (int y = 0; y < outputResolution; y++)
        {
            for (int x = 0; x < outputResolution; x++)
            {
                // UV coordinates in the Output (0-1)
                float u = x / (float)(outputResolution - 1);
                float v = y / (float)(outputResolution - 1);

                // Map to Bounded Region in Source
                float srcX = minX + (u * (boundW - 1));
                float srcY = minY + (v * (boundH - 1));
                
                // Bilinear Sample the source at srcX, srcY
                newPixels[y * outputResolution + x] = _drawingTexture.GetPixelBilinear(srcX / visualResolution, srcY / visualResolution);
                
                // Reinforce Alpha for solidarity
                if (newPixels[y * outputResolution + x].a > 0.1f) 
                     newPixels[y * outputResolution + x] = drawColor;
                else
                     newPixels[y * outputResolution + x] = backgroundColor;
            }
        }
        
        scaled.SetPixels(newPixels);
        scaled.Apply();
        return scaled;
    }

    public void FlashError()
    {
        if (feedbackFlash != null)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        feedbackFlash.color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        feedbackFlash.color = Color.clear;
        yield return new WaitForSeconds(0.1f);
        feedbackFlash.color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        feedbackFlash.color = Color.clear;
    }
}
