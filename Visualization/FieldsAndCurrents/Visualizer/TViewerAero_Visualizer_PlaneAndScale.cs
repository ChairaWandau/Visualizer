// Класс визуализатора, содержащий методы для отрисовки плоскости и шкалы на плоскости
using System.Collections.Generic;
using System.Drawing;
//
using AstraEngine;
using AstraEngine.Engine.GraphicCore;
using AstraEngine.Geometry.Model3D;
using AstraEngine.Scene;
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
        /// <param name="MinMax">Координаты вершин плоскости</param>
        /// <param name="PlaneTexture">Текстура с полями</param>
        public void PlaneWithScaleRender(Vector2 DisplayResolution, float min, float max, (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMax, TTexture2D PlaneTexture)
        {
            // Получаем отсортированный MinMax в глобальной системе координат
            var OXY = GetOXY(MinMax);
            // Нормаль к шкале
            var Normal = Vector3.Normalize(Vector3.Cross(OXY[1] - OXY[0], OXY[3] - OXY[0]));
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

            // Создание текстуры, на которой будут находится поля, шкала и подписи шкалы
            TTexture2D PlaneWithScaleTexture = new TTexture2D();
            PlaneWithScaleTexture.CreateEmpty((int)(DisplayResolution.X * (1f + ScaleWidthK)), (int)DisplayResolution.Y);
            PlaneWithScaleTexture.CreateFromTextures(false, new System.Tuple<AstraEngine.Point, TTexture2D>[]
            {
                new System.Tuple<AstraEngine.Point, TTexture2D>(new AstraEngine.Point(0,0),ScaleTexture),
                new System.Tuple<AstraEngine.Point, TTexture2D>(new AstraEngine.Point((int)(DisplayResolution.X*ScaleWidthK),0),PlaneTexture)
            });

            // Ширина шкалы (не в пикселях). Будет использоваться, как смещение относительно координат на которых расположена плоскость с полями
            var X_CorrectionBeacuseOfScale = (OXY[1] - OXY[0]) * ScaleWidthK;
            // Толщина шкалы на UV карте по координате X(не
            float UV_CorrectionBecauseOfScale = ScaleWidthK / (ScaleWidthK + 1f);

            // Создание плоскости на которую будет натянута текстура с изображением полей, шкалой, надписями.
            TModel3D Plane3D = new TModel3D(Render);
            Planes.Add(Plane3D);
            Plane3D.ControlLoad.LoadModel(new List<TTriangleContainer> { new TTriangleContainer(new List<TTriangle>
            {
                // Плоскость
                new TTriangle
                {
                    P0 = MinMax.OO,
                    P1 = MinMax.XO,
                    P2 = MinMax.OY,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV1 = new Vector2(1,0),
                    UV2 = new Vector2(UV_CorrectionBecauseOfScale,1)
                },
                new TTriangle
                {
                    P0 = MinMax.XO,
                    P1 = MinMax.XY,
                    P2 = MinMax.OY,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = new Vector2(1,0),
                    UV1 = new Vector2(1,1),
                    UV2 = new Vector2(UV_CorrectionBecauseOfScale,1)
                },
                new TTriangle
                {
                    P0 = MinMax.XO,
                    P1 = MinMax.OO,
                    P2 = MinMax.XY,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = new Vector2(1,0),
                    UV1 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV2 = new Vector2(1,1)
                },
                new TTriangle
                {
                    P0 = MinMax.OO,
                    P1 = MinMax.OY,
                    P2 = MinMax.XY,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV1 = new Vector2(UV_CorrectionBecauseOfScale,1),
                    UV2 = new Vector2(1,1)
                },

                // Шкала
                new TTriangle
                {
                    P0 = OXY[0]-X_CorrectionBeacuseOfScale,
                    P1 = OXY[0],
                    P2 = OXY[3]-X_CorrectionBeacuseOfScale,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = new Vector2(0,0),
                    UV1 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV2 = new Vector2(0,1)
                },
                new TTriangle
                {
                    P0 = OXY[0],
                    P1 = OXY[3],
                    P2 = OXY[3]-X_CorrectionBeacuseOfScale,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV1 = new Vector2(UV_CorrectionBecauseOfScale,1),
                    UV2 = new Vector2(0,1)
                },
                new TTriangle
                {
                    P0 = OXY[1]+X_CorrectionBeacuseOfScale,
                    P1 = OXY[1],
                    P2 = OXY[2]+X_CorrectionBeacuseOfScale,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = new Vector2(0,0),
                    UV1 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV2 = new Vector2(0,1)
                },
                new TTriangle
                {
                    P0 = OXY[1],
                    P1 = OXY[2],
                    P2 = OXY[2]+X_CorrectionBeacuseOfScale,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = new Vector2(UV_CorrectionBecauseOfScale,0),
                    UV1 = new Vector2(UV_CorrectionBecauseOfScale,1),
                    UV2 = new Vector2(0,1)
                }
            })});
            // Добавление текстуры к плоскости
            Plane3D.SetTexture(PlaneWithScaleTexture);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать плоскость, на которой рисуются поля скоростей/давлений
        /// </summary>
        /// <param name="Normal">Нормаль от плоскости</param>
        /// <param name="MinMax">Минимальные и максимальные значения (отсортированы в базисе на плоскости)</param>
        /// <param name="Texture">Текстура поля скоростей или поля давлений</param>
        public void PlaneRender(Vector3 Normal, (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMax, TTexture2D Texture)
        {
            // Создание плоскости
            TModel3D Plane3D = new TModel3D(Render);
            Planes.Add(Plane3D);
            CreatePlane(Normal, MinMax, Texture, Plane3D);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать плоскость, на которой рисуются поля скоростей/давлений
        /// </summary>
        /// <param name="Normal">Нормаль от плоскости</param>
        /// <param name="MinMax">Минимальные и максимальные значения (отсортированы в базисе на плоскости)</param>
        /// <param name="Texture">Текстура поля скоростей или поля давлений</param>
        public void CreatePlane(Vector3 Normal, (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMax, TTexture2D Texture, TModel3D Plane3D)
        {
            Plane3D.ControlLoad.LoadModel(new List<TTriangleContainer> { new TTriangleContainer(new List<TTriangle>
            {
                new TTriangle
                {
                    P0 = MinMax.OO,
                    P1 = MinMax.XO,
                    P2 = MinMax.OY,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = new Vector2(0,0),
                    UV1 = new Vector2(1,0),
                    UV2 = new Vector2(0,1)
                },
                new TTriangle
                {
                    P0 = MinMax.XO,
                    P1 = MinMax.XY,
                    P2 = MinMax.OY,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = new Vector2(1,0),
                    UV1 = new Vector2(1,1),
                    UV2 = new Vector2(0,1)
                },
                new TTriangle
                {
                    P0 = MinMax.XO,
                    P1 = MinMax.OO,
                    P2 = MinMax.XY,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = new Vector2(1,0),
                    UV1 = new Vector2(0,0),
                    UV2 = new Vector2(1,1)
                },
                new TTriangle
                {
                    P0 = MinMax.OO,
                    P1 = MinMax.OY,
                    P2 = MinMax.XY,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = new Vector2(0,0),
                    UV1 = new Vector2(0,1),
                    UV2 = new Vector2(1,1)
                }
            })});
            // Добавление текстуры к плоскости
            Plane3D.SetTexture(Texture);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать плоскость, на которой рисуются поля скоростей/давлений
        /// </summary>
        /// <param name="Normal">Нормаль от плоскости</param>
        /// <param name="MinMax">Минимальные и максимальные значения (отсортированы в базисе на плоскости)</param>
        public void PlaneRender(Vector3 Normal, (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMax)
        {
            TTexture2D WhiteTexture = new TTexture2D();
            Color[,] WhiteColor = new Color[1, 1];
            WhiteColor[0, 0] = Color.White;
            WhiteTexture.FromColor2DArray(WhiteColor);
            PlaneRender(Normal, MinMax, WhiteTexture);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Визуализировать шкалу значений
        /// </summary>
        public void ScaleRender(Vector2 DisplayResolution, float min, float max, (Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMax)
        {
            // Получаем отсортированные мин максы в глобальной системе координат
            var OXY = GetOXY(MinMax);
            var Normal = Vector3.Normalize(Vector3.Cross(OXY[1] - OXY[0], OXY[3] - OXY[0]));
            // Настройки отрисовки шкалы
            int NumberOfScaleMarks = 11;
            float ScaleWidthK = 0.0625f;
            int HightInPixels = (int)DisplayResolution.Y;
            int WidthInPixels = (int)(DisplayResolution.X * ScaleWidthK);
            Vector3 StepFromPlane = Normal * 50f;

            float Ratio = 5;
            var X = (OXY[1] - OXY[0]) * ScaleWidthK;
            var Y = Vector3.Normalize(OXY[3] - OXY[0]) * X.Length() / Ratio;
            X = X / 2;
            Y = Y / 2;
            // Добавочные пиксели к шкале
            int AdditionalPixels = (int)(HightInPixels / (OXY[3] - OXY[0]).Length() * Y.Length());

            var pixelPrice = HightInPixels / (OXY[3] - OXY[0]).Length();

            // Создание текстуры шкалы
            TTexture2D ScaleTexture = new TTexture2D();
            //            TTexture2D PlaneWithScaleTexture = new TTexture2D();
            //            ScaleTexture.CreateEmpty()
            //            ScaleTexture.CreateFromTextures(false,)
            //ScaleTexture.CutToTexture
            //                TRenderHelper.FromProjectSpace()

            Color[,] ScaleColor = new Color[WidthInPixels, HightInPixels + 2 * AdditionalPixels];
            for (int i = 0; i < AdditionalPixels; i++)
                for (int j = 0; j < WidthInPixels; j++) ScaleColor[j, i] = RGBFromHSV(240, 1, 1);

            for (int i = AdditionalPixels; i < HightInPixels + AdditionalPixels; i++)
                for (int j = 0; j < WidthInPixels; j++) ScaleColor[j, i] = RGBFromHSV(240 * (HightInPixels - i) / HightInPixels, 1, 1);

            for (int i = HightInPixels + AdditionalPixels; i < HightInPixels + 2 * AdditionalPixels; i++)
                for (int j = 0; j < WidthInPixels; j++) ScaleColor[j, i] = RGBFromHSV(0, 1, 1);
            ScaleTexture.FromColor2DArray(ScaleColor);

            // массив текстур, которые будут объединены
            System.Tuple<AstraEngine.Point, TTexture2D>[] Textures = new System.Tuple<AstraEngine.Point, TTexture2D>[NumberOfScaleMarks + 1];
            // добавляем в массив текстуру чистой шкалы
            System.Tuple<AstraEngine.Point, TTexture2D> scale =
                new System.Tuple<AstraEngine.Point, TTexture2D>
                (new AstraEngine.Point(0, 0), ScaleTexture);
            Textures[0] = scale;

            // Шаг по значению для подписи
            float ValueStep = (max - min) / (NumberOfScaleMarks - 1);
            // Шаг по пикселям, для каждой из подписи
            int markStep = (ScaleColor.GetUpperBound(1) - AdditionalPixels) / (NumberOfScaleMarks - 1);
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

            // Вычисление координат точек по котрым будут построены шкалы и надписи
            Vector3 OO = StepFromPlane + OXY[0];
            Vector3 XO = StepFromPlane + OXY[0] + (OXY[1] - OXY[0]) * ScaleWidthK;
            Vector3 OY = StepFromPlane + OXY[3];
            Vector3 XY = StepFromPlane + OXY[3] + (OXY[2] - OXY[3]) * ScaleWidthK;
            Vector3 BackOO = StepFromPlane * -1 + OXY[1];
            Vector3 BackXO = StepFromPlane * -1 + OXY[1] + (OXY[0] - OXY[1]) * ScaleWidthK;
            Vector3 BackOY = StepFromPlane * -1 + OXY[2];
            Vector3 BackXY = StepFromPlane * -1 + OXY[2] + (OXY[3] - OXY[2]) * ScaleWidthK;
            Vector2 UVOO = new Vector2(0, 0);
            Vector2 UVXO = new Vector2(1, 0);
            Vector2 UVOY = new Vector2(0, 1);
            Vector2 UVXY = new Vector2(1, 1);

            // Создание шкалы
            TModel3D Scale3D = new TModel3D(TStaticContent.Content.Game.Render);
            HelpModels.Add(Scale3D);
            Scale3D.ControlLoad.LoadModel(new List<TTriangleContainer> { new TTriangleContainer(new List<TTriangle>
            {
                new TTriangle
                {
                    P0 = OO - Y,
                    P1 = XO - Y,
                    P2 = OY + Y,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = UVOO,
                    UV1 = UVXO,
                    UV2 = UVOY
                },
                new TTriangle
                {
                    P0 = XO - Y,
                    P1 = XY + Y,
                    P2 = OY + Y,
                    N0 = Normal,
                    N1 = Normal,
                    N2 = Normal,
                    UV0 = UVXO,
                    UV1 = UVXY,
                    UV2 = UVOY
                },
                new TTriangle
                {
                    P0 = BackOO - Y,
                    P1 = BackXO - Y,
                    P2 = BackOY + Y,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = UVOO,
                    UV1 = UVXO,
                    UV2 = UVOY
                },
                new TTriangle
                {
                    P0 = BackXO - Y,
                    P1 = BackXY + Y,
                    P2 = BackOY + Y,
                    N0 = Normal * -1f,
                    N1 = Normal * -1f,
                    N2 = Normal * -1f,
                    UV0 = UVXO,
                    UV1 = UVXY,
                    UV2 = UVOY
                }
            })});
            Scale3D.SetTexture(ScaleTexture);
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Получение граничных точек плоскости в глобальных координатах отсортированные определнным образом 
        /// </summary>
        /// <param name="MinMax">Не отсортированные граничные точки плоскости</param>
        /// <returns>Граничные точки плоскости в глобальных координатах :[0] = min y; [1] = min y (>[0]); [2] = max x max y; [3] = оставшаяся точка</returns>
        private Vector3[] GetOXY((Vector3 OO, Vector3 XO, Vector3 XY, Vector3 OY) MinMax)
        {
            // Отдельная проверка на горизонтальную плоскость
            if (MinMax.OO.Y == MinMax.XO.Y && MinMax.XY.Y == MinMax.OY.Y && MinMax.OO.Y == MinMax.XY.Y)
            {
                return new Vector3[4] { MinMax.OO, MinMax.XO, MinMax.XY, MinMax.OY };
            }
            // Заносим точки с массив для удобного перебора циклами
            var OXY = new Vector3[4];
            OXY[0] = MinMax.OO;
            OXY[1] = MinMax.XO;
            OXY[2] = MinMax.OY;
            OXY[3] = MinMax.XY;
            // Массив индексов по которому далее будет собран возвращаеммый массив точек
            var ids = new byte[4] { 0, 1, 2, 3 };
            // Пузырьковая сортировка
            for (int sort = 0; sort < 4; sort++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (OXY[ids[i]].Y > OXY[ids[i + 1]].Y)
                    {
                        byte buffer = ids[i + 1];
                        ids[i + 1] = ids[i];
                        ids[i] = buffer;
                    }
                }
            }
            // Создание и заполнение массива для возврата
            var OUT = new Vector3[4];
            OUT[0] = OXY[ids[0]];
            OUT[1] = OXY[ids[1]];
            // проверка на перпендикулярность
            if (Vector3.AngleDegree(OUT[1] - OUT[0], OXY[ids[2]] - OUT[0]) > 89f)
            {
                OUT[2] = OXY[ids[3]];
                OUT[3] = OXY[ids[2]];
            }
            else
            {
                OUT[2] = OXY[ids[2]];
                OUT[3] = OXY[ids[3]];
            }
            return OUT;
        }
        //---------------------------------------------------------------
    }
}
