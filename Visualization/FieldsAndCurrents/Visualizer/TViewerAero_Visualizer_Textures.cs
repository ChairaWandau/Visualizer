// Класс визуализатора, содержащий методы для создания текстур
using System;
using System.Drawing;
//
using AstraEngine;
using AstraEngine.Engine.GraphicCore;
//***************************************************************
namespace Example
{
    public partial class TViewerAero_Visualizer
    {
        //---------------------------------------------------------------
        /// <summary>
        /// Создает текстуру шкалы и подписей. Объединяет их с текстурой полей. Создает Model3D_Control, на который натягивает текстуру.
        /// </summary>
        /// <param name="DisplayResolution">Разрешение отрендеренной текстуры полей</param>
        /// <param name="min">Значение которому будет соответствовать HSV 0 Темносиний</param>
        /// <param name="max">Значение которому будет соответствовать HSV 240 Красный</param>
        public TTexture2D GetScaleField(Vector2 DisplayResolution, float min, float max)
        {
            // Настройки отрисовки шкалы
            int NumberOfScaleMarks = 11;
            // Коэффициент ширины шкалы от ширины плоскости с полями
            float ScaleWidthK = 0.0625f;
            // Корректировка коэффициента в зависимости от разрешения
            if (DisplayResolution.X < 1920f)
                ScaleWidthK = 1920f / (float)DisplayResolution.X * ScaleWidthK;
            // Высота шкалы в пикселях
            int ScaleHightInPixels = (int)DisplayResolution.Y;
            // Ширина шкалы в пикселях
            int ScaleWidthInPixels = (int)(DisplayResolution.X * ScaleWidthK);

            // Создание текстуры шкалы
            TTexture2D ScaleTexture = new TTexture2D();
            Color[,] ScaleColor = new Color[ScaleWidthInPixels, ScaleHightInPixels];
            for (int i = 0; i < ScaleHightInPixels; i++)
                for (int j = 0; j < ScaleWidthInPixels; j++) ScaleColor[j, i] = RGBFromHSV(240 * (ScaleHightInPixels - i) / ScaleHightInPixels, 1, 1);
            ScaleTexture.FromColor2DArray(ScaleColor);

            // массив текстур, которые будут объединены
            System.Tuple<AstraEngine.Point, TTexture2D>[] Textures = new System.Tuple<AstraEngine.Point, TTexture2D>[NumberOfScaleMarks + 1];
            // добавляем в массив текстуру чистой шкалы
            System.Tuple<AstraEngine.Point, TTexture2D> scale =
                new System.Tuple<AstraEngine.Point, TTexture2D>
                (new AstraEngine.Point(0, 0), ScaleTexture);
            Textures[0] = scale;

            // Создание тестового текста, чтобы получить его высоту в пикселях
            var TestTextTexture = new TTexture2D();
            TestTextTexture.TextToImage((0).ToString("E3"), new Font("Consolas", 12), Color.Transparent, Color.Black, System.Windows.Forms.TextFormatFlags.Default);

            // Шаг по значению для подписи
            float ValueStep = (max - min) / (NumberOfScaleMarks - 1);
            // Шаг по пикселям, для каждой из подписи
            int markStep = (ScaleColor.GetUpperBound(1) - TestTextTexture.Height) / (NumberOfScaleMarks - 1);

            // Создание текстур из текста и запись в массив для дальнейшего сложения
            for (int i = 0; i < NumberOfScaleMarks; i++)
            {
                var TextTexture = new TTexture2D();
                TextTexture.TextToImage((max - i * ValueStep).ToString("E3"), new Font("Consolas", 12), Color.Transparent, Color.Black, System.Windows.Forms.TextFormatFlags.Default);
                System.Tuple<AstraEngine.Point, TTexture2D> scaleMark =
                    new System.Tuple<AstraEngine.Point, TTexture2D>
                    (new AstraEngine.Point(0, markStep * i), TextTexture);
                Textures[i + 1] = scaleMark;
            }
            // Нанесение текста на текстуру шкалы
            ScaleTexture.CreateFromTextures(true, Textures);

            return ScaleTexture;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создание текстуры полей скоростей/давлений
        /// </summary>
        /// <param name="Colour">Двумерный массив цветов</param>
        /// <returns>Объект текстуры</returns>
        public TTexture2D FieldTextureRender(Color[,] Colour)
        {
            TTexture2D Texture2D = new TTexture2D();
            Texture2D.FromColor2DArray(Colour);
            return Texture2D;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создание массива цветов
        /// </summary>
        /// <param name="Resolution">Разрешение текстуры</param>
        /// <param name="Characteristic">Характеристика точки (модуль скорости или давление), значения которой нужно перевести в цвет</param>
        /// <param name="Min"></param>
        /// <param name="Max"></param>
        /// <returns>Двумерный массив цветов</returns>
        public Color[,] ColorRender(Vector2 Resolution, float[,] Characteristic, float Min, float Max)
        {
            //Min = 50_000;
            //Max = 120_000;
            Color[,] Colour = new Color[(int)Resolution.X, (int)Resolution.Y];
            for (int i = 0; i < Resolution.X; i++)
            {
                for (int j = 0; j < Resolution.Y; j++)
                {
                    // переводим значение давления в представление в виде цвета через палитру цвета
                    // Min, Max - давления на шкале
                    float Hue = 240 - (240 * (Characteristic[i, j] - Min) / (Max - Min));
                    if (Hue > 240) Hue = 240;
                    else if (Hue < 0) Hue = 0;
                    // конвертируем HSV в RGB
                    Colour[i, j] = RGBFromHSV(Hue, 1, 1);
                }
            }
            return Colour;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создание массива цветов
        /// </summary>
        /// <param name="PointsInCurrentLines">Разрешение текстуры</param>
        /// <returns>Одномерный массив цветов</returns>
        public Color[] ColorRender(TFemElement_Visual[] Characteristic, float Min, float Max)
        {
            //float Min = (float)220;
            //float Max = (float)390;
            Color[] Colour = new Color[Characteristic.Length];
            for (int i = 1; i < Characteristic.Length; i++)
            {
                // переводим значение давления в представление в виде цвета через палитру цвета
                // Min, Max - давления на шкале
                float Hue = 240 - (240 * (((float)Characteristic[i].VelocityModule + (float)Characteristic[i - 1].VelocityModule) / 2 - Min) / (Max - Min));
                if (Hue > 240) Hue = 240;
                else if (Hue < 0) Hue = 0;
                // конвертируем HSV в RGB
                Colour[i] = RGBFromHSV(Hue, 1, 1);
            }
            return Colour;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Переводит цвет из RGB в HSV
        /// </summary>
        protected Color RGBFromHSV(float H, float S, float V)
        {
            float num = 0f;
            float num2 = 0f;
            float num3 = 0f;
            while (H < 0f)
                H += 360f;
            while (H >= 360f)
                H -= 360f;
            if (V > 1f)
                V /= 255f;
            if (S > 1f)
                S /= 255f;
            if (V <= 0f)
                return Color.Black;
            if (S <= 0f)
                return Color.FromArgb(255, (int)(255f * V), (int)(255f * V), (int)(255f * V));
            float num4 = H / 60f;
            int num5 = (int)Math.Floor(num4);
            float num6 = num4 - (float)num5;
            float num7 = V * (1f - S);
            float num8 = V * (1f - S * num6);
            float num9 = V * (1f - S * (1f - num6));
            switch (num5)
            {
                case 0:
                    num = V;
                    num2 = num9;
                    num3 = num7;
                    break;
                case 1:
                    num = num8;
                    num2 = V;
                    num3 = num7;
                    break;
                case 2:
                    num = num7;
                    num2 = V;
                    num3 = num9;
                    break;
                case 3:
                    num = num7;
                    num2 = num8;
                    num3 = V;
                    break;
                case 4:
                    num = num9;
                    num2 = num7;
                    num3 = V;
                    break;
                case 5:
                    num = V;
                    num2 = num7;
                    num3 = num8;
                    break;
                case 6:
                    num = V;
                    num2 = num9;
                    num3 = num7;
                    break;
                case -1:
                    num = V;
                    num2 = num7;
                    num3 = num8;
                    break;
                default:
                    num = (num2 = (num3 = V));
                    break;
            }
            num = Clamp((int)((double)num * 255.0));
            num2 = Clamp((int)((double)num2 * 255.0));
            num3 = Clamp((int)((double)num3 * 255.0));
            return Color.FromArgb(255, (int)num, (int)num2, (int)num3);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Не дает значениям цвета превышать 255 и становится меньше 0
        /// </summary>
        protected int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
        //---------------------------------------------------------------
    }
}
