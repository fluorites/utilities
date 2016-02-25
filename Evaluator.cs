using System;
using System.CodeDom.Compiler;
using System.Text;

//  Создание пространства имен Антона Ю. Захарова
namespace Zakharov {
    //  Создание пространства имен выражений.
    namespace Expression {

        public class CEvaluator {
            /// <summary>Шаблон для содержимого сборки.</summary>
            private const string n_sTemplate_AssemblyBegin = "using System; public static class Evaluator { public static object Evaluate() { return ";
            private const string n_sTemplate_AssemblyEnd="; } }";

            private static CEvaluator m_ojInstance=null;

            private CodeDomProvider m_cpCompiler = null;
            private CompilerParameters m_cpCompilerOptions = null;

            public static CEvaluator Instance {
                get {
                    return m_ojInstance ?? (m_ojInstance = new CEvaluator());
                }
            }

            /// <summary>Создание объекта.</summary>
            /// <remarks>Конструктор класса объявлен защищенным, чтобы предотвратить создание экземпляра класса.</remarks>
            protected CEvaluator() {
                m_cpCompilerOptions = new CompilerParameters() {
                    GenerateInMemory=true              
                };
                m_cpCompilerOptions.ReferencedAssemblies.Add("mscorlib.dll");
                m_cpCompiler = CodeDomProvider.CreateProvider("c#");
            }


            public object Evaluate(string x_sExpression) {
                CompilerResults c_crResult=m_cpCompiler.CompileAssemblyFromSource(
                    m_cpCompilerOptions, 
                    String.Concat(n_sTemplate_AssemblyBegin,x_sExpression,n_sTemplate_AssemblyEnd));
                if (c_crResult.Errors.HasErrors) {
                    StringBuilder c_sbErrors=new StringBuilder();
                    foreach (CompilerError c_eError in c_crResult.Errors)
                        c_sbErrors.AppendLine(string.Format("{0} in {1}", c_eError.ErrorText, c_eError.Column));
                    throw new ArgumentException(c_sbErrors.ToString(), "x_sExpression");
                }

                return (object)c_crResult.CompiledAssembly.GetType("Evaluator").GetMethod("Evaluate").Invoke(null, null);
            }
        }
    }
}