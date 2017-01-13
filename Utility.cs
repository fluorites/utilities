using System;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;

//  Создание пространства имен Онуфрия Токарева.
namespace Onuphrius {
    //  Создание пространства имен дополнительных функций.
    namespace Utility {

        /// <summary>Класс дополнительных функций форматирования.</summary>
        public static class CFormat {
            /// <summary>Формирование строки с названием месяца.</summary>
            /// <param name="x_lMonth">индекс месяца. Замечание: месяцы нумеруются начиная с 1.</param>
            /// <returns>строка с названием месяца.</returns>
            public static string GetMonth(long x_lMonth) {
                string[] c_sMonths=new string[] {
                    CResource.LoadString("IDS_VAL_JANUARY"),
                    CResource.LoadString("IDS_VAL_FEBRUARY"),
                    CResource.LoadString("IDS_VAL_MARCH"),
                    CResource.LoadString("IDS_VAL_APRIL"),
                    CResource.LoadString("IDS_VAL_MAY"),
                    CResource.LoadString("IDS_VAL_JUNE"),
                    CResource.LoadString("IDS_VAL_JULY"),
                    CResource.LoadString("IDS_VAL_AUGUST"),
                    CResource.LoadString("IDS_VAL_SEPTEMBER"),
                    CResource.LoadString("IDS_VAL_OCTOBER"),
                    CResource.LoadString("IDS_VAL_NOVEMBER"),
                    CResource.LoadString("IDS_VAL_DECEMBER")
                };
                if ((x_lMonth>=1)&&(x_lMonth<=12))
                    return c_sMonths[x_lMonth-1];
                else
                    throw new ArgumentException(
                        CResource.LoadString(String.Format("IDS_ERR_MONTHOUTOFRANGE",x_lMonth)),
                        "x_lMonth");
            }
            /// <summary>Формирование строки с перечислением переменного количества аргументов.</summary>
            /// <param name="x_ojArguments">массив переменного количества аргументов.</param>>
            /// <returns>строка с перечислением переменного количества аргументов.</returns>>
            public static string GetParams(params object[] x_ojArguments) {
                string c_sArguments="";

                for (int i=0; i<x_ojArguments.Length; i++) {
                    c_sArguments+=","+GetNull(x_ojArguments[i]);
                }

                return c_sArguments;
            }
            /// <summary>Формирование строки с информацией о строке в программном коде.</summary>
            /// <param name="x_sfCaller">информация о стеке вызова.</param>
            /// <returns>строка с информацией о строке в программном коде.</returns>
            public static string GetCaller(StackFrame x_sfCaller) {
                if (x_sfCaller.GetFileName()!=null)
                    return String.Format("{0}|{1} at {2} on line {3}",
                        x_sfCaller.GetMethod().DeclaringType.Name,
                        x_sfCaller.GetMethod().Name,
                        x_sfCaller.GetFileName().Substring(x_sfCaller.GetFileName().LastIndexOf("\\")+1),
                        x_sfCaller.GetFileLineNumber());
                else
                    return String.Format("{0}|{1} at unknown file on line {2}",
                        x_sfCaller.GetMethod().DeclaringType.Name,
                        x_sfCaller.GetMethod().Name,
                        x_sfCaller.GetFileLineNumber());
            }
            /// <summary>Преобразование объекта в строку с проверкой на null.</summary>
            /// <param name="x_ojValue">произвольный объект.</param>
            /// <returns>строковое представление объекта.</returns>
            public static string GetNull(object x_ojValue) {
                if (x_ojValue==null)
                    return "null";
                else
                    return x_ojValue.ToString();
            }
            /// <summary>Формирование строки с перечислением элементов массива.</summary>
            /// <param name="x_ojArguments">массив элементов.</param>
            /// <returns>строка с перечислением элементов массива.</returns>
            public static string GetArray(object[] x_ojArguments) {
                StringBuilder c_sbArguments=new StringBuilder();

                if (x_ojArguments!=null)
                    for (int i=0; i<x_ojArguments.Length; i++) {
                        c_sbArguments.Append(","+x_ojArguments[i].ToString());
                    }

                return c_sbArguments.ToString();
            }
            /// <summary>Формирование строки с описаниями всех внутренних исключений данного исключения.</summary>
            /// <param name="x_eError">исключение.</param>>
            /// <param name="x_bInner">если true, то возвращается описание только самого внутреннего исключения.</param>
            /// <returns>строка с описаниями всех внутренних исключений данного исключения.</returns>
            public static string GetException(Exception x_eError, bool x_bInner=false) {
                string c_sMessage=x_eError.Message;

                Exception c_eError=x_eError.InnerException;
                while (c_eError!=null) {
                    if (x_bInner)
                        c_sMessage=c_eError.Message;
                    else
                        c_sMessage+=" "+c_eError.Message;
                    c_eError=c_eError.InnerException;
                }

                return c_sMessage;
            }
        }

