using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Excel.Common
{
    /// <summary>
    /// EPPlus公用帮助类（导入导出时，如需改变表头文字，在相应属性上使用Description特性来描述文字）
    /// </summary>
    public class EPPlusCommon
    {

        #region 由List创建简单Exel.列头取属性的Description或属性名
        /// <summary>
        /// （单个索引页）由List创建简单Exel.列头取T属性的Description或属性名 单个索引页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="sheetName">索引页名称</param>
        public static async Task CreateExcelByList<T>(string filePath, List<T> dataList, string sheetName = "sheet1") where T : class
        {

            string dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string fileName = Path.GetFileName(filePath);
            FileInfo newFile = new FileInfo(filePath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(filePath);
            }

            PropertyInfo[] properties = null;
            if (dataList.Count > 0)
            {
                Type type = dataList[0].GetType();
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var filedDescriptions = GetDescriptions<T>();//字段与excel列名对应关系
                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
                    SetExcelWorksheetStyle(worksheet, properties.Length);
                    int row = 1, col;
                    object objColValue;
                     //表头
                    for (int j = 0; j < properties.Length; j++)
                    {
                        row = 1;
                        col = j + 1;
                        var description = filedDescriptions.Where(o => o.Key == properties[j].Name).Select(o => o.Value).FirstOrDefault();
                        worksheet.Cells[row, col].Value = description; 
                    }
                    worksheet.View.FreezePanes(row + 1, 1); //冻结表头
                    //各行数据
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        row = i + 2;
                        for (int j = 0; j < properties.Length; j++)
                        {
                            col = j + 1;
                            objColValue = properties[j].GetValue(dataList[i], null);
                            ListSetValueByType(worksheet, row, col, objColValue);
                         }
                    }
                    await package.SaveAsync();
                }

            }
        }

        /// <summary>
        /// （多个索引页）由List创建简单Exel.列头取T属性的Description或属性名  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="dataListDic">Key 索引页名称，Value 索引页数据集合</param>
        public static async Task CreateExcelByList(string filePath, Dictionary<string, IEnumerable<object>> dataListDic)
        {

            string dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string fileName = Path.GetFileName(filePath);
            FileInfo newFile = new FileInfo(filePath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(filePath);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                foreach (var item in dataListDic)
                {
                    PropertyInfo[] properties = null;
                    Type type = item.Value.ToArray()[0].GetType();

                    properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var filedDescriptions = GetDescriptions(type);//字段与excel列名对应关系
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(item.Key);
                    //设置表头单元格格式
                    SetExcelWorksheetStyle(worksheet, properties.Length);

                    int row = 1, col;
                    object objColValue;
                     //表头
                    for (int j = 0; j < properties.Length; j++)
                    {
                        row = 1;
                        col = j + 1;
                        var description = filedDescriptions.Where(o => o.Key == properties[j].Name).Select(o => o.Value).FirstOrDefault();
                        
                        worksheet.Cells[row, col].Value = description;
                    }
                    worksheet.View.FreezePanes(row + 1, 1); //冻结表头
                                                            //各行数据
                    for (int i = 0; i < item.Value.Count(); i++)
                    {
                        row = i + 2;
                        for (int j = 0; j < properties.Length; j++)
                        {
                            col = j + 1;
                            objColValue = properties[j].GetValue(item.Value.ToArray()[i], null);
                            ListSetValueByType(worksheet, row, col, objColValue);

                        }
                    }
                }
                await package.SaveAsync();
            }


        }



        #endregion

        #region 由Dictionary创建简单Exel.列头取属性的Description或属性名
        /// <summary>
        /// （单个索引页）由Dictionary创建简单Exel.列头取T属性的Description或属性名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="dataDic">The data Dic  key=F1 value=1.</param>
        /// <param name="sheetName">索引页名称</param>
        public static async Task CreateExcelByDictionary<T>(string filePath, Dictionary<string, object> dataDic, string sheetName = "sheet1") where T : class
        {

            string dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            string fileName = Path.GetFileName(filePath);
            FileInfo newFile = new FileInfo(filePath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(filePath);
            }
            PropertyInfo[] properties = null;
            if (dataDic.Count > 0)
            {
                Type type = typeof(T);
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var filedDescriptions = GetDescriptions<T>();//字段与excel列名对应关系
                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);

                    //设置表头单元格格式
                    SetExcelWorksheetStyle(worksheet, properties.Length);

                    int row = 1, col;
                    //表头
                    for (int j = 0; j < properties.Length; j++)
                    {
                        row = 1;
                        col = j + 1;
                        var description = filedDescriptions.Where(o => o.Key == properties[j].Name).Select(o => o.Value).FirstOrDefault();
                        worksheet.Cells[row, col].Value = description;

                    }
                    worksheet.View.FreezePanes(row + 1, 1); //冻结表头
                    //各行数据
                    foreach (var item in dataDic)
                    {
                        DictionarySetValueByType(worksheet,item);                  
                    }
                    await package.SaveAsync();
                }
            }
        }





        /// <summary>
        /// （多个个索引页）由Dictionary创建简单Exel.列头取Type属性的Description或属性名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="dataDicDic">Key  索引页名称  Value 坐标和数据key=F1 value=1</param>
        /// <param name="types">索引页相对于的实体类型</param>
        public static async Task CreateExcelByDictionary(string filePath, Dictionary<string, Dictionary<string, object>> dataDicDic, Type[] types)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            FileInfo newFile = new FileInfo(filePath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(filePath);
            }
            PropertyInfo[] properties = null;


            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                int i = 0;
                foreach (var item in dataDicDic)
                {
                    Type type = types[i];
                    properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var filedDescriptions = GetDescriptions(type);//字段与excel列名对应关系
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(item.Key);

                    //设置表头单元格格式
                    SetExcelWorksheetStyle(worksheet, properties.Length);

                    int row = 1, col;
                    //表头
                    for (int j = 0; j < properties.Length; j++)
                    {
                        row = 1;
                        col = j + 1;
                        var description = filedDescriptions.Where(o => o.Key == properties[j].Name).Select(o => o.Value).FirstOrDefault();
                        worksheet.Cells[row, col].Value = description;
                    }
                    worksheet.View.FreezePanes(row + 1, 1); //冻结表头
                    //各行数据
                    foreach (var itemValue in item.Value)
                    {
                        DictionarySetValueByType(worksheet, itemValue);
                    }
                    i++;
                }
                await package.SaveAsync();
            }
        }
        #endregion

        #region 继续写入Excel
        /// <summary>
        /// 通过Byte继续写入Excel,返回Byte数组
        /// </summary>
        /// <param name="bytes">Excel Byte数组</param>
        /// <param name="dataDic">数据</param>
        /// <param name="sheetIndex">索引页</param>
        /// <param name="sheetNameName">索引页名称</param>
        /// <returns></returns>
        public static async Task<byte[]> ContinueWriteExcelByByte(byte[] bytes, Dictionary<string, object> dataDic, int sheetIndex = 0, string sheetNameName = null)
        {
            using (var stream = new MemoryStream())
            {
                using (ExcelPackage package = new ExcelPackage(new MemoryStream(bytes)))
                {
                    var excelWorksheet = package.Workbook.Worksheets[sheetIndex];
                    excelWorksheet.Name = sheetNameName == null ? excelWorksheet.Name : sheetNameName;
                    foreach (var item in dataDic)
                    {
                        DictionarySetValueByType(excelWorksheet, item);
                    }
                    await package.SaveAsAsync(stream);
                    return stream.ToArray();
                }
            }
        }


        /// <summary>
        /// 通过文件继续写入Excel 
        /// </summary>
        /// <param name="bytes">Excel Byte数组</param>
        /// <param name="dataDic">数据</param>
        /// <param name="sheetIndex">索引页</param>
        /// <param name="sheetNameName">索引页名称</param>
        /// <returns></returns>
        public static async Task ContinueWriteExcelByByte(string filePath, Dictionary<string, object> dataDic, int sheetIndex = 0, string sheetNameName = null)
        {
            FileInfo newFile = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                var excelWorksheet = package.Workbook.Worksheets[sheetIndex];
                excelWorksheet.Name = sheetNameName == null ? excelWorksheet.Name : sheetNameName;

                foreach (var item in dataDic)
                {
                    DictionarySetValueByType(excelWorksheet, item);
                }
                await package.SaveAsync();
            }
        }

        #endregion

        #region 获取类型中属性的描述文字
        public static Dictionary<string, string> GetDescriptions<T>()
        {
            var type = typeof(T);
            var propertyInfos = type.GetProperties();
            Dictionary<string, string> dicDescriptions = new Dictionary<string, string>();
            propertyInfos.ToList().ForEach(m => { dicDescriptions.Add(m.Name, m.IsDefined(typeof(DescriptionAttribute), true) == true ? (m.GetCustomAttribute(typeof(DescriptionAttribute), true) as DescriptionAttribute).Description : m.Name); });
            return dicDescriptions;
        }

        public static Dictionary<string, string> GetDescriptions(Type type)
        {

            var propertyInfos = type.GetProperties();
            Dictionary<string, string> dicDescriptions = new Dictionary<string, string>();
            propertyInfos.ToList().ForEach(m => { dicDescriptions.Add(m.Name, m.IsDefined(typeof(DescriptionAttribute), true) == true ? (m.GetCustomAttribute(typeof(DescriptionAttribute), true) as DescriptionAttribute).Description : m.Name); });
            return dicDescriptions;
        }
        #endregion

        #region Excel转换为IEnumerable<T>
        /// <summary>
        /// 从Excel中加载数据（泛型）
        /// </summary>
        /// <typeparam name="T">每行数据的类型</typeparam>
        /// <param name="FileName">Excel文件名</param>
        /// <returns>泛型列表</returns>
        public static IEnumerable<T> GetListFromExcel<T>(FileInfo existingFile) where T : new()
        {
            //FileInfo existingFile = new FileInfo(FileName);//如果本地地址可以直接使用本方法，这里是直接拿到了文件
            List<T> resultList = new List<T>();
            Dictionary<string, int> dictHeader = new Dictionary<string, int>();

            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                int colStart = worksheet.Dimension.Start.Column;  //工作区开始列
                int colEnd = worksheet.Dimension.End.Column;       //工作区结束列
                int rowStart = worksheet.Dimension.Start.Row;       //工作区开始行号
                int rowEnd = worksheet.Dimension.End.Row;       //工作区结束行号

                //将每列标题添加到字典中
                for (int i = colStart; i <= colEnd; i++)
                {
                    dictHeader[worksheet.Cells[rowStart, i].Value.ToString()] = i;
                }

                List<PropertyInfo> propertyInfoList = new List<PropertyInfo>(typeof(T).GetProperties());

                for (int row = rowStart + 1; row <= rowEnd; row++)
                {
                    T result = new T();

                    var nameCellNameDic = GetDescriptions<T>();
                    //为对象T的各属性赋值
                    foreach (PropertyInfo p in propertyInfoList)
                    {
                        try
                        {
                            ExcelRange cell = worksheet.Cells[row, dictHeader[nameCellNameDic[p.Name]]]; //与属性名对应的单元格

                            if (cell.Value == null)
                                continue;
                            switch (p.PropertyType.Name.ToLower())
                            {
                                case "string":
                                    p.SetValue(result, cell.GetValue<String>());
                                    break;
                                case "int16":
                                    p.SetValue(result, cell.GetValue<Int16>());
                                    break;
                                case "int32":
                                    p.SetValue(result, cell.GetValue<Int32>());
                                    break;
                                case "int64":
                                    p.SetValue(result, cell.GetValue<Int64>());
                                    break;
                                case "decimal":
                                    p.SetValue(result, cell.GetValue<Decimal>());
                                    break;
                                case "double":
                                    p.SetValue(result, cell.GetValue<Double>());
                                    break;
                                case "datetime":
                                    p.SetValue(result, cell.GetValue<DateTime>());
                                    break;
                                case "boolean":
                                    p.SetValue(result, cell.GetValue<Boolean>());
                                    break;
                                case "byte":
                                    p.SetValue(result, cell.GetValue<Byte>());
                                    break;
                                case "char":
                                    p.SetValue(result, cell.GetValue<Char>());
                                    break;
                                case "single":
                                    p.SetValue(result, cell.GetValue<Single>());
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    resultList.Add(result);
                }
            }
            return resultList;
        }
        #endregion

        #region 通用功能
        /// <summary>
        /// 字典根据类型给单元格赋值（包括公式） 
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="item">值</param>
        public static void DictionarySetValueByType(ExcelWorksheet worksheet, KeyValuePair<string, object> item)
        {

            switch (GetValueByType(item.Value))
            {
                case TypeEnum.Int:
                    worksheet.Cells[item.Key].Value = item.Value.ToInt();

                    break;
                case TypeEnum.String:
                    worksheet.Cells[item.Key].Value = item.Value.ToString();

                    break;
                case TypeEnum.Float:
                    worksheet.Cells[item.Key].Value = item.Value.ToDecimal();

                    break;
                case TypeEnum.Decimal:
                    worksheet.Cells[item.Key].Value = item.Value.ToDecimal();

                    break;
                case TypeEnum.Bool:
                    worksheet.Cells[item.Key].Value = Convert.ToBoolean(item.Value);
                    break;
                case TypeEnum.DateTime:
                    worksheet.Cells[item.Key].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    worksheet.Cells[item.Key].Value = item.Value.ToDateTime();

                    break;
                case TypeEnum.Formula:
                    worksheet.Cells[item.Key].Formula = ((MyFormula)item.Value).Formula;
                    break;
                default:
                    worksheet.Cells[item.Key].Value = item.Value.ToString();
                    break;
            }

        }



        /// <summary>
        /// 集合根据类型给单元格赋值（包括公式） 
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <param name="colValue">值</param>
        public static void ListSetValueByType(ExcelWorksheet worksheet, int row, int col, object colValue)
        {

            switch (GetValueByType(colValue))
            {
                case TypeEnum.Int:
                    worksheet.Cells[row, col].Value = colValue.ToInt();

                    break;
                case TypeEnum.String:
                    worksheet.Cells[row, col].Value = colValue.ToString();

                    break;
                case TypeEnum.Float:
                    worksheet.Cells[row, col].Value = colValue.ToDecimal();

                    break;
                case TypeEnum.Decimal:

                    worksheet.Cells[row, col].Value = colValue.ToDecimal();


                    break;
                case TypeEnum.Bool:
                    worksheet.Cells[row, col].Value = Convert.ToBoolean(colValue);

                    break;
                case TypeEnum.DateTime:
                    worksheet.Cells[row, col].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    worksheet.Cells[row, col].Value = colValue.ToDateTime();

                    break;
                case TypeEnum.Formula:
                    worksheet.Cells[row, col].Formula = ((MyFormula)colValue).Formula;
                    break;
                default:
                    worksheet.Cells[row, col].Value = colValue.ToString();
                    break;
            }

        }



        /// <summary>
        /// 设置表头和通用样式
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="length">表头长度</param>
        public static void SetExcelWorksheetStyle(ExcelWorksheet worksheet, int length)
        {
            worksheet.DefaultColWidth = 25; //默认列宽
                                            // worksheet.DefaultRowHeight = 10; //默认行高
            worksheet.TabColor = Color.Blue; //Sheet Tab的颜色
            worksheet.Cells.Style.WrapText = true; //单元格文字自动换行

            //设置表头单元格格式
            using (var range = worksheet.Cells[1, 1, 1, length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                range.Style.Font.Color.SetColor(Color.White);
            }

            //设置指定行或列的样式(宽、高、隐藏、自动换行、数字格式、锁定等)：

            //worksheet.Column(1).Width = 10;
            //worksheet.Row(1).Height = 30;

            //worksheet.Column(1).Hidden = true;
            //worksheet.Row(1).Hidden = true;

            //worksheet.Column(1).Style.WrapText = true;

            //worksheet.Column(1).Style.Numberformat.Format = "$#,###.00";

            //worksheet.Row(1).Style.Locked = true;

        }


        /// <summary>
        /// 获取值对应的类型
        /// </summary>
        /// <param name="value">单元格值</param>
        /// <returns></returns>
        public static TypeEnum GetValueByType(object value)
        {
            Type type = value.GetType();
            if (type == typeof(int))
                return TypeEnum.Int;
            else if (type == typeof(string))
                return TypeEnum.String;
            else if (type == typeof(float))
                return TypeEnum.Float;
            else if (type == typeof(decimal))
                return TypeEnum.Decimal;
            else if (type == typeof(bool))
                return TypeEnum.Bool;
            else if (type == typeof(DateTime))
                return TypeEnum.DateTime;
            else if (type == typeof(MyFormula))
                return TypeEnum.Formula;
            else
                return TypeEnum.String;
        }
        #endregion

    }


    /// <summary>
    /// 公式类（单元格值是公式时，传入此类）
    /// </summary>
    public class MyFormula
    {
        public MyFormula(string  formula)
        {
            Formula = formula;
        }
        public string Formula { get; }
    }

    /// <summary>
    /// 类型枚举
    /// </summary>
    public enum TypeEnum
    {
        Int,
        String,
        Float,
        Decimal,
        Bool,
        DateTime,
        Formula
    }
}
