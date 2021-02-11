using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    private Texture2D _texture;
    private Sprite _sprite;
    private SpriteRenderer _spriteRenderer;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _texture = new Texture2D(512, 512);
        _sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));

        for (int y = 0; y < _texture.height; y++)
        {
            for (int x = 0; x < _texture.width; x++) //Goes through each pixel
            {
                Color pixelColour;
                if (Random.Range(0, 2) == 1) //50/50 chance it will be black or white
                {
                    pixelColour = new Color(0, 0, 0, 1);
                }
                else
                {
                    pixelColour = new Color(1, 1, 1, 1);
                }

                _texture.SetPixel(x, y, pixelColour);
            }
        }

        _texture.Apply();
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Add sprite"))
        {
            _spriteRenderer.sprite = _sprite;
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 CurMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast(CurMousePos, Vector2.zero);
            if (hit2D.transform != null)
            {
                _texture.SetPixel((int) hit2D.point.x, (int) hit2D.point.y, Color.green);
                _texture.Apply();

                Sprite sprite = _spriteRenderer.sprite;
                Bounds bounds = _spriteRenderer.sprite.bounds;
                var pivotX = - bounds.center.x / bounds.extents.x / 2 + 0.5f;
                var pivotY = - bounds.center.y / bounds.extents.y / 2 + 0.5f;
                var pixelsToUnits = sprite.textureRect.width / bounds.size.x;
                Debug.Log("x = " + pivotX);
                Debug.Log("x = " + pivotY);
                Sprite sameSprite = Sprite.Create(sprite.texture,sprite.rect,
                    new Vector2(pivotX,pivotY),
                    pixelsToUnits);
            }
        }
    }
}
