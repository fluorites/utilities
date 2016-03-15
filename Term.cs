using System;
using System.CodeDom.Compiler;
using System.Text;
using Zakharov.Handbook;

//  Создание пространства имен Антона Захарова.
namespace Zakharov {
    //  Создание пространства имен выражений.
    namespace Term {

        /// <summary>Обработчик вычисления выражений, записанных в виде строки.</summary>
        public class CEvaluator {
            /// <summary>Шаблон для содержимого сборки.</summary>
            /// <remarks>Поскольку шаблон строки будет заполняться с помощью метода String.Format фигурные скобки в коде дублируются.</remarks>
            private const string n_sTemplate_AssemblyContent = "using System; public static class {0} {{ public static object {1}() {{ return {2}; }} }}";
            /// <summary>Имя класса в сборке, используемое по умолчанию.</summary>
            private const string n_sDefault_ClassName="CEvaluator";
            /// <summary>Имя метода класса в сборке, используемое по умолчанию.</summary>
            private const string n_sDefault_MethodName="EvaluateTerm";

            #region Реализация шаблона одиночка (singleton)
            /// <summary>Единственный экзмепляр класса</summary>
            private static CEvaluator m_ojInstance=null;

            /// <summary>Единственный экземпляр класса.</summary>
            public static CEvaluator Instance {
                get {
                    return m_ojInstance ?? (m_ojInstance = new CEvaluator());
                }
            }
            #endregion

            /// <summary>Создание объекта.</summary>
            /// <remarks>Конструктор класса объявлен защищенным, чтобы предотвратить создание экземпляра класса.</remarks>
            protected CEvaluator() {
            }
            /// <summary>Вычисление выражения.</summary>
            /// <typeparam name="T">тип результата вычисления выражения</typeparam>
            /// <param name="x_sTerm">выражение</param>
            /// <returns>результат вычисления выражения</returns>
            public T EvaluateTerm<T>(string x_sTerm) {                                
                // Компиляция сборки, содержащей класс с единственным методом, возвращающим результат вычисления выражения.
                CompilerResults c_crAssembly=CodeDomProvider.CreateProvider("c#").CompileAssemblyFromSource(
                    new CompilerParameters(new string[]{"mscorlib.dll"}) {
                        GenerateInMemory=true
                    },
                    String.Format(
                        n_sTemplate_AssemblyContent, 
                        n_sDefault_ClassName, 
                        n_sDefault_MethodName, 
                        x_sTerm));
                // Проверка ошибок при компиляции сборки.
                if (c_crAssembly.Errors.HasErrors) {
                    #region Формирование сообщения об ошибке компиляции
                    StringBuilder c_sbErrors=new StringBuilder();
                    foreach(CompilerError c_eError in c_crAssembly.Errors)
                        c_sbErrors.AppendLine(c_eError.ErrorText);
                    throw new ArgumentException(
                        String.Format(
                            CResource.LoadString("IDS_ERR_COMPILERERROR"),
                            x_sTerm,
                            c_sbErrors.ToString()), 
                        "x_sExpression");
                    #endregion
                }

                // Выполнение метода, возвращающего результат вычисления выражения.
                return (T)c_crAssembly.CompiledAssembly.GetType(n_sDefault_ClassName).GetMethod(n_sDefault_MethodName).Invoke(null, null);
            }
        }
    }
}