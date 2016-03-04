using System;
using System.CodeDom.Compiler;
using System.Text;

//  Создание пространства имен Антона Ю. Захарова
namespace Zakharov {
    //  Создание пространства имен выражений.
    namespace Term {

        /// <summary>Обработчик вычисления выражений.</summary>
        public class CEvaluator {
            /// <summary>Шаблон для начала содержимого сборки.</summary>
            private const string n_sTemplate_AssemblyBegin = "using System; public static class Evaluator { public static object Evaluate() { return ";
            /// <summary>Шаблон для конца содержимого сборки.</summary>
            private const string n_sTemplate_AssemblyEnd="; } }";

            #region Реализация шаблона одиночка (singleton)
            /// <summary>Единственный экзмепляр класса</summary>
            private static CEvaluator m_ojInstance=null;

            /// <summary>Единственный экзмепляр класса</summary>
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
                    String.Concat(n_sTemplate_AssemblyBegin,x_sTerm,n_sTemplate_AssemblyEnd));                    
                if (c_crAssembly.Errors.HasErrors) {
                    StringBuilder c_sbErrors=new StringBuilder();
                    foreach(CompilerError c_eError in c_crAssembly.Errors)
                        c_sbErrors.AppendLine(c_eError.ErrorText);
                    throw new ArgumentException(c_sbErrors.ToString(), "x_sExpression");                    
                }

                // Выполнение метода, возвращающего результат вычисления выражения.
                return (T)c_crAssembly.CompiledAssembly.GetType("Evaluator").GetMethod("Evaluate").Invoke(null, null);
            }
        }
    }
}