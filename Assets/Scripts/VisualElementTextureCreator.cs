using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ImagePainting
{
    public class VisualElementTextureCreator : MonoBehaviour
    {
        [SerializeField] private PanelSettings _elementPanelSettings;
        
        public IEnumerator Create(VisualElement element, Action<Texture2D> complited)
        {
            
            int width = (int)element.resolvedStyle.width;
            int height = (int)element.resolvedStyle.height;
            // Store the existing parent (if any).
            VisualElement _existingParent = element.parent;
         
            // Create a new UIDocumment, RenderTexture, and Panel to draw to.
            GameObject obj = new GameObject();
            UIDocument doc = obj.AddComponent<UIDocument>();
            doc.panelSettings = _elementPanelSettings;
            RenderTexture rt = new RenderTexture(width, height, 32);
            doc.panelSettings.targetTexture = rt;
            doc.panelSettings.referenceResolution = new Vector2Int(width, height);
            rt.Create();
            doc.rootVisualElement.Add(element);
            yield return null;
            // A frame later, we should have the RenderTexture fully rendered.
            // Create a texture and fill it in from the RenderTexture now that it's drawn.
            RenderTexture.active = rt;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.alphaIsTransparency = true;
            texture.Apply();
        
            yield return null;
            // Clean up the object we created and release the RenderTexture
            // (RenderTextures are not garbage collected objects).
            Object.DestroyImmediate(obj);
            rt.Release();
            // Restore the existing parent of the element, and invoke
            // any completion action specified.
            _existingParent?.Add(element);
            complited?.Invoke(texture);
        }

        
    }
}