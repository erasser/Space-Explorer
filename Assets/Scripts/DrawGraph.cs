using UnityEngine;
using UnityEngine.UI;

public class DrawGraph : MonoBehaviour
{
    static Texture2D _texture;
    static int _pixelPointer = 1;
    static int _halfHeight;
    static int _width;
    static int _height;
    static Vector2 _lastPoint;

    void Start()
    {
        var image = GetComponent<Image>();
        image.color = Color.white;
        _width = (int)image.rectTransform.rect.width;
        _height = (int)image.rectTransform.rect.height;
        _halfHeight = _height / 2;

        _texture = new(_width, _height);
        image.material.mainTexture = _texture;

        Clear();
    }

    public static void DrawPoint(float y)
    {
        IncrementPointer();

        _texture.SetPixel(_pixelPointer, (int)(y * _halfHeight + _halfHeight), Color.cyan);

        // _lastPoint = new(_pixelPointer, (int)(y * _halfHeight + _halfHeight));

        _texture.Apply();
    }

    public static void DrawPointAbsolute(float y)
    {
        IncrementPointer();

        _texture.SetPixel(_pixelPointer, (int)(y + _halfHeight), Color.red);

        _texture.Apply();
    }

    public static void DrawPoints(float y1, float y2)
    {
        IncrementPointer();

        if (_pixelPointer % 2 == 0)  // solves overdrawing
        {
            _texture.SetPixel(_pixelPointer, (int)(y1 * _halfHeight + _halfHeight), Color.cyan);
            _texture.SetPixel(_pixelPointer, (int)(y2 * _halfHeight + _halfHeight), Color.yellow);
        }
        else
        {
            _texture.SetPixel(_pixelPointer, (int)(y2 * _halfHeight + _halfHeight), Color.yellow);
            _texture.SetPixel(_pixelPointer, (int)(y1 * _halfHeight + _halfHeight), Color.cyan);
        }

        _texture.Apply();
        
        // if ((int)(y1 * _halfHeight + _halfHeight) == (int)(y2 * _halfHeight + _halfHeight))
        //     print("overdrawing!");
    }

    static void IncrementPointer()
    {
        _pixelPointer++;

        if (_pixelPointer == _width - 1)
        {
            _pixelPointer = 1;
            Clear();
        }
    }

    static void Clear()  // TODO: Dal by se z toho udělat background a pak použít texture copy
    {
        for (int y = 0; y < _height; ++y)
            for (int x = 0; x < _width; ++x)
                if (x == 0 || y == 0 || x == _width - 1 || y == _height - 1 || y == _halfHeight)
                    _texture.SetPixel(x, y, Color.magenta);
                else
                    _texture.SetPixel(x, y, new Color(0, 0, 0, .95f));

        _texture.Apply();
    }

    void DrawLine(Texture2D texture, Vector2 p1, Vector2 p2, Color color)
    {
        Vector2 t = p1;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
        {
            t = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            texture.SetPixel((int)t.x, (int)t.y, color);
        }
    }

}
