using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using ImagePainting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Task = UnityEditor.VersionControl.Task;

[RequireComponent(typeof(VisualElementTextureCreator))]
public class PaintController : MonoBehaviour
{
    [SerializeField] private UIDocument _canvas;
    [SerializeField] private VisualElementTextureCreator _visualElementTextureCreator;
    [SerializeField] private Brush _brush;
    
    private VisualElement _paintingContainer;
    private Vector2? _previousPoint = null;
    private bool _isDrawing = false;
    private Texture2D _baseTexture;
    private Texture2D _savedTexture;
    
    private void Awake()
    {
        _brush = new Brush();
        _paintingContainer = _canvas.rootVisualElement.Q("PaintingContainer");
        _visualElementTextureCreator = GetComponent<VisualElementTextureCreator>();
    }

    private void Start()
    {
        LoadAndSetTextureOnContainer(_paintingContainer);
    }
   
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {  
            //Print path for test save
           SaveTexture(@"");
        }
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

            _baseTexture = NativeCamera.LoadImageAtPath(path, markTextureNonReadable: false);
            paintingContainer.style.backgroundImage = _baseTexture;
            paintingContainer.style.width = _baseTexture.width;
            paintingContainer.style.height = _baseTexture.height;
            paintingContainer.transform.scale = new Vector3(0.1f, 0.1f, 1);
            SetDateMarkUp();

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
    
    private void ApplyNewTexture() => _baseTexture.Apply();
    
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
        if (_baseTexture == null) return;
        
        VisualElement dateMarkUp = _paintingContainer.Q("DateMarkUp");
        StartCoroutine(_visualElementTextureCreator.Create(dateMarkUp, (textureByElement) =>
        {
            _savedTexture = _baseTexture.AddWatermark(textureByElement);
            byte[] bytes = _savedTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
        }));
    }

    private void SetDateMarkUp(string date = null)
    {
        Label dateMarkUp = _paintingContainer.Q<Label>("DateMarkUp");
        dateMarkUp.text = date ?? DateTime.Now.ToShortDateString();
    }
  
    private void DrawOnTexture(Vector2 start)
    {
        int width = _baseTexture.width;
        int height = _baseTexture.height;

        int x0 = (int)start.x;
        int y0 = (int)start.y;

        for (int x = Mathf.Max(0, x0 - _brush.Size); x < Mathf.Min(width, x0 + _brush.Size); x++)
        {
            for (int y = Mathf.Max(0, y0 - _brush.Size); y < Mathf.Min(height, y0 + _brush.Size); y++)
            {
                int transformedY = height - y - 1;

                if (_brush.IsWithinBrushArea(x, transformedY, x0, height - y0 - 1))
                {
                    Color pixelColor = _baseTexture.GetPixel(x, transformedY);
                    Color newColor = Color.Lerp(pixelColor, _brush.Color, _brush.Size);
                    _baseTexture.SetPixel(x, transformedY, newColor);
                }
            }
        }

    }
}
