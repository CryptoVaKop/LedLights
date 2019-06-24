using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Простой ColorPicker. Перемещая маркер по цветному кругу можно выбрать цвет. 
/// Перемещая ползунок регулировки яркости можно установить яркость выбранного цвета.
/// </summary>


public class ColorPicker : MonoBehaviour, ITouchListener
{
    class SliderControl
    {
        public Slider Slider;
        public Image Background;
        public Image Fill;
        public Image Handle;

        public SliderControl(Transform parent, string name)
        {
            Transform sliderTransform = parent.Find(name);
            Slider = sliderTransform.GetComponent<Slider>();
            Background = sliderTransform.Find("Background").GetComponent<Image>();
            Fill = sliderTransform.Find("Fill Area").Find("Fill").GetComponent<Image>();
            Handle = sliderTransform.Find("Handle Slide Area").Find("Handle").GetComponent<Image>();

        }

        public Color Color
        {
            get { return Fill.color; }
            set { Background.color = Fill.color = Handle.color = value; }
        }
    }


    /// <summary>
    /// Цветной круг для выбора цвета.
    /// </summary>
    public RectTransform ColorCircle;

    /// <summary>
    /// Маркер, перемещая который можно выбирать цвет.
    /// </summary>
    public RectTransform PickerTransform;
    public Image Picker;

    /// <summary>
    /// Регулировка яркости цвета.
    /// </summary>
    private SliderControl Brightness;

    /// <summary>
    /// Регулировка насыщенности цвета.
    /// </summary>

    private SliderControl Saturation;

    private Image SelectedColor;


    /// <summary>
    /// Радиус цветного круга.
    /// </summary>
    private float CircleRadius;

    private float CircleThink = 50.0f;

    /// <summary>
    /// Радиус маркера.
    /// </summary>
    private float PickerRadius;

    /// <summary>
    /// Массив с координатами точек R, G и B.
    /// </summary>
    private Vector2[] RGBpoints = new Vector2[3];

    /// <summary>
    /// Ссылка на массив под угловые расстояния заданной точки до точек R, G и B соответственно.
    /// </summary>
    private float[] Angles;


    Vector2 TouchToPicker;


    /// <summary>
    /// Получить текущий цвет.
    /// </summary>
    /// <returns> Текущий цвет. </returns>
    public Color Color { get { return SelectedColor.color; } }



    /// <summary>
    /// Рассчет цвета пикселя в зависимости от координат его центра. 
    /// </summary>
    /// <param name="pixelPos"> Координаты центр пикселя относительно центра закрашиваемого круга. </param>
    /// <returns> Рассчитанный цвет пикселя. </returns>
    private Color CalcColor(Vector2 pixelPos)
    {
        Color color = Color.black;

        // Рассчитать угловые расстояния от центра пикселя до точек R, G и B соответственно
        for (int n = 0; n < Angles.Length; n++)
        {
            Angles[n] = Vector2.Angle(pixelPos, RGBpoints[n]);
        }

        // Найти минимальное, максмальное и среднее угловые расстояния
        float angleMin = Mathf.Min(Angles);
        float angleMax = Mathf.Max(Angles);
        float angleMed = 0;
        for (int n = 0; n < Angles.Length; n++)
        {
            if ((Angles[n] != angleMin) && (Angles[n] != angleMax))
            {
                angleMed = Angles[n];
            }
        }

        // В зависимости от углвых расстояни рассчитать цвет пикселя
        for (int n = 0; n < Angles.Length; n++)
        {
            if (Angles[n] == angleMin)
            {
                color[n] = 1.0f;
            }
            else if (Angles[n] == angleMax)
            {
                color[n] = 0.0f;
            }
            else
            {
                color[n] = 2 * angleMin / (angleMin + angleMed);
            }
        }

        return color;
    }


