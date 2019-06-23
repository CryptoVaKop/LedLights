using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Слушатель событий от TouchController.
/// </summary>
public interface ITouchListener
{
    // Коснулись пальцем экрана
    bool OnTouchBegin(Vector2 position);

    // Водят пальцем по экрану
    void OnTouchMove(Vector2 position);

    // Убрали палец от экрана
    void OnTouchEnd(Vector2 position);
}


/// <summary>
/// Контроллер тача. 
/// Фиксирует следующие события: 
/// 1. Коснулись пальцем экрана или нажали кнопку мыши.
/// 2. Перемещают палец по экрану или перемещают мышь удерживая нажатой кнопку.
/// 3. Убрали палец от экрана или отпустили кнопку мыши.
/// </summary>
public static class TouchController
{
    /// <summary>
    /// Контекст слушателя.
    /// </summary>
    class ListenerContext
    {
        // Ссылка на слушателя
        public ITouchListener TouchListener;

        // Идентификатор пальца, используемый слушателем
        public int FingerId = -1;

        public ListenerContext(ITouchListener touchListener)
        {
            TouchListener = touchListener;
        }
    }

    /// <summary>
    /// Список контекстов слушателей.
    /// </summary>
    static List<ListenerContext> ContextsList = new List<ListenerContext>();


    /// <summary>
    /// Зарегистрировать слушателя.
    /// </summary>
    /// <param name="touchListener"> Ссылка на регистрируемого слушателя. </param>
    public static void RegListener(ITouchListener touchListener)
    {
        ContextsList.Add(new ListenerContext(touchListener));
    }


    /// <summary>
    /// Разрегистрировать слушателя.
    /// </summary>
    /// <param name="touchListener"> Ссылка на разрегистрируемого слушателя. </param>
    public static void UnregListener(ITouchListener touchListener)
    {
        ContextsList.RemoveAll((context) =>
        {
            return (context.TouchListener == touchListener) || (context.TouchListener == null);
        });
    }


    /// <summary>
    /// Получить координаты клика.
    /// </summary>
    /// <param name="touchPosition"> если клик был произведен не по GUI-элементу, 
    /// то в touchPosition будут помещены координаты клика </param>
    /// <returns> true - если клик был произведен не по GUI-элементу, 
    /// false - если клик был произведен по GUI-элементу. </returns>
    public static bool GetTouch(out Vector3 touchPosition)
    {
        touchPosition = Vector2.zero;
        int touchCount = Input.touchCount;

        if (touchCount > 0)
        {
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.touches[i];

                // Если клик был произведен не по GUI-елементу - вернуть координаты клика
                if ((touch.phase == TouchPhase.Began) && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    touchPosition = touch.position;
                    return true;
                }
            }
            return false;
        }

        // Если клик левой кнопкой мыши был произведен не по GUI-елементу - вернуть координаты клика
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            touchPosition = Input.mousePosition;
            return true;
        }
        return false;
    }


    /// <summary>
    /// Данный метод необходимо вызывать перед каждым обновлением фрейма.
    /// </summary>
    public static void Update()
    {
        if (Input.touchCount > 0)
        {
            for (int n = 0; n < Input.touchCount; n++)
            {
                Touch touch = Input.GetTouch(n);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                    {
                        OnTouchBegin(touch.fingerId, touch.position);
                        break;
                    }

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                    {
                        OnMove(touch.fingerId, touch.position);
                        break;
                    }

                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                    {
                        OnTouchEnd(touch.fingerId, touch.position);
                        break;
                    }
                }
            }
            return;
        }

        for (int n = 0; n < 3; n++)
        {
            if (Input.GetMouseButtonDown(n))
            {
                OnTouchBegin(n, Input.mousePosition);
            }
            else if (Input.GetMouseButton(n))
            {
                OnMove(n, Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(n))
            {
                OnTouchEnd(n, Input.mousePosition);
            }
        }
    }


    /// <summary>
    /// Коснулись пальцем экрана или нажали кнопку мыши.
    /// </summary>
    /// <param name="fingerId"> Идентификатор пальца или индекс кнопки мыши. </param>
    /// <param name="position"> Координаты касания или клика. </param>
    static void OnTouchBegin(int fingerId, Vector2 position)
    {
        // Пройтись по всем подписчикам, найти свободных
        foreach (ListenerContext context in ContextsList)
        {
            if ((context.FingerId < 0) && context.TouchListener.OnTouchBegin(position))
            {
                context.FingerId = fingerId;
                break;
            }
        }
    }


    /// <summary>
    /// Перемещают палец по экрану или перемещают мышь удерживая нажатой кнопку.
    /// </summary>
    /// <param name="fingerId"> Идентификатор пальца или индекс кнопки мыши. </param>
    /// <param name="position"> Координаты пальца или мыши </param>
    static void OnMove(int fingerId, Vector2 position)
    {
        foreach (ListenerContext context in ContextsList)
        {
            if (context.FingerId == fingerId)
            {
                context.TouchListener.OnTouchMove(position);
                break;
            }
        }
    }


    /// <summary>
    /// Убрали палец от экрана или отпустили кнопку мыши.
    /// </summary>
    /// <param name="fingerId"> Идентификатор пальца или индекс кнопки мыши. </param>
    /// <param name="position"> Координаты отпускания пальца или отпускания кнопки мыши. </param>
    static void OnTouchEnd(int fingerId, Vector2 position)
    {
        foreach (ListenerContext context in ContextsList)
        {
            if (context.FingerId == fingerId)
            {
                context.TouchListener.OnTouchEnd(position);
                context.FingerId = -1;
                break;
            }
        }
    }
}
