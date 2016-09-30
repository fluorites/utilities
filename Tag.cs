using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zakharov.Utility;

//  Создание пространства имен Антона Захарова.
namespace Zakharov {
    //  Создание пространства имен тегов.
    namespace Tag {

        /// <summary>Определяет свойства и методы, используемые для обработки списка тегов объектов.</summary>
        /// <remarks>Теги представляются в виде строк.</remarks>
        public interface ITaggable {
            /// <summary>Список тегов, разделенный определенным символом.</summary>
            /// <returns>Строка с тегами.</returns>
            /// <remarks>Строка со списком тегов используется для чтения/записи тегов из источника данных.</remarks>
            string Tags { get; set; }

            /// <summary>Перебор списка тегов объекта.</summary>
            /// <returns>Перечислитель, который поддерживает простой перебор тегов объекта.</returns>
            IEnumerable<string> GetTags();
            
            /// <summary>Добавление тега в список.</summary>
            /// <param name="x_stTag">тег</param>
            void AttachTag(string x_sTag);
            /// <summary>Удаление тега из списка.</summary>
            /// <param name="x_stTag">тег</param>
            void DetachTag(string x_sTag);
        }

        /// <summary>Список тегов объектов.</summary>
        /// <remarks>Теги представляются в виде строк.</remarks>
        public class CTags : ITaggable {
            /// <summary>Символ, используемый для разделения тегов в строке.</summary>
            public const char n_cSettings_TagSeparator=';';

            /// <summary>Перечень тегов.</summary>
            /// <remarks>Список тегов содержит только уникальные значения.</remarks>
            private HashSet<string> m_hsTags=new HashSet<string>();

            /// <summary>Список тегов, разделенный определенным символом.</summary>
            /// <returns>Строка с тегами.</returns>
            /// <remarks>Свойство со списком тегов не поддерживается. Для получения списка тегов следует использовать метод GetTags.</remarks>
            public string Tags {
                get {
                    throw new NotImplementedException();
                }
                set {
                    throw new NotImplementedException();
                }
            }

            /// <summary>Создание объекта.</summary>
            public CTags() {
            }

            /// <summary>Возвращает строковое представление текущего объекта.</summary>
            /// <returns>Строка, представляющая текущий объект.</returns>
            public override string ToString() {
                return CString.GetConcat(m_hsTags,n_cSettings_TagSeparator);                
            }
            /// <summary>Играет роль хэш-функции для определённого типа.</summary>
            /// <returns>Хэш-код для текущего объекта.</returns>
            public override int GetHashCode() {
                return ToString().GetHashCode();
            }
            /// <summary>Добавление тега в список.</summary>
            /// <param name="x_stTag">тег</param>
            /// <remarks>Перед добавлением тег переводится в нижний регистр с удалением начальных и конечных знаков пробела.</remarks>
            public void AttachTag(string x_sTag) {
                if (String.IsNullOrEmpty(x_sTag))
                    throw new ArgumentException(
                        CResource.LoadString("IDS_ERR_EMPTYTAGAVLUE"),
                        "x_sTag"); 
                m_hsTags.Add(x_sTag.ToLower().Trim());
            }
            /// <summary>Удаление тега из списка.</summary>
            /// <param name="x_stTag">тег</param>
            /// <remarks>Перед удалением тег переводится в нижний регистр с удалением начальных и конечных знаков пробела.</remarks>
            public void DetachTag(string x_sTag) {
                if (String.IsNullOrEmpty(x_sTag))
                    throw new ArgumentException(
                        CResource.LoadString("IDS_ERR_EMPTYTAGAVLUE"),
                        "x_sTag");
                m_hsTags.Remove(x_sTag.ToLower().Trim());
            }
            /// <summary>Перебор списка тегов объекта.</summary>
            /// <returns>Перечислитель, который поддерживает простой перебор тегов объекта.</returns>
            public IEnumerable<string> GetTags() {
                foreach(string c_sTag in m_hsTags)
                    yield return c_sTag;
            }
        }
    }
}