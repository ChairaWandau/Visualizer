// Вспомогательный класс для TViewerAero, хранящий методы для отображения полей на поверхности
using System;
using System.Collections.Generic;
//
using AstraEngine;
using AstraEngine.Engine.GraphicCore;
//
using MPT707.Aerodynamics.Structs;
//***************************************************************
namespace Example
{
    public partial class TViewerAero
    {
        //---------------------------------------------------------------
        /// <summary>
        /// Получить минимальное значение физ величены на поверхностях
        /// </summary>
        /// <param name="eTypeValueAero">Тип физической величены (давление/скорость)</param>
        /// <param name="surfaceId">Идентификаторы поверхностей</param>
        /// <returns>X - Минимум, Y - Максимум</returns>
        /// <exception cref="NotImplementedException"></exception>
        private Vector2 GetMinimumAndMaximumForSurfaces(ETypeValueAero eTypeValueAero, string[] surfaces)
        {
            // X - Минимум, Y - Максимум
            var MinMax = new Vector2();
            MinMax.X = float.MaxValue;
            MinMax.Y = float.MinValue;
            // По всем указанным в параметрах индексам поверхностям
            foreach (var surface in surfaces)
            {

                // По всем граням поверхности
                foreach (var Face in FEM_V.Surfaces.Find(x => x.ObjectName == surface).Faces)
                {
                    float Value;
                    // Получение значения в зваисимости от выбранной физ. вел.
                    switch (eTypeValueAero)
                    {
                        case ETypeValueAero.Pressure:
                            Value = Face.Pressure;
                            break;
                        case ETypeValueAero.Velocity:
                            Value = Face.VelocityModule;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    // Сравнение значения с текущим минимальным и максимальным
                    if (Value < MinMax.X)
                    {
                        MinMax.X = Value;
                    }
                    if (Value > MinMax.Y)
                    {
                        MinMax.Y = Value;
                    }
                }
            }
            // Возвращаем полученное минимально и максимальное значение
            return MinMax;
        }
        //---------------------------------------------------------------
        /// <summary>
        /// Создать контейнер треугольников по поверхности
        /// </summary>
        /// <param name="eTypeValueAero">Тип физической величены (давление/скорость)</param>
        /// <param name="id">Идентификатор поверхности</param>
        /// <returns>Поверхность</returns>
        private TTriangleContainer CreateTTriangleContainerFromSurface(ETypeValueAero eTypeValueAero, string surface, Vector2 MinMax)
        {
            // Список треугольников, который будем заполнять
            var Triangles = new List<TTriangle>();
            // По всем граням поверхности
            foreach (var face in FEM_V.Surfaces.Find(x => x.ObjectName == surface).Faces)
            {
                // Получаем UV координаты для значения в данной грани
                var UV = new Vector2();
                switch (eTypeValueAero)
                {
                    case ETypeValueAero.Pressure:
                        UV = Visualizer.GetUVCoordinate(face.Pressure, MinMax.X, MinMax.Y);
                        break;
                    case ETypeValueAero.Velocity:
                        UV = Visualizer.GetUVCoordinate(face.VelocityModule, MinMax.X, MinMax.Y);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // Количество треугольников, которое можно получить из грани
                var NumberOfTriangles = face.Nodes.Length - 2;
                // Делим грани на треугольники
                for (int i = 0; i < NumberOfTriangles; i++)
                {
                    // Делим на треугольники грани и записываем в список
                    Triangles.Add(new TTriangle
                    {
                        P0 = new Vector3(FEM_V.Nodes[face.Nodes[0]].X, FEM_V.Nodes[face.Nodes[0]].Y, FEM_V.Nodes[face.Nodes[0]].Z),
                        P1 = new Vector3(FEM_V.Nodes[face.Nodes[i + 1]].X, FEM_V.Nodes[face.Nodes[i + 1]].Y, FEM_V.Nodes[face.Nodes[i + 1]].Z),
                        P2 = new Vector3(FEM_V.Nodes[face.Nodes[i + 2]].X, FEM_V.Nodes[face.Nodes[i + 2]].Y, FEM_V.Nodes[face.Nodes[i + 2]].Z),
                        //N0 = Normal,
                        //N1 = Normal,
                        //N2 = Normal,
                        UV0 = UV,
                        UV1 = UV,
                        UV2 = UV
                    });
                }
            }
            // Возвращаем Контейнер
            return new TTriangleContainer(Triangles);
        }
//---------------------------------------------------------------
    }
}
