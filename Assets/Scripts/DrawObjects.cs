using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DrawObjects : MonoBehaviour {

	public Camera drawCamera; // камера, которая будет делать снимок нарисованной формы
	public SpriteRenderer brush; // спрайт кисточки
	public Color brushColor = Color.blue; // цвет кисти
	[Range(0.1f, 1f)] public float brushSize = 0.5f; // размер кисточки
	public int maxDrawCount = 100; // сколько объектов, то есть кисточек можно создать за раз (чернила)
	public string planeTag = "GameController"; // тег холста/фона на котором можно рисовать
	public int maxObjects = 10; // сколько фигур можно создать
	public Slider slider; // вывод текущего статуса для чернил

	private int drawCounter, shapeCounter;
	private float[] posX;
	private float[] posY;
	private bool stop;
	private Vector3 mousePosLast;
	private List<SpriteRenderer> shape = new List<SpriteRenderer>();

	void Start()
	{
		posX = new float[maxDrawCount];
		posY = new float[maxDrawCount];
	}

	void Update()
	{
		if(Input.GetMouseButton(0) && !stop && shapeCounter < maxObjects)
		{
			Draw();
		}
		else if(Input.GetMouseButtonUp(0) && shapeCounter < maxObjects)
		{
			StartCoroutine(Create());
		}
		else if(Input.GetMouseButtonDown(1)) // удаление объектов
		{
			foreach(SpriteRenderer e in shape) 
			{
				Destroy(e.sprite.texture);
				Destroy(e.gameObject);
			}

			shapeCounter = 0;
			shape = new List<SpriteRenderer>();
		}

		slider.value = 1f - (float)drawCounter/(float)maxDrawCount;
		mousePosLast = Input.mousePosition;
	}

	void Clear() // удаление клонов кисточки и очистка массивов
	{
		drawCounter = 0;
		foreach(Transform child in transform) 
		{
			Destroy(child.gameObject);
		}
		posX = new float[maxDrawCount];
		posY = new float[maxDrawCount];
		stop = false;
	}

	float[] ReArray(float[] arr) // убираем из массива нули
	{
		int k = 0;
		foreach(float e in arr) 
		{
			if(e == 0) k++;
		}
		float[] tmp = new float[arr.Length - k];
		for(int i = 0, j = 0; i != arr.Length; i++)
		{
			if (arr[i] != 0)
			{
				tmp[j] = arr[i];
				j++;
			}
		}
		return arr = tmp;
	}

	IEnumerator Create() // создание фигуры
	{
		posX = ReArray(posX);
		posY = ReArray(posY);

		Vector3 posStart = Camera.main.WorldToScreenPoint(new Vector3(brush.bounds.min.x, brush.bounds.min.y, brush.bounds.min.z));
		Vector3 posEnd = Camera.main.WorldToScreenPoint(new Vector3(brush.bounds.max.x, brush.bounds.max.y, brush.bounds.min.z));

		int widthX = (int)(posEnd.x - posStart.x);
		int widthY = (int)(posEnd.y - posStart.y);

		float minX = Mathf.Min(posX);
		float maxX = Mathf.Max(posX);
		float minY = Mathf.Min(posY);
		float maxY = Mathf.Max(posY);

		minX -= widthX;
		maxX += widthX;
		minY -= widthY;
		maxY += widthY;

		int width = (int)(maxX - minX);
		int height = (int)(maxY - minY);

		yield return new WaitForEndOfFrame();

		Rect rect = new Rect(0, 0, width, height);
		rect.center = new Vector2(minX + width/2, minY + height/2);
		Vector3 position = Camera.main.ScreenToWorldPoint(rect.center);

		drawCamera.transform.position = position;
		drawCamera.orthographicSize = ((float)height / (Mathf.Rad2Deg * ((Screen.height / Mathf.Rad2Deg) / Camera.main.orthographicSize)));
		Destroy(drawCamera.targetTexture);
		drawCamera.targetTexture = new RenderTexture(width, height, 24);
		drawCamera.Render();
		RenderTexture.active = drawCamera.targetTexture;
		Texture2D tex = new Texture2D(drawCamera.targetTexture.width, drawCamera.targetTexture.height, TextureFormat.ARGB32, false);
		tex.ReadPixels(new Rect(0, 0, drawCamera.targetTexture.width, drawCamera.targetTexture.height), 0, 0);
		tex.Apply();
		RenderTexture.active = null;

		GameObject obj = new GameObject("Object: " + shapeCounter);
		obj.transform.position = new Vector3(position.x, position.y, 0);
		SpriteRenderer ren = obj.AddComponent<SpriteRenderer>();
		int unit = Mathf.RoundToInt((float)Screen.height/(Camera.main.orthographicSize * 2f));
		ren.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), unit);
		obj.AddComponent<PolygonCollider2D>();
		obj.AddComponent<Rigidbody2D>().useAutoMass = true;
		Clear();

		shape.Add(ren);
		shapeCounter++;
	}

	void InstBrush(Vector3 position)
	{
		SpriteRenderer m = Instantiate(brush) as SpriteRenderer;
		m.color = brushColor;
		m.transform.position = new Vector2(position.x, position.y);
		m.transform.parent = transform;
	}

	void Fill(Vector3 last, Vector3 current) // заполняем пробелы, между двумя точками (рывки мышки)
	{
		float step = 0.1f;
		bool result = true;

		while(result)
		{
			last = Vector3.MoveTowards(last, current, step);
			InstBrush(last);

			if(last == current || shapeCounter >= maxObjects)
			{
				Vector3 lastPos = Camera.main.WorldToScreenPoint(last);
				posX[posX.Length - 1] = lastPos.x;
				posY[posX.Length - 1] = lastPos.y;
				result = false;
			}
			else
			{
				drawCounter++;
			}
		}
	}

	void Draw() // рисование, клонирование кисточки
	{
		if(mousePosLast == Input.mousePosition) return;

		brush.transform.localScale = Vector3.one * brushSize;
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if(hit.transform && hit.transform.tag == planeTag)
		{
			InstBrush(hit.point);

			if(Vector2.Distance(mousePosLast, Input.mousePosition) > 10)
			{
				Fill(Camera.main.ScreenToWorldPoint(mousePosLast), Camera.main.ScreenToWorldPoint(Input.mousePosition));
			}

			if(drawCounter >= maxDrawCount)
			{
				stop = true;
			}
			else
			{
				posX[drawCounter] = Input.mousePosition.x;
				posY[drawCounter] = Input.mousePosition.y;
			}

			drawCounter++;
		}
	}
}