    // Start is called before the first frame update
    void Start()
    {
        // Инициализировать необходимые компоненты цветного круга 
        ColorCircle = transform.GetChild(0) as RectTransform;

        // Инициализировать необходимые компоненты маркера
        PickerTransform = ColorCircle.GetChild(0) as RectTransform;
        Picker = PickerTransform.GetComponent<Image>();

        // Создать регулировку яркости
        Brightness = new SliderControl(transform, "BrightnessSlider");

        // Создать регулировку насыщенности
        Saturation = new SliderControl(transform, "SaturationSlider");

        SelectedColor = transform.Find("SelectedColor").GetComponent<Image>();

        // Инициализировать радиусы цветного круга и маркера
        CircleRadius = 0.5f * ColorCircle.sizeDelta.x;
        PickerRadius = 0.5f * PickerTransform.sizeDelta.x;

        // Кватернион поворота на 120 градусов по часовой стрелке вокруг оси Z
        Quaternion rotateQuaternion = Quaternion.Euler(0, 0, -120);

        // Инициализировать массив с координатами точек R, G и B 
        RGBpoints[0] = new Vector2(0, CircleRadius);
        for (int p = 1; p < RGBpoints.Length; p++)
        {
            RGBpoints[p] = rotateQuaternion * RGBpoints[p - 1];
        }

        // Создать массив под угловые расстояния заданной точки до точек R, G и B
        Angles = new float[RGBpoints.Length];

        // Обновить текущий цвет маркера
        Picker.color = CalcColor(PickerTransform.anchoredPosition);

        // Обновить текущий цвет регулировки насыщенности
        Saturation.Color = Color.Lerp(Color.white, Picker.color, Saturation.Slider.value);

        // Обновить текущий цвет регулировки яркости
        Brightness.Color = SelectedColor.color = Color.Lerp(Color.black, Saturation.Color, Brightness.Slider.value);

        // При изменении насыщенности - обновлять цвет
        Saturation.Slider.onValueChanged.AddListener((float value) =>
        {
            Saturation.Color = Color.Lerp(Color.white, Picker.color, Saturation.Slider.value);
            Brightness.Color = SelectedColor.color = Color.Lerp(Color.black, Saturation.Color, Brightness.Slider.value);
        });

        // При изменении яркости - обновлять цвет
        Brightness.Slider.onValueChanged.AddListener((float value) =>
        {
            Brightness.Color = SelectedColor.color = Color.Lerp(Color.black, Saturation.Color, Brightness.Slider.value);
        });

        // Зарегистрироваться в TouchController-е, чтобы отслеживать перемещения пальцев или мыши по экрану
        TouchController.RegListener(this);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            // Выйти из программы
            Application.Quit();
        }

        // Дать поработать TouchController-у
        TouchController.Update();
    }


    /// <summary>
    /// Коснулись пальцем экрана или нажали кнопку мыши.
    /// </summary>
    /// <param name="position"> Координаты касания или клика. </param>
    public bool OnTouchBegin(Vector2 position)
    {
        TouchToPicker = (Vector2)PickerTransform.position - position;
        if (TouchToPicker.sqrMagnitude <= PickerRadius * PickerRadius)
        {
            // Если попали на маркер - вернуть true, иначе false
            return true;
        }
        return false;
    }


    /// <summary>
    /// Перемещают палец по экрану или перемещают мышь удерживая нажатой кнопку.
    /// </summary>
    /// <param name="position"> Координаты пальца или мыши. </param>
    public void OnTouchMove(Vector2 position)
    {
        PickerTransform.position = position + TouchToPicker;

        // Ограничить положение маркера
        PickerTransform.anchoredPosition = PickerTransform.anchoredPosition.normalized * (CircleRadius - 0.5f * CircleThink);

        // Обновить цвет маркера и регулировки яркости
        Picker.color = CalcColor(PickerTransform.anchoredPosition);
        Saturation.Color = Color.Lerp(Color.white, Picker.color, Saturation.Slider.value);
        Brightness.Color = SelectedColor.color = Color.Lerp(Color.black, Saturation.Color, Brightness.Slider.value);

    }


    /// <summary>
    /// Убрали палец от экрана или отпустили кнопку мыши.
    /// </summary>
    /// <param name="position"> Координаты отпускания пальца или отпускания кнопки мыши. </param>
    public void OnTouchEnd(Vector2 position)
    {
    }
}
