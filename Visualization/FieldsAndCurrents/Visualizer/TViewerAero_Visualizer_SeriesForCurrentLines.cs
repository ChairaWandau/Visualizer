// Класс для визуализации серии линий тока
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using AstraEngine;
//***************************************************************
namespace Example
{
    public partial class TViewerAero_Visualizer
    {
        /// <summary>
        /// Визуализация серии расчетов линий тока
        /// </summary>
        /// <param name="CurrentLinesSettings">Настройки для линий тока</param>
        /// <param name="Timeout">Таймаут</param>
        public void SeriesForCurrentLinesVisualizer (TViewerAero_CurrentLinesSettings CurrentLinesSettings, int Timeout = 5)
        {
            TViewerAero_Scale2D Scale2D = new TViewerAero_Scale2D();
            for (int I=0; I<SeriesCurrentLines.Length; I++)
            {
                for (int i = 0; i < SeriesCurrentLines[I].Length; i++)
                {
                    if (SeriesCurrentLines[I][i] == null) continue;
                    //Отрисовка линий тока
                    СurrentLinesRender(SeriesCurrentLines[I][i], CurrentLinesSettings, Absolute[I*2+1], Absolute[I * 2]);
                }
                // Создание объекта шкалы 2D
                Scale2D.CreateScale2D(GetScaleField(new Vector2(1920, 1080), AbsoluteMin, AbsoluteMax));
                // Пауза между отрисовкой
                System.Threading.Thread.Sleep(Timeout * 1000);
                // Удаление шкалы
                Scale2D.DisposeScale2D();
                // Удаление всех линий тока
                DisposeСurrentLinesFromRender();
            }
            
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
