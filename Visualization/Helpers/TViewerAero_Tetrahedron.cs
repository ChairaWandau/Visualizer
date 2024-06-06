// Класс для преобразования точки в тетраэдр
using System;
using System.Collections.Generic;
using System.Drawing;
//
using AstraEngine;
using AstraEngine.Components;
using AstraEngine.Engine.GraphicCore;
//***************************************************************
namespace Example
{
    internal class TViewerAero_Tetrahedron
    {
        /// <summary>
        /// Преобразование точки в тетраэдр 
        /// </summary>
        /// <param name="Position"> Точка, которую надо преобразовать</param>
        /// <param name="OriginalNormal"> Направление стрелки</param>
        /// <param name="Size">Размер граней тераэра, он одинаковый, так как это правильный</param>
        /// <param name="color">Цвет тетраэдра</param>
        /// <returns>Контейнер с полигонами</returns>
        internal TTriangleContainer ConvertingPointToTetrahedron(Vector3 Position, Vector3 OriginalNormal, float Size, Vector4 color)
        {
            try
            {
                List<TTriangle> Triangles = new List<TTriangle>();
                // A, B, C  точки в основании, S - вершина
                double d = 2f / 3f;
                Vector3 A = new Vector3(0 - 0.5f * Size, (float)((double)0 - Math.Sqrt(d) * (double)Size / 3d), 0 - (float)Math.Sqrt(3) * Size / 6f);
                Vector3 B = new Vector3(0 + 0.5f * Size, 0 - (float)Math.Sqrt(d) * Size / 3, 0 - (float)Math.Sqrt(3) * Size / 6f);
                Vector3 C = new Vector3(0, 0 - (float)Math.Sqrt(d) * Size / 3, 0 + (float)Math.Sqrt(3) * Size / 3f);
                Vector3 S = new Vector3(0, 0 + (float)Math.Sqrt(d) * Size * 2 / 3, 0);

                float D = -(OriginalNormal.X * Position.X + OriginalNormal.Y * Position.Y + OriginalNormal.Z * Position.Z);
                OriginalNormal.Normalize();
                Vector3 PointInPLane;
                if (OriginalNormal.Z != 0) PointInPLane = new Vector3(0f, 0f, -(D / OriginalNormal.Z));
                else if (OriginalNormal.X != 0) PointInPLane = new Vector3(-(D / OriginalNormal.X), 0f, 0f);
                else PointInPLane = new Vector3(0f, -(D / OriginalNormal.Y), 0f);
                Vector3 NewX = PointInPLane - Position;
                if (NewX.X == 0 && NewX.Y == 0 && NewX.Z == 0)
                {
                    NewX = new Vector3(1f, 0f, 0f);
                }
                NewX.Normalize();
                Vector3 NewY = Vector3.Cross(NewX, OriginalNormal);
                NewY.Normalize();
                //Матрица перехода к новому базису
                Matrix TR = new Matrix(NewX.X, NewX.Y, NewX.Z, Position.X, OriginalNormal.X, OriginalNormal.Y, OriginalNormal.Z, Position.Y, NewY.X, NewY.Y, NewY.Z, Position.Z, 0, 0, 0, 1);
                A = Vector3.Transform(A, TR) + Position;
                B = Vector3.Transform(B, TR) + Position;
                C = Vector3.Transform(C, TR) + Position;
                S = Vector3.Transform(S, TR) + Position;

                for (int i = 0; i < 4; i++)
                {
                    var Triangle = new TTriangle();

                    if (i == 0)
                    {
                        Triangle = new TTriangle(A, B, C);
                        Vector3 Normal = new Vector3();
                        Normal = Vector3.Cross(A - B, B - C);
                        Normal.Normalize();
                        Triangle.N0 = Normal;
                        Triangle.N1 = Normal;
                        Triangle.N2 = Normal;
                    }
                    if (i == 1)
                    {
                        Triangle = new TTriangle(S, B, A);
                        Vector3 Normal = new Vector3();
                        Normal = Vector3.Cross(S - A, B - S);
                        Normal.Normalize();
                        Triangle.N0 = Normal;
                        Triangle.N1 = Normal;
                        Triangle.N2 = Normal;
                    }
                    if (i == 2)
                    {
                        Triangle = new TTriangle(S, A, C);
                        Vector3 Normal = new Vector3();
                        Normal = Vector3.Cross(S - C, A - S);
                        Normal.Normalize();
                        Triangle.N0 = Normal;
                        Triangle.N1 = Normal;
                        Triangle.N2 = Normal;
                    }
                    if (i == 3)
                    {
                        Triangle = new TTriangle(S, C, B);
                        Vector3 Normal = new Vector3();
                        Normal = Vector3.Cross(S - B, C - S);
                        Normal.Normalize();
                        Triangle.N0 = Normal;
                        Triangle.N1 = Normal;
                        Triangle.N2 = Normal;
                    }
                    Triangles.Add(Triangle);
                }
                // Лист с треугольникмми (гранями) отправляется в контейнер
                TTriangleContainer Tetra = new TTriangleContainer(Triangles);
                //Задание цвета 
                int alpha = (int)color.W;
                int red = (int)color.X;
                int green = (int)color.Y;
                int blue = (int)color.Z;
                Tetra.Colour = Color.FromArgb(alpha, red, green, blue);

                return Tetra;
            
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0007: Error TViewerAero_Tetrahedron:ConvertingPointToTetrahedron(): " + E.Message);
                return null;
            }
        }

    }
}




