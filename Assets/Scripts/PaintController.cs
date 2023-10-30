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
            paintingContainer.style.width = texture.width;
            paintingContainer.style.height = texture.height;
            paintingContainer.transform.scale = new Vector3(0.5f, 0.5f, 1);

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

        if (!_previousPoint.HasValue)
            DrawOnTexture(eventData.localPosition);
        else
            FillBetweenPoints(_previousPoint.Value, eventData.localPosition);

        _previousPoint = eventData.localPosition;
        ApplyNewTexture();
    }
    
    private void ApplyNewTexture() => texture.Apply();
    
    private void FillBetweenPoints(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);

        Vector2 currentPosition = end;

        float steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += steps)
        {
            currentPosition = Vector2.Lerp(start, end, lerp);
            DrawOnTexture(currentPosition);
        }
    }

    private void SaveTexture(string path)
    {
        if (texture == null) return;

        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    private void OnDestroy()
    {
        SaveTexture(@"C:\\test.png");
        Debug.Log(@"File saved to C:\\test.png");
    }

    private void DrawOnTexture(Vector2 start)
    {
        int width = texture.width;
        int height = texture.height;

        int x0 = (int)start.x;
        int y0 = (int)start.y;

        for (int x = Mathf.Max(0, x0 - _brush.Size); x < Mathf.Min(width, x0 + _brush.Size); x++)
        {
            for (int y = Mathf.Max(0, y0 - _brush.Size); y < Mathf.Min(height, y0 + _brush.Size); y++)
            {
                int transformedY = height - y - 1;

                if (_brush.IsWithinBrushArea(x, transformedY, x0, height - y0 - 1))
                {
                    Color pixelColor = texture.GetPixel(x, transformedY);
                    Color newColor = Color.Lerp(pixelColor, _brush.Color, _brush.Size);
                    texture.SetPixel(x, transformedY, newColor);
                }
            }
        }

    }
}
