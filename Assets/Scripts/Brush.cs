using System;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace ImagePainting
{
    [Serializable]
    public class Brush
    {
        [field:SerializeField] public int Size { get; private set; } = 10;
        [field:SerializeField] public Color Color { get; private set; } = Color.red;

        public void SetColor(Color color)
        {
            Color = color;
        }

        public void SetColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                SetColor(color);
            }

            SetColor(Color.red);
        }
        
        public void SetSize(int size)
        {
            if(size<=0)
                return;
            
            Size = size;
        }
        
        public virtual bool IsWithinBrushArea(int x, int y, int x0, int y0)
        {
            return Mathf.Pow(x - x0, 2) + Mathf.Pow(y - y0, 2) <= Mathf.Pow(Size, 2);
        }
        
    }
}