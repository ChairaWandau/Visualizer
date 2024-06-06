// Класс для работы с UV картой
using System;
using System.Drawing;
//
using AstraEngine;
using AstraEngine.Components.MathHelper;
using AstraEngine.Engine.GraphicCore;
//***************************************************************
namespace Example
{
    public partial class TViewerAero_Visualizer
    {
        /// <summary>
        /// Размер отступа между цветами 
        /// </summary>
        private int UVMapColorDistance = 3;
        /// <summary>
        /// Количество цветов
        /// </summary>
        private int UVMapColorCount = 1024;
        /// <summary>
        /// Минимальное значение величины в сетке
        /// </summary>
        private float AbsoluteMin;
        /// <summary>
        /// Максимальное значение величины в сетке
        /// </summary>
        private float AbsoluteMax;
        //---------------------------------------------------------------
        /// <summary>
        /// Создать UV карту в виде HSV палитры
        /// </summary>
        /// <returns> Массив цветов, из которого состоит UV карта</returns>
        public TTexture2D CreateUVMap()
        {
            // Задаем размер массива цветов, из которого будет состоять UV карта
            Color[,] UVMap = new Color[1, UVMapColorDistance * UVMapColorCount];
            // Заполняем массив
            for (int I = 0; I < UVMapColorDistance * UVMapColorCount; I += UVMapColorDistance)
            {
                float Color = TMath.LINO(0, 0, UVMapColorDistance * UVMapColorCount - 1, 240, I);
                for (int J = 0; J < UVMapColorDistance; J++)
                {
                    UVMap[0, I + J] = RGBFromHSV(Color, 1, 1);
                }
            }
            TTexture2D Texture = new TTexture2D();
            Texture.FromColor2DArray(UVMap);
            return Texture;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создать UV карту в виде HSV палитры
        /// </summary>
        /// <param name="Min">Минимальное значение величины, заданное пользователем</param>
        /// <param name="Max">Максимальное значение величины, заданное пользователем</param>
        /// <returns> Массив цветов, из которого состоит UV карта</returns>
        public TTexture2D CreateUVMap(float Min, float Max)
        {
            UVMapColorCount = 1024;
            if (Min < AbsoluteMin) Min = AbsoluteMin;
            if (Max > AbsoluteMax) Max = AbsoluteMax;
            // Считаем в процентах максимум и минимум (AbsoluteMax = 100%, AbsoluteMin = 0 %)
            float MinPersent = TMath.LINO(AbsoluteMin, 0, AbsoluteMax, 100, Min);
            float MaxPersent = TMath.LINO(AbsoluteMin, 0, AbsoluteMax, 100, Max);
            // Коэффициент для перевода из процентов в количество пикселей
            float FromPersentToPixelCount = UVMapColorCount / (MaxPersent - MinPersent);
            // Считаем в процентах разницу между максимумами и минимумами
            float MinDifferencePersent = MinPersent;
            float MaxDifferencePersent = 100 - MaxPersent;
            // Перевод из процентов в количество пикселей
            int MinDifferencePixelCount = (int)Math.Ceiling(MinDifferencePersent * FromPersentToPixelCount);
            int MaxDifferencePixelCount = (int)Math.Ceiling(MaxDifferencePersent * FromPersentToPixelCount);

            // Задаем размер массива цветов, из которого будет состоять UV карта
            Color[,] UVMap = new Color[1, MinDifferencePixelCount + UVMapColorDistance * UVMapColorCount + MaxDifferencePixelCount];
            // Заполняем массив синим цветом до участка размером UVMapColorDistance * UVMapColorCount
            Color Blue = RGBFromHSV(0, 1, 1);
            for (int I = 0; I < MinDifferencePixelCount; I++)
                UVMap[0, I] = Blue;
            // Заполняем участок массива размером UVMapColorDistance * UVMapColorCount цветами от синего к красному
            for (int I = MinDifferencePixelCount; I < MinDifferencePixelCount + UVMapColorDistance * UVMapColorCount; I += UVMapColorDistance)
            {
                float Color = TMath.LINO(MinDifferencePixelCount, 0, (MinDifferencePixelCount + UVMapColorDistance * UVMapColorCount) - 1, 240, I);
                for (int J = 0; J < UVMapColorDistance; J++)
                {
                    UVMap[0, I + J] = RGBFromHSV(Color, 1, 1);
                }
            }
            // Заполняем остаток массива красным
            Color Red = RGBFromHSV(240, 1, 1);
            for (int I = MinDifferencePixelCount + UVMapColorDistance * UVMapColorCount; I < MinDifferencePixelCount + UVMapColorDistance * UVMapColorCount + MaxDifferencePixelCount; I++)
                UVMap[0, I] = Red;
            TTexture2D Texture = new TTexture2D();
            Texture.FromColor2DArray(UVMap);
            return Texture;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Изменить абсолютные значения минимумов и максимумов
        /// </summary>
        /// <param name="AbsoluteMin">Минимальное значение величины в сетке</param>
        /// <param name="AbsoluteMax">Максимальное значение величины в сетке</param>
        /// <returns> Массив цветов, из которого состоит UV карта</returns>
        public void ChangeUVMapAbsoluteMinMax(float AbsoluteMin, float AbsoluteMax)
        {
            this.AbsoluteMin = AbsoluteMin;
            this.AbsoluteMax = AbsoluteMax;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получить UV координату для данного значения величины
        /// </summary>
        /// <param name="Characteristic">Значение величины</param>
        /// <param name="Min">Минимальное значение величины</param>
        /// <param name="Max">Максимальное значение величины</param>
        /// <returns>UV координата для данного значения характеристики</returns>
        public Vector2 GetUVCoordinate(float Characteristic, float Min, float Max)
        {
            //MinMax.X = 95000;
            //MinMax.Y = 100000;
            // переводим значение характеристи
            float Hue = 240 - (240 * (Characteristic - Min) / (Max - Min));
            if (Hue > 240)
            {
                return new Vector2(1f, (float)(UVMapColorCount * UVMapColorDistance - (UVMapColorDistance - 1) / 2f) / (float)(UVMapColorDistance * UVMapColorCount));
            }
            else if (Hue < 0) 
            {
                return new Vector2(1f, (float)((UVMapColorDistance - 1) / 2f) / (float)(UVMapColorDistance * UVMapColorCount));
            }
            else
            {
                float Pixel = (1 + TMath.LINO(0, 0, 240, UVMapColorCount - 1, Hue)) * UVMapColorDistance - (UVMapColorDistance - 1) / 2f;
                return new Vector2(1f, Pixel / (float)(UVMapColorDistance * UVMapColorCount));
            }
        }
        //---------------------------------------------------------------
    }
}