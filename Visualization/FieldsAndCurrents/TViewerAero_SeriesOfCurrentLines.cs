// Класс для расчета линий тока по нескольким расчетам
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Geometry.Model3D;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        /// <summary>
        /// Расчет линий тока для серии расчетов
        /// </summary>
        /// <param name="BasePoints">Список базовых точек</param>
        /// <param name="CurrentLinesSettings">Настройки для линий тока</param>
        /// <returns></returns>
        public (TFemElement_Visual[][][] Points, float[] Absoluty) SeriesOfCalculationForCurrentLines(List<Vector3> BasePoints, TViewerAero_CurrentLinesSettings CurrentLinesSettings, int Timeout=5)
        {
            try
            {
                // Количество расчетов
                int NumberOfResults = ResultsManager.GetNumberOfResults;
                // Массив с рассчитанными линиями
                TFemElement_Visual[][][] CurrentLines = new TFemElement_Visual[NumberOfResults][][];
                for (int i = 0; i < NumberOfResults; i++)
                {
                    // Запрос результатов каждого расчета
                    FEM_V = ResultsManager.RecallResult(i);
                    // Отключить секущую плоскость, если она была создана
                    if (ControlPlane != null) Disable_ControlPlane();
                    // Удаляем 2D шкалу, если она была создана
                    if (Scale2D != null) Scale2D.DisposeScale2D();
                    // Определяем расчетную область
                    Display_Domain(FEM_V);
                    TFemElement_Visual[] BasePoint = TransferVector3BasePoint(BasePoints);
                    // Массив массивов с линями тока
                    CurrentLines[i] = new TFemElement_Visual[BasePoints.Count][];
                    Parallel.For(0, CurrentLines[i].Length, I =>
                    {
                        TFemElement_Visual[] OneCurrentLine = CreateCurrentLine(BasePoint[I], CurrentLinesSettings);
                        if (OneCurrentLine.Length == 0) CurrentLines[i][I] = null;
                        CurrentLines[i][I] = OneCurrentLine;
                    });
                }
                // Массив с цветами, для последующей визуализации
                float[] Absolute = new float[NumberOfResults*2];
                // Поочередная визуализация линий тока для каждого расчета
                for (int I = 0; I < CurrentLines.Length; I++)
                {
                    // Расчет данных для раскрашивания прямых
                    float AbsoluteMax = float.MinValue;
                    float AbsoluteMin = float.MaxValue;
                    for (int j = 0; j < CurrentLines[I].Length; j++)
                    {
                        if (CurrentLines[I][j] == null) continue;
                        for (int k = 0; k < CurrentLines[I][j].Length; k++)
                        {
                            if (AbsoluteMax < CurrentLines[I][j][k].VelocityModule) AbsoluteMax = CurrentLines[I][j][k].VelocityModule;
                            if (AbsoluteMin > CurrentLines[I][j][k].VelocityModule) AbsoluteMin = CurrentLines[I][j][k].VelocityModule;
                        }
                    }
                    Absolute[I*2]=AbsoluteMax;
                    Absolute[I * 2 + 1] = AbsoluteMin;
                }
                return (CurrentLines, Absolute);
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C001: Error TViewerAero:SeriesOfCalculationForCurrentLines(): " + E.Message);
                return (null, null);
            }

        }
        //---------------------------------------------------------------
    }
}
