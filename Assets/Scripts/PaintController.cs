using System;
using ImagePainting;
using UnityEngine;
using UnityEngine.UIElements;

public class PaintController : MonoBehaviour
{
    [SerializeField] private UIDocument _canvas;
    [SerializeField] private Texture2D texture;
    [SerializeField] private Brush _brush;
    
    private VisualElement _paintingContainer;
    private Vector2? _previousPoint = null;
    private bool _isDrawing = false;

    private void Awake()
    {
        _brush = new Brush();
        _paintingContainer = _canvas.rootVisualElement.Q("PaintingContainer");
    }

    private void Start()
    {
        LoadAndSetTextureOnContainer(_paintingContainer);
    }

    private void OnDisable()
    {
        _paintingContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        _paintingContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        _paintingContainer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnEnable()
    {
        _paintingContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _paintingContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
        _paintingContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void LoadAndSetTextureOnContainer(VisualElement paintingContainer)
    {
        NativeCamera.TakePicture((path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("path is not valid!");
                return;
            }

            texture = NativeCamera.LoadImageAtPath(path, markTextureNonReadable: false);
            paintingContainer.style.backgroundImage = texture;
        });
    }

    private void OnPointerUp(PointerUpEvent eventData)
    {
        _isDrawing = false;
        _previousPoint = null;
    }

    private void OnPointerDown(PointerDownEvent eventData)
    {
        _isDrawing = true;
        _previousPoint = null;
    }

    private void OnPointerMove(PointerMoveEvent eventData)
    {
        if (!_isDrawing) return;

        Vector2 localPosition = _paintingContainer.WorldToLocal(eventData.position);
        localPosition.x /= _paintingContainer.resolvedStyle.width;
        localPosition.y /= _paintingContainer.resolvedStyle.height;
        localPosition.x *= texture.width;
        localPosition.y *= texture.height;

        if (_previousPoint.HasValue)
            DrawOnTexture(_previousPoint.Value, localPosition);

        _previousPoint = localPosition;
    }

    private void SaveTexture(string path)
    {
        if (texture == null) return;

        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    private void DrawOnTexture(Vector2 start, Vector2 end)
    {
        int width = texture.width;
        int height = texture.height;

        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;

        for (int x = Mathf.Max(0, x0 - _brush.Size); x < Mathf.Min(width, x0 + _brush.Size); x++)
        {
            for (int y = Mathf.Max(0, y0 - _brush.Size); y < Mathf.Min(height, y0 + _brush.Size); y++)
            {
                int transformedY = height - y - 1;

                if (_brush.IsWithinBrushArea(x, transformedY, x0, height - y0 - 1))
                {
                    Color pixelColor = texture.GetPixel(x, transformedY);
                    Color newColor = Color.Lerp(pixelColor, _brush.Color, 10f);
                    texture.SetPixel(x, transformedY, newColor);
                }
            }
        }

        texture.Apply();
    }
}
