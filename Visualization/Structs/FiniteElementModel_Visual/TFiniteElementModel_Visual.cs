// Описание конечно-элементной модели
using System;
using System.Collections.Generic;
//
using AstraEngine.Components;
//******************************************************************
namespace Example
{
    /// <summary>
    /// Описание конечно-элементной модели
    /// </summary>
    public class TFiniteElementModel_Visual
    {
        /// <summary>
        /// 
        /// </summary>
        public TFemElement_Visual[] Elements { get; set; } = new TFemElement_Visual[0];
        /// <summary>
        /// 
        /// </summary>
        public List<(string ObjectName, TFemFace_Visual[] Faces)> Surfaces =
           new List<(string ObjectName, TFemFace_Visual[] Faces)>();
        /// <summary>
        /// Массив вершин
        /// </summary>
        public TFemNode_Visual[] Nodes = new TFemNode_Visual[0];
//----------------------------------------------------------------
        /// <summary>
        /// Удалить
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
//----------------------------------------------------------------
        /// <summary>
        /// Сгенерировать для отрисовки конечно-элементной модели Grid
        /// </summary>
        public void DebugDraw()
        {
            try
            {
                //// Создаем если нет
                //if (Grid == null)
                //{
                //    Grid = new TModel3D(TStaticContent.Content.Game.Render);
                //}
                //// Заполняем меш треугольниками
                //TTriangleContainer Mesh = new TTriangleContainer();
                //// Идем по всем элементам и строим отрезки
                //foreach (var Element in Elements)
                //{
                //    List<Vector3> PointsTetrahedron = new List<Vector3>();
                //    foreach (var Node in Element.ID_Nodes)
                //    {
                //        var SelectNode = Nodes.Find(X => (X.ID_Node == Node));
                //        if (SelectNode == null)
                //        {
                //            continue;
                //        }
                //        PointsTetrahedron.Add(new Vector3((float)SelectNode.X, (float)SelectNode.Y, (float)SelectNode.Z));
                //    }
                //    if (PointsTetrahedron.Count != 4)
                //    {
                //        continue;
                //    }
                //    // Четыре треугольника составляют тетраэдр
                //    Mesh.Add(PointsTetrahedron[0], PointsTetrahedron[1], PointsTetrahedron[2]);
                //    Mesh.Add(PointsTetrahedron[0], PointsTetrahedron[1], PointsTetrahedron[3]);
                //    Mesh.Add(PointsTetrahedron[0], PointsTetrahedron[3], PointsTetrahedron[2]);
                //    Mesh.Add(PointsTetrahedron[1], PointsTetrahedron[2], PointsTetrahedron[3]);
                //    //
                //}
                //// Создаем из меша модель
                //Grid.ControlPolygons.CreateFromPolygons(Mesh, true);
                //// Задаем параметры - отрисовывать как сетку, отрисовывать с двух сторон
                //Grid.ControlDraw.Enable_WireFrame = true;
                //Grid.ControlDraw.Enable_TransparentMode = false;
                //// Задаем параметры освещения
                //Grid.ControlLight.SetAmbientLight(Color.White, 0.6f);
                //Grid.ControlLight.AddDirectionLight(Color.White, 1.5f, new Vector3(1000, 1000, 1000));
                //Grid.SetColour(Color.Red);
                throw new NotImplementedException();
            }
            catch(Exception E)
            {
                TJournalLog.WriteLog("C0087: Error TFiniteElementModel_Visual:DebugDraw(): " + E.Message);
            }
        }
//----------------------------------------------------------------
        /// <summary>
        /// Выполнить запись в структуру значений по нодам
        /// </summary>
        /// <param name="ID_Nodes">Идентификаторы нодов</param>
        /// <param name="XYZ">Координаты нодов</param>
        public void WriteNodes(long[] ID_Nodes, double[] XYZ)
        {
            try
            {
                //Nodes.Clear();
                ////
                //int IndexXYZ = 0;
                //for (int I = 0; I < ID_Nodes.Length; I++)
                //{
                //    TFemNode_Transformable FemNode = new TFemNode_Transformable();
                //    FemNode.ID_Node = ID_Nodes[I];
                //    FemNode.X = XYZ[IndexXYZ];
                //    IndexXYZ = IndexXYZ + 1;
                //    FemNode.Y = XYZ[IndexXYZ];
                //    IndexXYZ = IndexXYZ + 1;
                //    FemNode.Z = XYZ[IndexXYZ];
                //    IndexXYZ = IndexXYZ + 1;
                //    Nodes.Add(FemNode);
                //}
                throw new NotImplementedException();
            }
            catch(Exception E)
            {
                TJournalLog.WriteLog("C0052: Error TFiniteElementModel_Visual:WriteNodes(): " + E.Message);
            }
        }
//----------------------------------------------------------------
        /// <summary>
        /// Выполнить запись в структуру значений по элементам (тетраэдрам - tetrahedron)
        /// </summary>
        /// <param name="Types">Тип элементов</param>
        /// <param name="ID_Elements">Идентификаторы элементов</param>
        /// <param name="ID_InputNodes">Ссылки на идентификаторы нодов образующих элементы</param>
        public void WriteElements(int[] Types, long[][] ID_Elements, long[][] ID_InputNodes)
        {
            try
            {
                //// Получаем требуемый тип элементов
                //var IndexPrism = Types.ToList().FindIndex(X => (X == 4));
                //if (IndexPrism < 0)
                //{
                //    TJournalLog.WriteLog("C0052: Error TFiniteElementModel_Transformable:WriteElements(): IndexPrism < 0, unsupported type elements of finite element model - use only tetrahedron !");
                //    return;
                //}
                //// 
                //int CountElements = ID_Elements[IndexPrism].Length;
                //var NodesElements = ID_InputNodes[IndexPrism];
                ////
                //Elements.Clear();
                //// Парсим элементы
                //int IndexXYZ = 0;
                //for (int I = 0; I < CountElements; I++)
                //{
                //    TFemElement FemElements = new TFemElement();
                //    FemElements.ID_Element = ID_Elements[IndexPrism][I];
                //    FemElements.Type = IndexPrism;
                //    //
                //    FemElements.ID_Nodes.Add(NodesElements[IndexXYZ]);
                //    IndexXYZ = IndexXYZ + 1;
                //    FemElements.ID_Nodes.Add(NodesElements[IndexXYZ]);
                //    IndexXYZ = IndexXYZ + 1;
                //    FemElements.ID_Nodes.Add(NodesElements[IndexXYZ]);
                //    IndexXYZ = IndexXYZ + 1;
                //    FemElements.ID_Nodes.Add(NodesElements[IndexXYZ]);
                //    IndexXYZ = IndexXYZ + 1;
                //    Elements.Add(FemElements);
                //}
                throw new NotImplementedException();
            }
            catch (Exception E)
            {
                TJournalLog.WriteLog("C0098: Error TFiniteElementModel_Visual:WriteElements(): " + E.Message);
            }
        }
//----------------------------------------------------------------
    }
}
