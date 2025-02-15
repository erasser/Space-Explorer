using UnityEngine;
using UnityEngine.UI;

public class DrawGraph : MonoBehaviour
{
    static Texture2D _texture;
    static int _pixelPointer = 1;
    static int _halfHeight;
    static int _width;
    static int _height;

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
        _pixelPointer++;

        if (_pixelPointer == _texture.width - 1)
        {
            _pixelPointer = 1;
            Clear();
        }

        _texture.SetPixel(_pixelPointer, (int)(y * _halfHeight + _halfHeight), Color.cyan);

        _texture.Apply();
    }

    static void Clear()  // TODO: Dal by se z toho udělat background a pak použít texture copy
    {
        for (int y = 0; y < _texture.height; ++y)
            for (int x = 0; x < _texture.width; ++x)
                if (x == 0 || y == 0 || x == _texture.width - 1 || y == _texture.height - 1 || y == _texture.height / 2)
                    _texture.SetPixel(x, y, Color.magenta);
                else
                    _texture.SetPixel(x, y, new Color(0, 0, 0, .95f));

        _texture.Apply();
    }

}
