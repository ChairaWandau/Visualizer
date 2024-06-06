// Класс, хранящий методы для визуализации нескольких расчетов в виде слайд-шоу
using System;
using System.Collections.Generic;
using System.Linq;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Geometry.Model3D;
using MPT707.Aerodynamics.Structs;
//***************************************************************
namespace Example
{
    public partial class TViewerAero_Visualizer
    {
        /// <summary>
        /// Список вершин прямоугольников плоскости и текстур для отрисовки на плоскость
        /// </summary>
        public ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Texture)[] ResultsForSeriesOfCalculation;

        /// <summary>
        /// Массив моделей для нескольких расчетов (плоскостей, поверхностей или линий тока)
        /// </summary>
        private List<TModel3D>[] SeveralCalculations;
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать серию полей скоростей/давлений на плоскости
        /// </summary>
        /// <param name="Normal">Нормаль плоскости</param>
        /// <param name="MinMaxPlanePointsAndTextures">Список вершин прямоугольников плоскости и текстур для отрисовки на плоскость</param>
        /// <param name="Timeout">Время паузы в секундах</param>
        public void SeriesOfCalculationRender(Vector3 Normal, ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Textures)[] MinMaxPlanePointsAndTextures, int Timeout = 1)
        {
            try
            {
                // Объявляем массив
                ResultsForSeriesOfCalculation = MinMaxPlanePointsAndTextures;
                // Создаем модель плоскости, которую будем перерисовывать
                TModel3D Plane = new TModel3D(Render);
                for (int i = 0; i < ResultsForSeriesOfCalculation.Length; i++)
                {
                    // Рисуем плоскость с текстурой
                    CreatePlane(Normal, ResultsForSeriesOfCalculation[i].MinMaxPoints, ResultsForSeriesOfCalculation[i].Texture, Plane);
                    // Пауза между отрисовкой
                    System.Threading.Thread.Sleep(Timeout/2*75);
                }
                // Стираем плоскость с экрана по окончанию визуализации
                Plane.Unload();
                Plane = null;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C001: Error TViewerAero_Visualizer:SeriesOfCalculationRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Инициализировать массив результатов расчетов
        /// </summary>
        public void InitializeArrayOfResults(int NumberOfResults)
        {
            SeveralCalculations = new List<TModel3D>[NumberOfResults];
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Сохранить последний результат расчета
        /// </summary>
        internal void SaveLastResultOfVisualisation(int IndexOfCalculation, ETypeViewAero Viewer_TypeOutAero)
        {
            if (IndexOfCalculation > 0)
            {
                for (int i=0;i< SeveralCalculations[IndexOfCalculation-1].Count(); i++)
                {
                    SeveralCalculations[IndexOfCalculation - 1][i].Enable = false;
                }
            }
            switch (Viewer_TypeOutAero)
            {
                case ETypeViewAero.OnPlane:
                    List<TModel3D> NewPlanes = new List<TModel3D>();
                    for(int i = 0;i < Planes.Count; i++)
                    {
                        TModel3D NewPlane = new TModel3D(Render);
                        NewPlane = Planes[i];
                        //NewPlane.Enable = false;
                        NewPlanes.Add(NewPlane);
                    }
                    Planes.Clear();
                    SeveralCalculations[IndexOfCalculation] = NewPlanes;
                    break;
                case ETypeViewAero.OnSurface:
                    List<TModel3D> NewSurfaces = new List<TModel3D>();
                    for (int i = 0; i < Surfaces.Count; i++)
                    {
                        TModel3D NewSurface = new TModel3D(Render);
                        NewSurface = Surfaces[i];
                        //NewSurface.Enable = false;
                        NewSurfaces.Add(NewSurface);
                    }
                    Surfaces.Clear();
                    SeveralCalculations[IndexOfCalculation] = NewSurfaces;
                    break;
                case ETypeViewAero.СurrentLines:
                    List<TModel3D> NewСurrentLines = new List<TModel3D>();
                    for (int i = 0; i < CurrentLines.Count; i++)
                    {
                        TModel3D NewСurrentLine = new TModel3D(Render);
                        NewСurrentLine = CurrentLines[i];
                        NewСurrentLine.Enable = false;
                        NewСurrentLines.Add(NewСurrentLine);
                    }
                    CurrentLines.Clear();
                    SeveralCalculations[IndexOfCalculation] = NewСurrentLines; ;
                    break;
                default:
                    TJournalLog.WriteLog("C0093: Error TViewerAero:SaveLastResultOfVisualisation(): unindifier key ! Key: " + Viewer_TypeOutAero.ToString());
                    break;
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить массив результатов расчетов
        /// </summary>
        public List<TModel3D>[] GetResultsOfVisualisation()
        {
            return SeveralCalculations;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Обновить кадр
        /// </summary>
        public void UpdateFrame(int FrameID)
        {
            if (SeveralCalculations == null) return;
            for (int i = 0; i < SeveralCalculations.Length; i++)
            {
                if (SeveralCalculations[i] == null) continue;
                if(i == FrameID)
                {
                    // Отрисовать модели заданного кадра анимации
                    for (int j = 0; j < SeveralCalculations[i].Count; j++)
                    {
                        SeveralCalculations[i][j].Enable = true;
                    }
                }
                else
                {
                    // Скрыть модели всех кадров анимации, кроме необходимого
                    for (int j = 0; j < SeveralCalculations[i].Count; j++)
                    {
                        SeveralCalculations[i][j].Enable = false;
                    }
                }
            }

            //// Скрыть модели всех кадров анимации, кроме текущего
            //if (SeveralCalculations[FrameID] != null)
            //{
            //    for (int i = 0; i < SeveralCalculations[FrameID - 1].Count; i++)
            //    {
            //        SeveralCalculations[FrameID - 1][i].Enable = false;
            //    }
            //    for (int i = 0; i < SeveralCalculations.Last().Count; i++)
            //    {
            //        SeveralCalculations.Last()[i].Enable = false;
            //    }

                
            //}
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить результаты визуализации
        /// </summary>
        public void ClearResultsOfVisualisation()
        {
            if(SeveralCalculations == null) return;
            foreach (var Calculation in SeveralCalculations)
            {
                for (int i = 0; i < Calculation.Count; i++)
                {
                    Calculation[i].Unload();
                }
                Calculation.Clear();
            }
        }
        //---------------------------------------------------------------
    }
}