        /// <summary>Класс дополнительных функций работы со строками.</summary>
        public static class CString {
            /// <summary>Выделение первых n-символов в строке.</summary>
            /// <param name="x_sSource">строка, из которой необходимо выделить первых n-символов.</param>
            /// <param name="x_iCount">количество символов.</param>
            /// <returns>строка из n первых символов передаваемой строки или передаваемая строка, если количество символов в ней меньше n.</returns>
            public static string Choose(this string x_sSource, int x_iCount) {
                if(x_sSource.Length < x_iCount)
                    return x_sSource;
                else
                    return x_sSource.Substring(0, x_iCount);
            }
            /// <summary>Формирование строки с хэш-кодом передаваемой строки.</summary>
            /// <param name="x_sSource">строка, для которой необходимо рассчитать хэш-код.</param>
            /// <returns>строка с хэш-кодом передаваемой строки.</returns>
            public static string GetHash(this string x_sSource) {
                string c_sHashValue = "";

                if(!String.IsNullOrEmpty(x_sSource)) {
                    // Получение хэш-кода переданной строки в виде массива целых чисел.
                    byte[] c_bHashValue = (new SHA1Managed()).ComputeHash((new UnicodeEncoding()).GetBytes(x_sSource));
                    // Преобразование хэш-кода в строку.
                    foreach(byte c_bHashByte in c_bHashValue)
                        c_sHashValue += c_bHashByte.ToString();
                }

                return c_sHashValue;
            }
            /// <summary>Сцепление строк со вставкой между ними разделителя.</summary>
            /// <param name="x_sValues">список сцепляемых строк.</param>
            /// <param name="x_cSeparator">символ разделителя.</param>
            /// <returns>строка из сцепленных строк со вставленными между ними разделителями.</returns>
            public static string GetConcat(IEnumerable<string> x_sValues, char x_cSeparator) {
                StringBuilder c_sbValues = new StringBuilder();

                foreach(string c_sValue in x_sValues)
                    c_sbValues.Append(c_sValue + x_cSeparator);

                return c_sbValues.ToString();
            }
        }

        /// <summary>Класс дополнительных функций работы с ресурсами сборки.</summary>
        public class CResource {
            /// <summary>Чтение строки из файла ресурсов вызывающей сборки.</summary>
            /// <param name="x_sName">имя ресурса.</param>
            /// <returns>строка со значением требуемой ресурсной строки.</returns>
            /// <remarks>Для поиска строки выбирается ресурсный файл с именем Strings.resx.</remarks>
            public static string LoadString(string x_sName) {
                ResourceManager c_rmManager=new ResourceManager(
                    Assembly.GetCallingAssembly().GetName().Name+".Strings",
                    Assembly.GetCallingAssembly());
                
                return c_rmManager.GetString(x_sName, Thread.CurrentThread.CurrentUICulture);
            }
        }

        /// <summary>Класс дополнительных функций работы с реестром Windows.</summary>
        public static class CRegistry {
            /// <summary>Проверка существования заданного значения в реестре и, если оно не существует, создание пустого значения.</summary>
            /// <param name="x_rgKey">раздел реестра.</param>
            /// <param name="x_sName">имя значения.</param>
            /// <returns>true, если заданное значение существует.</returns>
            public static bool Exists(this RegistryKey x_rgKey, string x_sName) {
                // Чтение значения в реестре.
                if (x_rgKey.GetValue(x_sName)==null) {
                    // Создание пустого значения в реестре.
                    x_rgKey.SetValue(x_sName, "");
                    return false;
                }
                else
                    return true;
            }
        }

        /// <summary>Тип элемента документа XML.</summary>
        public enum ElementTypeEnum {
            /// <summary>Информация элемента документа XML представлена в виде набора дочерних узлов.</summary>
            NODE_ELEMENT=0x00000000,
            /// <summary>Информация элемента документа XML представлена в виде набора атрибутов.</summary>
            ATTRIBUTE_ELEMENT=0x00000001
        }

