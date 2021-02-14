using HSVPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Painter
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Painter : MonoBehaviour
    {
        [SerializeField] private ColorPicker picker;
        [SerializeField] private Slider sizeBrush;
        private SpriteRenderer _spriteRenderer;
        private Camera _camera;
        private Sprite _updatedSprite;
        private Transform _transform;
        private Sprite _spriteForDrawing;
        private Color _colorBrash;

        private float _pixelsPerUnit;
        private readonly DataSave _dataSave = new DataSave();
        private Plane _plane;
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _camera = Camera.main;
            _transform = GetComponent<Transform>();
            _pixelsPerUnit = _spriteRenderer.sprite.pixelsPerUnit;
            _spriteForDrawing = CreateNewSprite(_spriteRenderer);
            _spriteRenderer.sprite = _spriteForDrawing;
            
            _plane = new Plane(transform.forward, _transform.position);
            
            picker.onValueChanged.AddListener(color =>
            {
                _colorBrash = color;
            });
            _colorBrash = picker.CurrentColor;
            
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 viewportPos = _camera.ScreenToViewportPoint(Input.mousePosition);

                Ray ray = _camera.ViewportPointToRay(viewportPos);
                if (_spriteRenderer.sprite == null) return;

                Texture2D newTexture = _spriteRenderer.sprite.texture;
                
                float rayIntersectDist; 
                if(!_plane.Raycast(ray, out rayIntersectDist)) return; 
                
                Vector3 spritePos = _spriteRenderer.worldToLocalMatrix.MultiplyPoint3x4(ray.origin + (ray.direction * rayIntersectDist));
                float halfRealTexWidth = newTexture.width * 0.5f; 
                float halfRealTexHeight = newTexture.height * 0.5f;
                
                int texPosX = (int)(spritePos.x * _pixelsPerUnit + halfRealTexWidth);
                int texPosY = (int)(spritePos.y * _pixelsPerUnit + halfRealTexHeight);
                
                Draw(newTexture, texPosX, texPosY, (int)sizeBrush.value, _colorBrash);
                
                newTexture.Apply();
                _spriteRenderer.sprite = Sprite.Create (newTexture, new Rect (0, 0, newTexture.width, newTexture.height), new Vector2 (0.5f, 0.5f));
            }
        }
        
        public void ClearSprite()
        {
            Texture2D newTex = _spriteRenderer.sprite.texture;
            Color[] defColors = new Color[newTex.width * newTex.height];
            for (int i = 0; i < defColors.Length; i++)
            {
                defColors[i] = Color.white;
            }
            newTex.SetPixels(defColors);
            newTex.Apply();
            _spriteRenderer.sprite = Sprite.Create(newTex, new Rect (0, 0, newTex.width, newTex.height), new Vector2 (0.5f, 0.5f));
        }

        public Sprite CreateNewSprite(SpriteRenderer spriteRenderer)
        {
            Texture2D newTex = new Texture2D(spriteRenderer.sprite.texture.width, spriteRenderer.sprite.texture.height);
            Color[] defColors = spriteRenderer.sprite.texture.GetPixels();
            newTex.SetPixels(defColors);
            newTex.Apply();
            var sprite = Sprite.Create(newTex, new Rect (0, 0, newTex.width, newTex.height), new Vector2 (0.5f, 0.5f));
            sprite.texture.filterMode = FilterMode.Point;
            return sprite;
        }

        public void SaveSprite(string nameFile)
        {
            _dataSave.SaveTextureToFile(_spriteRenderer.sprite.texture, nameFile + ".png");
        }

        private void Draw(Texture2D texture, int posX, int posY, int size, Color color)
        {
            var halfSize = size / 2;
            for (int i = -halfSize; i <= halfSize; i++)
            {
                for (int j = -halfSize; j <= halfSize; j++)
                {
                    texture.SetPixel(posX + i, posY + j, color);
                }
            }
        }
    }
}