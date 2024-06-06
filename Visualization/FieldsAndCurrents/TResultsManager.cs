// Сохранение, чтение и удаление результатов расчета для визуализации/Создания анимации
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
//*******************************************************************
namespace Example
{
    public class TResultsManager
    {
        /// <summary>
        /// Результаты
        /// </summary>
        private List<string> Results = new List<string>();
        /// <summary>
        /// Счетчик запомненных результатов (для создания файлов с соответствующим названием, по мере сохранения новых результатов)
        /// </summary>
        private int RememberedResultsCounter = 0;
        /// <summary>
        /// Название папки в которой будет все храниться
        /// </summary>
        private string FolderName = "LastSessionResults";
        /// <summary>
        /// Путь до папки в которой хранятся сохраненные расчеты с "\\" на конце
        /// </summary>
        private string PathToFolder { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + FolderName + "\\"; } }
        /// <summary>
        /// Существует ли целевая папка
        /// </summary>
        private bool FolderExist { get { return System.IO.Directory.Exists(PathToFolder); } }
        /// <summary>
        /// Получить количество запомненных результатов
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfResults { get { return Results.Count; } }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Запомни результат
        /// </summary>
        /// <param name="FEM_V">Сетка со значениями по которой можно строить визуализацию</param>
        public void RememberResult(TFiniteElementModel_Visual FEM_V)
        {
            // Добавляем список путь до результата
            Results.Add(PathToFolder + RememberedResultsCounter.ToString() + ".result");
            // Инкрементируем счетчик запомненных расчетов
            this.RememberedResultsCounter++;
            // Отправляем результат на запись
            WriteResultInFile(FEM_V);
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Записать результат в файл
        /// </summary>
        /// <param name="FEM_V">Конечно элементная модель адаптированная под визуализацию</param>
        private void WriteResultInFile(TFiniteElementModel_Visual FEM_V)
        {
            using (BinaryWriter BW = new BinaryWriter(File.Open(Results[Results.Count - 1], FileMode.OpenOrCreate)))
            {
                // == Записываем ноды ==
                // Количество нод
                BW.Write(FEM_V.Nodes.Length);
                // Записываем все ноды
                for (int i = 0; i < FEM_V.Nodes.Length; i++)
                {
                    BW.Write(FEM_V.Nodes[i].X);
                    BW.Write(FEM_V.Nodes[i].Y);
                    BW.Write(FEM_V.Nodes[i].Z);
                }

                // == Записываем элементы ==
                // Количество элементов
                BW.Write(FEM_V.Elements.Length);
                // Записываем все элементы
                for (int i = 0; i < FEM_V.Elements.Length; i++)
                {
                    // Записываем количество нод
                    BW.Write(FEM_V.Elements[i].Nodes.Length);
                    // Записываем индексы нод
                    for (int j = 0; j < FEM_V.Elements[i].Nodes.Length; j++)
                    {
                        BW.Write(FEM_V.Elements[i].Nodes[j]);
                    }
                    // Записываем давление
                    BW.Write(FEM_V.Elements[i].Pressure);
                    // Записываем скорость по трем компонентам
                    BW.Write(FEM_V.Elements[i].Velocity.X);
                    BW.Write(FEM_V.Elements[i].Velocity.Y);
                    BW.Write(FEM_V.Elements[i].Velocity.Z);
                    // Записываем модуль скорости
                    BW.Write(FEM_V.Elements[i].VelocityModule);
                    // Записываем позицию центра элемента
                    BW.Write(FEM_V.Elements[i].Position.X);
                    BW.Write(FEM_V.Elements[i].Position.Y);
                    BW.Write(FEM_V.Elements[i].Position.Z);
                }

                // == Записываем поверхности ==
                // Записываем количество поверхностей
                BW.Write(FEM_V.Surfaces.Count);
                // Записываем все поверхности
                for (int i = 0; i < FEM_V.Surfaces.Count; i++)
                {
                    // Название поверхности
                    BW.Write(FEM_V.Surfaces[i].ObjectName);
                    // Записываем количество полигонов
                    BW.Write(FEM_V.Surfaces[i].Faces.Length);
                    // Записываем все полигоны
                    for (int j = 0; j < FEM_V.Surfaces[i].Faces.Length; j++)
                    {
                        // Давление
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Pressure);
                        // Скорость по ее компонентам
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Velocity.X);
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Velocity.Y);
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Velocity.Z);
                        // Модуль скорости
                        BW.Write(FEM_V.Surfaces[i].Faces[j].VelocityModule);
                        // Позиция центра полигона по трем координатам
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Position.X);
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Position.Y);
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Position.Z);
                        // Записываем количество нод
                        BW.Write(FEM_V.Surfaces[i].Faces[j].Nodes.Length);
                        // Записываем все идентификаторы нод
                        for (int k = 0; k < FEM_V.Surfaces[i].Faces[j].Nodes.Length; k++)
                        {
                            BW.Write(FEM_V.Surfaces[i].Faces[j].Nodes[k]);
                        }
                    }
                }
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Запрос результата
        /// </summary>
        /// <param name="id">Номер запомненного результата</param>
        /// <returns>Сетка со значениями по которой можно строить визуализацию</returns>
        public TFiniteElementModel_Visual RecallResult(int id)
        {
            try
            {
                // Создаем объект конечно элементной модели для визуализации в которую будем все писать
                var FEM_V = new TFiniteElementModel_Visual();
                // Читаем
                using (BinaryReader BR = new BinaryReader(File.Open(Results[id], FileMode.Open)))
                {
                    // == Читаем ноды ==
                    // Создаем массив нодов соответствующей длинны
                    FEM_V.Nodes = new TFemNode_Visual[BR.ReadInt32()];
                    // Читаем все ноды
                    for (int i = 0; i < FEM_V.Nodes.Length; i++)
                    {
                        // Создаем саму ноду
                        FEM_V.Nodes[i] = new TFemNode_Visual();
                        // Пишем координаты позиции
                        FEM_V.Nodes[i].X = BR.ReadSingle();
                        FEM_V.Nodes[i].Y = BR.ReadSingle();
                        FEM_V.Nodes[i].Z = BR.ReadSingle();
                    }

                    // == Читаем элементы ==
                    // Создаем массив элементов
                    FEM_V.Elements = new TFemElement_Visual[BR.ReadInt32()];
                    // Читаем все элементы
                    for (int i = 0; i < FEM_V.Elements.Length; i++)
                    {
                        FEM_V.Elements[i] = new TFemElement_Visual();
                        // Читаем количество нод
                        FEM_V.Elements[i].Nodes = new int[BR.ReadInt32()];
                        // Читаем индексы нод
                        for (int j = 0; j < FEM_V.Elements[i].Nodes.Length; j++)
                        {
                            FEM_V.Elements[i].Nodes[j] = BR.ReadInt32();
                        }
                        // Читаем давление
                        FEM_V.Elements[i].Pressure = BR.ReadSingle();
                        // Читаем скорость по трем компонентам
                        FEM_V.Elements[i].Velocity = new AstraEngine.Vector3();
                        FEM_V.Elements[i].Velocity.X = BR.ReadSingle();
                        FEM_V.Elements[i].Velocity.Y = BR.ReadSingle();
                        FEM_V.Elements[i].Velocity.Z = BR.ReadSingle();
                        // Читаем модуль скорости
                        FEM_V.Elements[i].VelocityModule = BR.ReadSingle();
                        // Читаем позицию центра элемента
                        FEM_V.Elements[i].Position = new AstraEngine.Vector3();
                        FEM_V.Elements[i].Position.X = BR.ReadSingle();
                        FEM_V.Elements[i].Position.Y = BR.ReadSingle();
                        FEM_V.Elements[i].Position.Z = BR.ReadSingle();
                    }

                    // == Читаем поверхности ==
                    int SurfacesCount = BR.ReadInt32();
                    for (int i = 0; i < SurfacesCount; i++)
                    {
                        // Создаем кортеж, который будем записывать в объект
                        (string ObjectName, TFemFace_Visual[] Faces) Tuple;
                        // Заполняем название поверхности
                        Tuple.ObjectName = BR.ReadString();
                        // Создаем массив полигонов соответствующего размера
                        Tuple.Faces = new TFemFace_Visual[BR.ReadInt32()];
                        // Читаем все полигоны
                        for(int j = 0; j < Tuple.Faces.Length; j++)
                        {
                            // Создаем объект полигона для заполнения
                            Tuple.Faces[j] = new TFemFace_Visual();
                            // Читаем давление
                            Tuple.Faces[j].Pressure = BR.ReadSingle();
                            // Создаем объект для записи скорости
                            Tuple.Faces[j].Velocity = new AstraEngine.Vector3();
                            // Читаем компоненты скорости
                            Tuple.Faces[j].Velocity.X = BR.ReadSingle();
                            Tuple.Faces[j].Velocity.Y = BR.ReadSingle();
                            Tuple.Faces[j].Velocity.Z = BR.ReadSingle();
                            // Читаем модуль скорости
                            Tuple.Faces[j].VelocityModule = BR.ReadSingle();
                            // Создаем объект для записи позиции
                            Tuple.Faces[j].Position = new AstraEngine.Vector3();
                            // Читаем координаты позиции
                            Tuple.Faces[j].Position.X = BR.ReadSingle();
                            Tuple.Faces[j].Position.Y = BR.ReadSingle();
                            Tuple.Faces[j].Position.Z = BR.ReadSingle();
                            // Читаем количество нод
                            Tuple.Faces[j].Nodes = new int[BR.ReadInt32()];
                            // Читаем идентификаторы нод
                            for (int k = 0; k < Tuple.Faces[j].Nodes.Length; k++)
                            {
                                Tuple.Faces[j].Nodes[k] = BR.ReadInt32();
                            }
                        }
                        // Добавляем заполненную поверхность
                        FEM_V.Surfaces.Add(Tuple);
                    }
                }
                return FEM_V;
            }
            catch
            {
                return new TFiniteElementModel_Visual();
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Забыть все результаты
        /// </summary>
        public void ForgetResults()
        {
            // Если папка существует - чистим ее
            if (FolderExist)
            {
                // Удаляем папку рекурсивно
                Directory.Delete(PathToFolder, true);
                // Создаем ее заново
                Directory.CreateDirectory(PathToFolder);
                // Очистка спискапутей к результатам
                this.Results.Clear();
                // Сброс счетчика
                this.RememberedResultsCounter = 0;
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Забыть конкретный результат
        /// </summary>
        /// <param name="id">Порядковый номер хранимого расчета</param>
        public void ForgetResult(int id)
        {
            if (id < 0 || id >= this.RememberedResultsCounter) { return; }
            // если файл - последний записанный, то удаление немного другое
            if (id == this.RememberedResultsCounter - 1)
            {
                RememberedResultsCounter--;
                // Удаляем файл с записанными результатами расчета
                File.Delete(Results[id]);
                // Удаляем путь к файлу
                Results.RemoveAt(id);
            }
            else
            {
                // Удаляем файл с записанными результатами расчета
                File.Delete(Results[id]);
                // Удаляем путь к файлу
                Results.RemoveAt(id);
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// создать объект управляющий сохранением и чтением сохраненных расчетов
        /// </summary>
        public TResultsManager()
        {
            // Создаем папку если ее нет
            if (!FolderExist) { Directory.CreateDirectory(PathToFolder); }
            else
            {
                // Если папка есть, то проверяем есть ли там файлы, если есть файлы, то спрашиваем нужно ли их восстановить, если нет, то чистим папку
                //CheckForOldResultsAndRestore();
                ForgetResults();
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выдача окна в котором задается вопрос о востановлении
        /// </summary>
        /// <returns></returns>
        private bool AskForRestoration()
        {
            DialogResult result=MessageBox.Show(
            "Восстановить ранее записанные результаты расчетов?\n(При нажатии на кнопку \"Нет\" старые результаты расчетов будут удалены)",
            "Восстановление результатов расчетов",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.DefaultDesktopOnly);

            if (result == DialogResult.Yes) return true;
            else return false;
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Проверить существуют ли в папке старые результаты, если нужно, то восстановить
        /// </summary>
        /// <returns></returns>
        private void CheckForOldResultsAndRestore()
        {
            int MaxFileIndex = 0;
            // Получаем все файлы хранящиеся в папке
            string[] files = Directory.GetFiles(PathToFolder);
            // Проверяем наличие файлов папке
            if (files.Length > 0)
            {
                // Если файлы есть - спрашиваем о востановлении
                if (!AskForRestoration())
                {
                    ForgetResults();
                    return;
                }
            }
            else
            {
                return;
            }

            // Список существующих файлов
            var ExistingFiles = new List<int>();
            // Записываем подходящие файлы, удаляем мусор
            foreach (var file in files)
            {
                // Название файла без расширения и пути
                var fileName = Path.GetFileNameWithoutExtension(file);
                var fileIndex = -1;
                if (int.TryParse(fileName, out fileIndex))
                {
                    // Записываем подходящий индекс файла
                    ExistingFiles.Add(fileIndex);
                    // Обновляем максимальный найденый индекс фйала
                    if (fileIndex > MaxFileIndex) MaxFileIndex = fileIndex;
                }
                else
                {
                    // Если файл не подходящий - удаляем
                    File.Delete(file);
                }
            }

            // Востанавливаем значение счетчика сохраненных расчетов
            this.RememberedResultsCounter = MaxFileIndex + 1;
            
            // Сортируем по возростанию
            ExistingFiles.Sort();

            // Востанавливаем пути к файлам
            foreach (int file in ExistingFiles)
            {
                this.Results.Add(PathToFolder + file.ToString() + ".result");
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