        /// <summary>Класс дополнительных функций работы с документами XML.</summary>
        public class CXml {
            /// <summary>Создание нового элемента с указанной информацией в атрибутах в указанном узле документа XML.</summary>
            /// <param name="x_xnElementParent">узел документа XML, в который добавляется новый элемент.</param>
            /// <param name="x_sElementName">имя нового элемента.</param>
            /// <param name="x_lElementType">тип нового элемента.</param>
            /// <param name="x_sElementData">данные нового элемента.</param>
            public static void AddElement(
                XmlNode x_xnElementParent,
                string x_sElementName,
                ElementTypeEnum x_lElementType,
                params string[] x_sElementData) {

                // Создание нового элемента.
                XmlNode c_xnElement=x_xnElementParent.OwnerDocument.CreateNode("element", x_sElementName, "");

                switch (x_lElementType) {
                    case ElementTypeEnum.ATTRIBUTE_ELEMENT:
                        for (int i=0; i<x_sElementData.Length; i+=2) {
                            // Добавление атрибута к новому элементу и присваивание ему значения.
                            c_xnElement.Attributes.Append(x_xnElementParent.OwnerDocument.CreateAttribute(x_sElementData[i]));
                            c_xnElement.Attributes[x_sElementData[i]].InnerText=x_sElementData[i+1];
                        }
                        break;
                    case ElementTypeEnum.NODE_ELEMENT:
                        for (int i=0; i<x_sElementData.Length; i+=2) {
                            // Добавление дочернего узла к новому элементу и присваивание ему значения.
                            c_xnElement.AppendChild(x_xnElementParent.OwnerDocument.CreateNode(XmlNodeType.Element, x_sElementData[i], ""));
                            c_xnElement.LastChild.InnerText=x_sElementData[i+1];
                        }
                        break;
                    default:
                        throw new Exception(
                            String.Format(
                            CResource.LoadString("IDS_ERR_INVALIDELEMENTTYPE"),
                            Enum.Format(typeof(ElementTypeEnum), x_lElementType, "G")));
                }
                // Добавление созданного элемента к родительскому элементу документа XML.
                x_xnElementParent.AppendChild(c_xnElement);
            }
            /// <summary>Формирование строки с информацией об узле с исключением подчиненных элементов.</summary>
            /// <param name="x_xnElementNode">узел документа XML.</param>
            /// <returns>строка с информацией об узле.</returns>
            public static string GetElement(XmlNode x_xnElementNode) {
                string c_sElementNode="<"+x_xnElementNode.Name;

                // Перебор всех атрибутов узла.
                for (int i=0; i<x_xnElementNode.Attributes.Count; i++)
                    c_sElementNode+=" "+x_xnElementNode.Attributes[i].Name+"='"+x_xnElementNode.Attributes[i].Value+"'";
                c_sElementNode+=" />";

                return c_sElementNode;
            }
            /// <summary>Формирование узла из строки с информацией об узле.</summary>
            /// <param name="x_sElementNode">строка с информацией об узле.</param>
            /// <returns>узел документа XML.</returns>
            public static XmlNode GetElement(string x_sElementNode) {
                // Получение документа XML.
                XmlDocument c_xdElementDocument=new XmlDocument();
                c_xdElementDocument.LoadXml(x_sElementNode);
                // Получение узла документа XML.
                return c_xdElementDocument.FirstChild;
            }
        }

        /// <summary>Класс дополнительных функций работы с каталогами и подкаталогами.</summary>
        public static class CDirectory {
            /// <summary>
            /// Получение всех подкатологов (включая вложенные) текущего каталога
            /// </summary>
            /// <param name="x_diDirectory">текущий каталог, для которого необходимо получить все подкаталоги</param>
            /// <returns>массив всех подкатологов (включая вложенные) текущего каталога</returns>
            public static DirectoryInfo[] GetSubdirectories(this DirectoryInfo x_diDirectory) {
                List<DirectoryInfo> c_lsSubdirectories = new List<DirectoryInfo>();

                c_lsSubdirectories.AddRange(x_diDirectory.GetDirectories());
                foreach(DirectoryInfo c_diDirectory in x_diDirectory.GetDirectories())
                    c_lsSubdirectories.AddRange(c_diDirectory.GetSubdirectories());

                return c_lsSubdirectories.ToArray();
            }
        }
    }    
}