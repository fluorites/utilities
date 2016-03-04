using System;
using System.CodeDom.Compiler;
using System.Text;

//  Создание пространства имен Антона Ю. Захарова
namespace Zakharov {
    //  Создание пространства имен выражений.
    namespace Term {

        public class CTerm {
            /// <summary>Шаблон для содержимого сборки.</summary>
            private const string n_sTemplate_AssemblyContent="using System; public static class Term {{ public static object Evaluate() {{ return {0}; }} }}";

            /// <summary>Единственный экзмепляр класса</summary>
            private static CTerm m_ojInstance=null;

            /// <summary>Единственный экзмепляр класса</summary>
            public static CTerm Instance {
                get {
                    return m_ojInstance ?? (m_ojInstance = new CTerm());
                }
            }

            /// <summary>Создание объекта.</summary>
            /// <remarks>Конструктор класса объявлен защищенным, чтобы предотвратить создание экземпляра класса.</remarks>
            protected CTerm() {
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="x_sTerm"></param>
            /// <returns></returns>
            public T EvaluateTerm<T>(string x_sTerm) {                                
                // Компиляция сборки, содержащей класс с единственным методом, возвращающим результат вычисления выражения.
                CompilerResults c_crAssembly=CodeDomProvider.CreateProvider("c#").CompileAssemblyFromSource(
                    new CompilerParameters(new string[]{"mscorlib.dll"}) {
                        GenerateInMemory=true
                    }, 
                    String.Format(n_sTemplate_AssemblyContent,x_sTerm));                    
                if (c_crAssembly.Errors.HasErrors) {
                    StringBuilder c_sbErrors=new StringBuilder();
                    foreach(CompilerError c_eError in c_crAssembly.Errors)
                        c_sbErrors.AppendLine(c_eError.ErrorText);
                    throw new ArgumentException(c_sbErrors.ToString(), "x_sExpression");                    
                }

                // Выполнение метода, возвращающего результат вычисления выражения.
                return (T)c_crAssembly.CompiledAssembly.GetType("Term").GetMethod("Evaluate").Invoke(null, null);
            }
        }
    }
}