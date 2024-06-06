// Класс, храненящий методы для проведения множества расчетов на плоскости и управления сохраненными данными
using System;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
using MPT707.Aerodynamics.Structs;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        //---------------------------------------------------------------
        /// <summary>
        /// Получение поля давления/скорости на плоскости для множества расчетов (без визуализации)
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая характеристика</param>
        /// <param name="Value">Плоскость</param>
        /// <param name="DisplayResolution">Разрешение отрендеренной текстуры полей</param>
        private ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Textures)[] SeriesOfCalculationsOnPlane(ETypeValueAero eTypeValueAero, Plane Value, Vector2 DisplayResolution)
        {
            try
            {
                // Получаем количество сохраненных результатов расчетов
                int NumberOfResults = ResultsManager.GetNumberOfResults;
                // Создаем массив для хранения вешин, по котрым будет построена плоскость, и текстур полей
                ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Textures)[] MinMaxPlanePointsAndTextures =
                    new ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Texture)[NumberOfResults];
                // Визуализируем результат каждого расчета на плоскости и сохраняем полученную текстуру
                for (int I = 0; I < NumberOfResults; I++)
                {
                    // Получаем результат расчета
                    FEM_V = ResultsManager.RecallResult(I);
                    // Получаем поле на плоскости
                    ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, float Min, float Max, float[,] PlanePointsCharacteristic) Result =
                        CalculationsOnPlane(eTypeValueAero, Value, DisplayResolution);
                    if (Result.PlanePointsCharacteristic == null) continue;
                    MinMaxPlanePointsAndTextures[I].MinMaxPoints = Result.MinMaxPoints;
                    MinMaxPlanePointsAndTextures[I].Textures = Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, Result.PlanePointsCharacteristic, Result.Min, Result.Max));
                }
                return MinMaxPlanePointsAndTextures;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00013: Error TViewerAero:SeriesOfCalculationsOnPlane(): " + E.Message);
                return null;
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получение поля давления/скорости на плоскости для множества расчетов (без визуализации)
        /// </summary>
        /// <param name="eTypeValueAero">Интересующая характеристика</param>
        /// <param name="Value">Плоскость</param>
        /// <param name="DisplayResolution">Разрешение отрендеренной текстуры полей</param>
        private ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Textures)[] SeriesOfCalculationsOnSurface(ETypeValueAero eTypeValueAero, params string[] Surfaces)
        {
            try
            {
                // Получаем количество сохраненных результатов расчетов
                int NumberOfResults = ResultsManager.GetNumberOfResults;
                // Создаем массив для хранения вешин, по котрым будет построена плоскость, и текстур полей
                ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Textures)[] MinMaxPlanePointsAndTextures =
                    new ((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, TTexture2D Texture)[NumberOfResults];
                // Визуализируем результат каждого расчета на плоскости и сохраняем полученную текстуру
                for (int I = 0; I < NumberOfResults; I++)
                {
                    // Получаем результат расчета
                    FEM_V = ResultsManager.RecallResult(I);
                    // Получаем поле на плоскости
                    //((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMaxPoints, float Min, float Max, float[,] PlanePointsCharacteristic) Result =
                    //    CalculationsOnPlane(eTypeValueAero, Value, DisplayResolution);
                    //if (Result.PlanePointsCharacteristic == null) continue;
                    //MinMaxPlanePointsAndTextures[I].MinMaxPoints = Result.MinMaxPoints;
                    //MinMaxPlanePointsAndTextures[I].Textures = Visualizer.FieldTextureRender(Visualizer.ColorRender(DisplayResolution, Result.PlanePointsCharacteristic, Result.Min, Result.Max));
                }
                return MinMaxPlanePointsAndTextures;
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00013: Error TViewerAero:SeriesOfCalculationsOnPlane(): " + E.Message);
                return null;
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить все результаты расчетов
        /// </summary>
        public void ClearBuffer()
        {
            try
            {
                ResultsManager.ForgetResults();
                Visualizer.DisposeResultsForSeriesOfCalculation();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00014: Error TViewerAero:ClearBuffer(): " + E.Message);
            }
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Удалить конкретный результат расчета
        /// </summary>
        /// <param name="ID">Индекс расчета</param>
        public void ClearBuffer(int ID)
        {
            try
            {
                ResultsManager.ForgetResult(ID);
                Visualizer.DisposeResultsForSeriesOfCalculation(ID);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C00015: Error TViewerAero:ClearBuffer(int ID): " + E.Message);
            }
        }
        //---------------------------------------------------------------
    }
}
