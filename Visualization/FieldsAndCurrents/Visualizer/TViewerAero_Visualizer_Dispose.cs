// Класс визуализатора, содержащий методы для удаления отрисованных объектов
using System;
using System.Collections.Generic;
//
using AstraEngine.Components;
using AstraEngine.Geometry.Model3D;
//***************************************************************
namespace Example
{
    public partial class TViewerAero_Visualizer
    {
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить все модели с экрана
        /// </summary>
        public void DisposeFromRender(List<TModel3D> Scene)
        {
            try
            {
                // Удаляем все модели, нарисованные программой (кроме расчетной области)
                DisposeHelpModelsFromRender();
                // Удаляем все оси, нормали и т.д.
                DisposeHelpModelsTransformableFromRender();
                // Удаляем все модели, из который состоит расчетная область
                DisposeDomainFromRender();
                // Удаляем все модели, загруженные пользователем
                DisposeSceneFromRender(Scene);
                // Удалить модели линий, описывающие грани ячеек
                DisposeEdgesOfCellsFromRender();
                // Удалить модели, описывающие поля на поверхности
                DisposeSurfacesFromRender();
                // Удалить модели, описывающие поля на плоскости
                DisposePlanesFromRender();
                // Удалить модели, описывающие линии тока
                DisposeСurrentLinesFromRender();
                // Удалить точки, описывающие начало линий тока
                DisposePointsForСurrentLinesFromRender();
                // Удалить сохраненные текстуры и плоскости для серии расчетов
                //DisposeResultsForSeriesOfCalculation();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_Visualizer:DisposeFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удаляем все модели, загруженные пользователем
        /// </summary>
        public void DisposeSceneFromRender(List<TModel3D> Scene)
        {
            try
            {
                // Удаляем все модели, загруженные пользователем
                foreach (var Model in Scene)
                {
                    Model.Unload();
                }
                Scene.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0008: Error TViewerAero_Visualizer:DisposeSceneFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить расчетную область
        /// </summary>
        public void DisposeDomainFromRender()
        {
            try
            {
                foreach (var Model in DomainModels)
                {
                    Model.Unload();
                }
                DomainModels.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0009: Error TViewerAero_Visualizer:DisposeDomainFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить вспомогательные модели (кроме моделей, загруженных пользователем, и расчетной области), которые каждый раз пересчитываются
        /// </summary>
        public void DisposeHelpModelsFromRender()
        {
            try
            {
                // Удаляем все модели, нарисованные программой (кроме расчетной области)
                foreach (var Model in HelpModels)
                {
                    Model.Unload();
                }
                HelpModels.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0010: Error TViewerAero_Visualizer:DisposeHelpModelsFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить вспомогательные модели (кроме моделей, загруженных пользователем, и расчетной области), которые не  нужно пересчитывать каждый раз (оси, нормали)
        /// </summary>
        public void DisposeHelpModelsTransformableFromRender()
        {
            try
            {
                // Удаляем все модели, нарисованные программой (кроме расчетной области)
                foreach (var Model in HelpModels_Transformable)
                {
                    Model.Unload();
                }
                HelpModels_Transformable.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0011: Error TViewerAero_Visualizer:DisposeHelpModelsTransformableFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить модели линий, описывающие грани ячеек
        /// </summary>
        public void DisposeEdgesOfCellsFromRender()
        {
            try
            {
                // Удаляем все модели, нарисованные программой (кроме расчетной области)
                foreach (var Edge in Edges)
                {
                    Edge.Unload();
                }
                Edges.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0012: Error TViewerAero_Visualizer:DisposeEdgesOfCellsFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить модели, описывающие поверхности
        /// </summary>
        public void DisposeSurfacesFromRender()
        {
            try
            {
                foreach (var Surface in Surfaces)
                {
                    Surface.Unload();
                }
                Surfaces.Clear();
                ParentPartID_to_SurfaceID.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0013: Error TViewerAero_Visualizer:DisposeSurfacesFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить модели, описывающие плоскости
        /// </summary>
        public void DisposePlanesFromRender()
        {
            try
            {
                foreach (var Plane in Planes)
                {
                    Plane.Unload();
                }
                Planes.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0013: Error TViewerAero_Visualizer:DisposeSurfacesFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить модели, описывающие поверхности
        /// </summary>
        public void DisposeСurrentLinesFromRender()
        {
            try
            {
                foreach (var line in CurrentLines)
                {
                    line.Unload();
                }
                CurrentLines.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0014: Error TViewerAero_Visualizer:DisposeСurrentLinesFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить точки, описывающие поверхности
        /// </summary>
        public void DisposePointsForСurrentLinesFromRender()
        {
            try
            {
                foreach (var line in PointsForCurrentLines)
                {
                    line.Unload();
                }
                PointsForCurrentLines.Clear();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0014: Error TViewerAero_Visualizer:DisposePointsForСurrentLinesFromRender(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить сохраненные текстуры и плоскости для серии расчетов
        /// </summary>
        public void DisposeResultsForSeriesOfCalculation()
        {
            try
            {
                if (ResultsForSeriesOfCalculation != null)
                {
                    for (int i = 0; i < ResultsForSeriesOfCalculation.Length; i++)
                        DisposeResultsForSeriesOfCalculation(i);
                    ResultsForSeriesOfCalculation = null;
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0015: Error TViewerAero_Visualizer:DisposeResultsForSeriesOfCalculation(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить конкретную сохраненную текстуру для серии расчетов
        /// <param name="ID">Индекс расчета</param>
        /// </summary>
        public void DisposeResultsForSeriesOfCalculation(int ID)
        {
            try
            {
                if (ResultsForSeriesOfCalculation[ID].Texture != null)
                {
                    ResultsForSeriesOfCalculation[ID].Texture.UnloadFromDevice();
                    ResultsForSeriesOfCalculation[ID].Texture = null;
                }
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0016: Error TViewerAero_Visualizer:DisposeResultsForSeriesOfCalculation(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
    }
}
