﻿//Структура для сохранения результатов, заданных пользователем точек для линий тока
using AstraEngine;
//***************************************************************
namespace Example
{
    public class TViewerAero_CurrentLinesValueForVisualisation
    {
        /// <summary>
        /// Индекс метода
        /// </summary>
        public int IndexMethod = -1;
        //---------------------------------------------------------------------
        /// <summary>
        /// Центр окружности или первая точка
        /// </summary>
        public Vector3 Center = new Vector3();
        //---------------------------------------------------------------------
        /// <summary>
        /// Радиус или шаг или вертикальный шаг
        /// </summary>
        public float Radius = -1f;
        //---------------------------------------------------------------------
        /// <summary>
        /// Количество вершин или окружностей
        /// </summary>
        public int CountVertices = -1;
        //---------------------------------------------------------------------
        /// <summary>
        /// Вторая точка
        /// </summary>
        public Vector3 SecondPoints = new Vector3();
        //---------------------------------------------------------------------
        /// <summary>
        /// Третья точка
        /// </summary>
        public Vector3 ThirdPoints = new Vector3();
        //---------------------------------------------------------------------
        /// <summary>
        /// Шаг по горизонтали
        /// </summary>
        public float StepHorisontal=1f;
        //---------------------------------------------------------------------
        /// <summary>
        /// Угол между точками
        /// </summary>
        public float Angle = -1f;
        //---------------------------------------------------------------------
        /// <summary>
        /// Коэффициент увеличесния количества точек
        /// </summary>
        public float Ratio = -1f;
        //---------------------------------------------------------------------
        /// <summary>
        /// Плоскость, на которой точки
        /// </summary>
        public Plane Plane = new Plane();
        //---------------------------------------------------------------------
    }
}
