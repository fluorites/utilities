using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using System.Data;
using System.Data.SqlServerCe;
using System.Runtime.Serialization;
using System.Xml;

using Zakharov.Utility;
using System.Configuration;
using System.Collections.Generic;

//  Создание пространства имен Антона Захарова.
namespace Zakharov {
    //  Создание пространства имен журнала сообщений.
    namespace Journal {

        /// <summary>Тип сообщения.</summary>
        public enum MessageTypeEnum {
            /// <summary>Сообщение об ошибке.</summary>
            ERROR_MESSAGE=0x00000000,
            /// <summary>Сообщение с предупреждением.</summary>
            WARNING_MESSAGE=0x00000001,
            /// <summary>Сообщение с информацией.</summary>
            INFORMATION_MESSAGE=0x00000002
        }

        /// <summary>Вид сообщения.</summary>
        public enum MessageFormEnum {
            /// <summary>Сообщение для отладки.</summary>
            DEBUG_MESSAGE=0x00000000,
            /// <summary>Сообщение для работы.</summary>
            RELEASE_MESSAGE=0x00000001
        }

        /// <summary>Тип важности сообщения.</summary>
        public enum MessageSeverityEnum {
            /// <summary>Несущественное сообщение.</summary>
            LOW_PRIORITY=0x00000000,
            /// <summary>Обычное сообщение.</summary>
            NORMAL_PRIORITY=0x00000001,
            /// <summary>Важное сообщение.</summary>
            HIGH_PRIORITY=0x00000002,
            /// <summary>Критическое сообщение.</summary>
            CRITICAL_PRIORITY=0x00000003
        }

        /// <summary>Режим работы журнала сообщений.</summary>
        public enum JournalModeEnum {
            /// <summary>Режим записи отладочных сообщений включен.</summary>
            DEBUG_ON=0x00000000,
            /// <summary>Режим записи отладочных сообщений выключен.</summary>
            DEBUG_OFF=0x00000001
        }

        /// <summary>Абстрактный журнал сообщений.</summary>
        public abstract class CJournal {
                
            /// <summary>Значение конфигурации, в котором сохраняется допустимый уровень важности сообщения.</summary>
            private const string n_sSettings_SeverityLevel="Severity level";
            /// <summary>Значение конфигурации, в котором сохраняется режим работы журнала сообщений.</summary>
            private const string n_sSettings_JournalMode="Journal mode";
                

            /// <summary>Допустимый уровень важности сообщения.</summary>
            private MessageSeverityEnum m_lSeverityLevel;
            /// <summary>Режим работы журнала сообщений.</summary>
            private JournalModeEnum m_lJournalMode;
            /// <summary>Имя взаимного исключения, блокирующего одновременный доступ к журналу сообщений.</summary>
            protected string m_sMutexName;

            /// <summary>Запись сообщения.</summary>
            /// <param name="x_sSource">имя источника сообщения.</param>
            /// <param name="x_ulType">тип сообщения.</param>
            /// <param name="x_ulForm">вид сообщения.</param>
            /// <param name="x_sCategory">категория сообщения.</param>
            /// <param name="x_sDescription">текст сообщения.</param>
            /// <returns>True, если при  записи сообщения не возникли ошибки.</returns>
            protected abstract bool WritingMessage(
                string x_sSource,
                MessageTypeEnum x_ulType,
                MessageFormEnum x_ulForm,
                string x_sCategory,
                string x_sDescription);
            /// <summary>Создание объекта.</summary>
            public CJournal() {
                // Чтение значений параметров журнала, сохраненных в файле конфигурации. 
                try {
                    // Чтение допустимого уровеня важности сообщения.
                    m_lSeverityLevel=(MessageSeverityEnum)Enum.Parse(
                        typeof(MessageSeverityEnum),
                        ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_SeverityLevel]);
                }
                catch {
                    // Использование значения по умолчанию.
                    m_lSeverityLevel=MessageSeverityEnum.LOW_PRIORITY;
                }
                try {
                    // Чтение режима работы журнала сообщений.
                    m_lJournalMode=(JournalModeEnum)Enum.Parse(
                        typeof(JournalModeEnum),
                        ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_JournalMode]);
                }
                catch {
                    // Использование значения по умолчанию.
                    m_lJournalMode=JournalModeEnum.DEBUG_OFF;
                }
            }
            /// <summary>Запись сообщения.</summary>
            /// <param name="x_lImportanceLevel">уровень важности сообщения.</param>
            /// <param name="x_sSource">имя источника сообщения.</param>
            /// <param name="x_ulType">тип сообщения.</param>
            /// <param name="x_ulForm">вид сообщения.</param>>
            /// <param name="x_sCategory">категория сообщения.</param>
            /// <param name="x_sDescription">текст сообщения.</param>
            /// <returns>True, если при  записи сообщения не возникли ошибки.</returns>
            public bool WriteMessage(
                MessageSeverityEnum x_lImportanceLevel,
                string x_sSource,
                MessageTypeEnum x_ulType,
                MessageFormEnum x_ulForm,
                string x_sCategory,
                string x_sDescription) {

                bool c_bSucceeded=false;
                // Проверка необходимости записи отладочного сообщения.
                if ((x_ulForm==MessageFormEnum.RELEASE_MESSAGE)
                    ||((m_lJournalMode==JournalModeEnum.DEBUG_ON)&&(x_ulForm==MessageFormEnum.DEBUG_MESSAGE))) {
                    // Проверка важности сообщения.
                    if (x_lImportanceLevel>=m_lSeverityLevel) {
                        // Проверка необходимости использования взаимного исключения при записи сообщения.
                        if (String.IsNullOrEmpty(m_sMutexName)) {
                            // Вызов записи сообщения, переопределяемого в классе-потомке.
                            c_bSucceeded=WritingMessage(x_sSource, x_ulType, x_ulForm, x_sCategory, x_sDescription);
                        }
                        else {
                            bool c_bCaptured=false;
                            // Запрос владения журналом сообщений.
                            Mutex c_mxLockJournal=new Mutex(true, m_sMutexName, out c_bCaptured);

                            if (!c_bCaptured)
                                // Ожидание освобождения журнала сообщений.
                                c_mxLockJournal.WaitOne();
                            try {
                                // Вызов записи сообщения, переопределяемого в классе-потомке.
                                c_bSucceeded=WritingMessage(x_sSource, x_ulType, x_ulForm, x_sCategory, x_sDescription);
                            }
                            finally {
                                // Освобождение журнала сообщений.
                                c_mxLockJournal.ReleaseMutex();
                            }
                        }
                    }
                }

                return c_bSucceeded;
            }
        }            

        /// <summary>Статический класс для получения требуемого журнала сообщений.</summary>
        public class CJournalFactory {
            /// <summary>Перечень журналов сообщений различного вида.</summary>
            private static Dictionary<Type, CJournal> m_dcJournals=new Dictionary<Type, CJournal>();

            /// <summary>Создание объекта.</summary>
            private CJournalFactory() {
            }
            /// <summary>Получение доступа к требуемому журналу сообщений.</summary>
            /// <param name="x_tpJournalType">тип журнала.</param>>
            /// <returns>требуемый журнал.</returns>>
            public static CJournal Journal(Type x_tpJournalType) {
                if (!m_dcJournals.ContainsKey(x_tpJournalType)) {
                    // Создание требуемого журнала сообщений, если он не существует.
                    try {
                        m_dcJournals.Add(x_tpJournalType, (CJournal)Activator.CreateInstance(x_tpJournalType));
                    }
                    catch {
                        throw new ArgumentException(
                            String.Format(
                                CResource.LoadString("IDS_ERR_INVALIDJOURNALTYPE"),
                                x_tpJournalType.FullName),
                            "x_tpJournalType"); 
                    }
                }

                return m_dcJournals[x_tpJournalType];                   
            }
        }

        /// <summary>Журнал сообщений в виде файла в формате XML.</summary>
        public class CXmlJournal: CJournal {

            /// <summary>Значение конфигурации, в котором сохраняется имя файла журнала.</summary>
            private const string n_sSettings_FileName="File name";
            /// <summary>Имя файла журнала сообщений, используемого по умолчанию.</summary>
            private const string n_sDefault_FileName="c:\\tmp\\event log.xml";
            /// <summary>XPath-запрос для получения узла, содержащего сообщения.</summary>
            private const string n_sTemplate_JournalNode="//journal";
            /// <summary>Пустой журнал сообщений.</summary>
            private const string n_sTemplate_EmptyJournal="<?xml version=\"1.0\" encoding=\"UTF-8\"?><journal></journal>";
                

            /// <summary>Имя файла журнала сообщений.</summary>
            private string m_sFileName;

            /// <summary>Создание объекта.</summary>
            public CXmlJournal() {
                // Чтение значений параметров журнала, сохраненных в файле конфигурации. 
                try {
                    // Чтение имени файла журнала.
                    m_sFileName=ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_FileName];
                    if (String.IsNullOrWhiteSpace(m_sFileName))
                        throw new Exception();
                }
                catch {
                    // Использование значения по умолчанию.
                    m_sFileName=n_sDefault_FileName;
                }
                // Обеспечение совпадения имени взаимного исключения для данного журнала сообщений.
                m_sMutexName=m_sFileName.GetHash();
            }
            /// <summary>Запись сообщения.</summary>
            /// <param name="x_sSource">имя источника сообщения.</param>>
            /// <param name="x_ulType">тип сообщения.</param>>
            /// <param name="x_ulForm">вид сообщения.</param>>
            /// <param name="x_sCategory">категория сообщения.</param>>
            /// <param name="x_sDescription">текст сообщения.</param>>
            /// <returns>True, если при  записи сообщения не возникли ошибки.</returns>
            protected override bool WritingMessage(
                string x_sSource,
                MessageTypeEnum x_ulType,
                MessageFormEnum x_ulForm,
                string x_sCategory,
                string x_sDescription) {

                bool c_bSucceeded=true;
                // Текущие дата и время.
                DateTime c_dtCurrentTime=DateTime.Now;
                // Преобразование типа сообщения.
                string c_sType="Unknown";
                switch (x_ulType) {
                    case MessageTypeEnum.ERROR_MESSAGE:
                        c_sType="Error";
                        break;
                    case MessageTypeEnum.WARNING_MESSAGE:
                        c_sType="Warning";
                        break;
                    case MessageTypeEnum.INFORMATION_MESSAGE:
                        c_sType="Information";
                        break;
                }
                XmlDocument c_xdXmlJournal=new XmlDocument();
                try {
                    // Создание файла, если он не существует.
                    if (!File.Exists(m_sFileName)) {                         
                        c_xdXmlJournal.LoadXml(n_sTemplate_EmptyJournal);
                        c_xdXmlJournal.Save(m_sFileName);
                    }
                    // Открытие файла.
                    c_xdXmlJournal.Load(m_sFileName);
                    // Добавление нового сообщения в XML документ.
                    if (x_ulForm==MessageFormEnum.RELEASE_MESSAGE)
                        CXml.AddElement(
                            c_xdXmlJournal.SelectSingleNode(n_sTemplate_JournalNode),
                            "message",
                            ElementTypeEnum.ATTRIBUTE_ELEMENT,
                            "date", String.Format("{0:d2}.{1:d2}.{2:d4}", c_dtCurrentTime.Day, c_dtCurrentTime.Month, c_dtCurrentTime.Year),
                            "time", String.Format("{0:d2}:{1:d2}:{2:d2}.{3:d3}", c_dtCurrentTime.Hour, c_dtCurrentTime.Minute, c_dtCurrentTime.Second, c_dtCurrentTime.Millisecond),
                            "source", x_sSource,
                            "type", c_sType,
                            "category", x_sCategory,
                            "description", x_sDescription,
                            "user", Environment.UserName,
                            "computer", Environment.MachineName);
                    else
                        CXml.AddElement(
                            c_xdXmlJournal.SelectSingleNode(n_sTemplate_JournalNode),
                            "message",
                            ElementTypeEnum.ATTRIBUTE_ELEMENT,
                            "date", String.Format("{0:d2}.{1:d2}.{2:d4}", c_dtCurrentTime.Day, c_dtCurrentTime.Month, c_dtCurrentTime.Year),
                            "time", String.Format("{0:d2}:{1:d2}:{2:d2}.{3:d3}", c_dtCurrentTime.Hour, c_dtCurrentTime.Minute, c_dtCurrentTime.Second, c_dtCurrentTime.Millisecond),
                            "source", x_sSource,
                            "type", c_sType,
                            "category", x_sCategory,
                            "description", x_sDescription,
                            "user", Environment.UserName,
                            "computer", Environment.MachineName,
                            "thread", Thread.CurrentThread.ManagedThreadId.ToString(),
                            "writer", CFormat.GetCaller(new StackFrame(2, true)));
                    // Сохранение файла.    
                    c_xdXmlJournal.Save(m_sFileName);
                }
                catch {
                    c_bSucceeded=false;
                }

                return c_bSucceeded;
            }
        }

        /// <summary>Журнал сообщений в виде простого текстового файла.</summary>
        public class CTextJournal: CJournal {

            /// <summary>Значение конфигурации, в котором сохраняется имя папки с файлом журнала сообщений.</summary>
            private const string n_sSettings_FolderName="Folder name";
            /// <summary>Имя папки с файлом журнала сообщений, используемого по умолчанию.</summary>
            private const string n_sDefault_FolderName="c:\\tmp";
            /// <summary>Шаблон для формирования полного имени папки для хранения журнала сообщений.</summary>
            private const string n_sTemplate_MessageFolder="{0}\\{1:d4}\\{2}-{3}\\{4:d2}\\";
            /// <summary>Шаблон для формирования полного имени файла для хранения журнала сообщений.</summary>
            private const string n_sTemplate_MessageFile="{0}\\{1}.{2}";
            /// <summary>Шаблон для строки сообщения.</summary>
            private const string n_sTemplate_ReleaseLine="{0:d2}:{1:d2}:{2:d2}.{3:d3}\t{4}\t{5}\r\n";
            /// <summary>Шаблон для строки отладочного сообщения.</summary>
            private const string n_sTemplate_DebugLine="{0:d2}:{1:d2}:{2:d2}.{3:d3}\t{4}\t{5}\t{6}\t{7}\r\n";

            /// <summary>Имя папки с файлом журнала сообщений.</summary>
            private string m_sFolderName;

            /// <summary>Создание объекта.</summary>
            public CTextJournal() {
                // Чтение значений параметров журнала, сохраненных в файле конфигурации. 
                try {
                    // Чтение имени папки с файлом журнала сообщений.
                    m_sFolderName=ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_FolderName];
                    if (String.IsNullOrWhiteSpace(m_sFolderName))
                        throw new Exception();
                }
                catch {
                    // Использование значения по умолчанию.
                    m_sFolderName=n_sDefault_FolderName;
                }
                // Обеспечение совпадения имени взаимного исключения для данного журнала сообщений.
                m_sMutexName=m_sFolderName.GetHash();
            }
            /// <summary>Запись сообщения.</summary>
            /// <param name="x_sSource">имя источника сообщения.</param>>
            /// <param name="x_ulType">тип сообщения.</param>>
            /// <param name="x_ulForm">вид сообщения.</param>>
            /// <param name="x_sCategory">категория сообщения.</param>>
            /// <param name="x_sDescription">текст сообщения.</param>>
            /// <returns>True, если при  записи сообщения не возникли ошибки.</returns>>
            protected override bool WritingMessage(
                string x_sSource,
                MessageTypeEnum x_ulType,
                MessageFormEnum x_ulForm,
                string x_sCategory,
                string x_sDescription) {

                bool c_bSucceeded=true;                
                // Текущие дата и время.
                DateTime c_dtCurrentTime=DateTime.Now;
                // Формирование строки с именем папки журнала сообщений.
                string c_sFolderName=String.Format(
                                            n_sTemplate_MessageFolder,
                                            m_sFolderName,
                                            c_dtCurrentTime.Year,
                                            c_dtCurrentTime.Month,
                                            CFormat.GetMonth(c_dtCurrentTime.Month),
                                            c_dtCurrentTime.Day);
                // Формирование строки с расширением файла журнала сообщений.
                string c_sFileExtension="";
                switch (x_ulType) {
                    case MessageTypeEnum.ERROR_MESSAGE:
                        c_sFileExtension="err";
                        break;
                    case MessageTypeEnum.WARNING_MESSAGE:
                        c_sFileExtension="wrn";
                        break;
                    case MessageTypeEnum.INFORMATION_MESSAGE:
                        c_sFileExtension="inf";
                        break;
                }
                // Формирование строки с именем файла журнала сообщений.
                string c_sFileName=String.Format(
                                        n_sTemplate_MessageFile,
                                        c_sFolderName,
                                        x_sSource,
                                        c_sFileExtension);                
                try {
                    // Создание папки, если она не существует.
                    if (!Directory.Exists(c_sFolderName))
                        Directory.CreateDirectory(c_sFolderName);
                    using (StreamWriter c_swTextJournal=new StreamWriter(new FileStream(c_sFileName, FileMode.Append, FileAccess.Write, FileShare.Read))) {
                        // Формирование строки сообщения.
                        string c_sBuffer="";
                        if (x_ulForm==MessageFormEnum.RELEASE_MESSAGE)
                            c_sBuffer=String.Format(
                                n_sTemplate_ReleaseLine,
                                c_dtCurrentTime.Hour,
                                c_dtCurrentTime.Minute,
                                c_dtCurrentTime.Second,
                                c_dtCurrentTime.Millisecond,
                                x_sCategory,
                                x_sDescription);
                        else
                            c_sBuffer=String.Format(
                                n_sTemplate_DebugLine,
                                c_dtCurrentTime.Hour,
                                c_dtCurrentTime.Minute,
                                c_dtCurrentTime.Second,
                                c_dtCurrentTime.Millisecond,
                                x_sCategory,
                                x_sDescription,
                                Thread.CurrentThread.ManagedThreadId,
                                CFormat.GetCaller(new StackFrame(2, true)));
                        // Запись в файл сообщения. 
                        c_swTextJournal.Write(c_sBuffer);
                    }
                }
                catch {
                    c_bSucceeded=false;
                }
                            
                return c_bSucceeded;
            }
        }

        /// <summary>Стандартный журнал событий Windows.</summary>
        /// <remarks>Источник записей в журнале событий  должен быть создан до первого использования.</remarks>
        public class CEventJournal: CJournal {
                
            /// <summary>Значение конфигурации, в котором сохраняется имя журнала событий.</summary>
            private const string n_sSettings_LogName="Log name";

            /// <summary>Имя журнала событий.</summary>
            private string m_sLogName;

            /// <summary>Создание объекта.</summary>
            public CEventJournal() {
                // Чтение значений параметров журнала, сохраненных в файле конфигурации. 
                try {
                    // Чтение имени журнала событий.
                    m_sLogName=ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_LogName];
                    if (String.IsNullOrWhiteSpace(m_sLogName))
                        throw new Exception();
                }
                catch {
                    // Использование значения по умолчанию.
                    m_sLogName=Assembly.GetEntryAssembly().GetName().Name;                       
                }
            }
            /// <summary>Запись сообщения.</summary>
            /// <param name="x_sSource">имя источника сообщения.</param>>
            /// <param name="x_ulType">тип сообщения.</param>>
            /// <param name="x_ulForm">вид сообщения.</param>>
            /// <param name="x_sCategory">категория сообщения.</param>>
            /// <param name="x_sDescription">текст сообщения.</param>>
            /// <returns>True, если при  записи сообщения не возникли ошибки.</returns>>
            protected override bool WritingMessage(
                string x_sSource,
                MessageTypeEnum x_ulType,
                MessageFormEnum x_ulForm,
                string x_sCategory,
                string x_sDescription) {

                bool c_bSucceeded=true;
                // Преобразование типа сообщения.
                EventLogEntryType c_lType=EventLogEntryType.Information;
                switch (x_ulType) {
                    case MessageTypeEnum.ERROR_MESSAGE:
                        c_lType=EventLogEntryType.Error;
                        break;
                    case MessageTypeEnum.WARNING_MESSAGE:
                        c_lType=EventLogEntryType.Warning;
                        break;
                    case MessageTypeEnum.INFORMATION_MESSAGE:
                        c_lType=EventLogEntryType.Information;
                        break;
                }
                try {
                    using (EventLog c_elEventJournal=new EventLog(m_sLogName, ".", x_sSource)) {
                        // Запись в файл сообщения. 
                        c_elEventJournal.WriteEntry(x_sDescription, c_lType);
                    }
                }
                catch {
                    c_bSucceeded=false;
                }

                return c_bSucceeded;
            }
        }

        /// <summary>Журнал сообщений в виде таблицы базы данных.</summary>
        /// <remarks>Файл базы данных и таблица журнала событий должны быть созданы до первого использования.</remarks>
        public class CSqlceJournal: CJournal {
                
            /// <summary>Значение конфигурации, в котором сохраняется имя файла базы данных.</summary>
            private const string n_sSettings_FileName="File name";
            /// <summary>Имя файла базы данных, используемого по умолчанию.</summary>
            private const string n_sDefault_FileName="c:\\tmp\\event log.sdf";
            /// <summary>Значение конфигурации, в котором сохраняется имя таблицы базы данных.</summary>
            private const string n_sSettings_TableName="Table name";
            /// <summary>Имя таблицы базы данных, используемой по умолчанию.</summary>
            private const string n_sDefault_TableName="Event_List";           
            /// <summary>Шаблон для запроса на создание таблицы для сообщений.</summary>
            public const string n_sTemplate_CreateTable="create table {0} ( "+
                "EventID int identity primary key, "+
                "Source nvarchar(128) not null, "+
                "Type nvarchar(11) not null, "+
                "Category nvarchar(128) not null, "+
                "Description nvarchar(1024) not null, "+
                "Date datetime not null, "+
                "[User] nvarchar(128), "+
                "Computer nvarchar(128), "+
                "Thread int, "+
                "Writer nvarchar(128))";
            /// <summary>Шаблон для запроса на добавления сообщения.</summary>
            private const string n_sTemplate_InsertMessage="insert into {0} "+
                "(Source,Type,Category,Description,[Date],[User],Computer) values "+
                "('{1}','{2}','{3}','{4}',getdate(),'{5}','{6}')";
            /// <summary>Шаблон для запроса на добавления отладочного сообщения.</summary>
            private const string n_sTemplate_InsertDebug="insert into {0} "+
                "(Source,Type,Category,Description,[Date],[User],Computer,Thread,Writer) values "+
                "('{1}','{2}','{3}','{4}',getdate(),'{5}','{6}',{7},'{8}')";

            /// <summary>Имя файла базы данных.</summary>
            private string m_sFileName;
            /// <summary>Имя таблицы базы данных.</summary>
            private string m_sTableName;

            /// <summary>Создание объекта.</summary>
            public CSqlceJournal() {
                // Чтение значений параметров журнала, сохраненных в файле конфигурации.
                try {
                    // Чтение имени файла баы данных
                    m_sFileName=ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_FileName];
                    if (String.IsNullOrWhiteSpace(m_sFileName))
                        throw new Exception();
                }
                catch {
                    // Использование значения по умолчанию.
                    m_sFileName=n_sDefault_FileName;
                }
                try {
                    // Чтение имени таблицы баы данных
                    m_sTableName=ConfigurationManager.AppSettings[GetType().FullName+"."+n_sSettings_TableName];
                    if (String.IsNullOrWhiteSpace(m_sTableName))
                        throw new Exception();
                }
                catch {
                    // Использование значения по умолчанию.
                    m_sTableName=n_sDefault_TableName;
                }                   
            }
            /// <summary>Запись сообщения.</summary>
            /// <param name="x_sSource">имя источника сообщения.</param>>
            /// <param name="x_ulType">тип сообщения.</param>>
            /// <param name="x_ulForm">вид сообщения.</param>>
            /// <param name="x_sCategory">категория сообщения.</param>>
            /// <param name="x_sDescription">текст сообщения.</param>>
            /// <returns>True, если при  записи сообщения не возникли ошибки.</returns>>
            protected override bool WritingMessage(
                string x_sSource,
                MessageTypeEnum x_ulType,
                MessageFormEnum x_ulForm,
                string x_sCategory,
                string x_sDescription) {

                bool c_bSucceeded=true;
                // Преобразование типа сообщения.
                string c_sType="Unknown";
                switch (x_ulType) {
                    case MessageTypeEnum.ERROR_MESSAGE:
                        c_sType="Error";
                        break;
                    case MessageTypeEnum.WARNING_MESSAGE:
                        c_sType="Warning";
                        break;
                    case MessageTypeEnum.INFORMATION_MESSAGE:
                        c_sType="Information";
                        break;
                }
                try {
                    // Создание файла базы данных, если он не существует.
                    if (!File.Exists(m_sFileName)) {
                        (new SqlCeEngine(String.Format("Data Source='{0}'", m_sFileName))).CreateDatabase();
                        // Создание таблицы для сообщений.
                        using (SqlCeConnection c_cnSqlceJournal=new SqlCeConnection(String.Format("Data Source='{0}'", m_sFileName))) {
                            c_cnSqlceJournal.Open();
                            // Формирование и выполнение запроса на создание таблицы.
                            SqlCeCommand c_cmCreate=new SqlCeCommand(
                                String.Format(n_sTemplate_CreateTable, m_sTableName),
                                c_cnSqlceJournal);
                            c_cmCreate.ExecuteNonQuery();
                        }
                    }
                    using(SqlCeConnection c_cnSqlceJournal=new SqlCeConnection(String.Format("Data Source='{0}'", m_sFileName))){
                        c_cnSqlceJournal.Open();
                        // Формирование строки запроса.
                        string c_sBuffer="";
                        if (x_ulForm==MessageFormEnum.RELEASE_MESSAGE)
                            c_sBuffer=String.Format(n_sTemplate_InsertMessage,
                                m_sTableName,
                                x_sSource,
                                c_sType,
                                x_sCategory,
                                x_sDescription.Replace("'", "''"),
                                Environment.UserName,
                                Environment.MachineName);
                        else
                            c_sBuffer=String.Format(n_sTemplate_InsertDebug,
                                m_sTableName,
                                x_sSource,
                                c_sType,
                                x_sCategory,
                                x_sDescription.Replace("'", "''"),
                                Environment.UserName,
                                Environment.MachineName,
                                Thread.CurrentThread.ManagedThreadId,
                                CFormat.GetCaller(new StackFrame(2, true)));
                        // Формирование и выполнение запроса на добавление сообщения.
                        SqlCeCommand c_cmInsert=new SqlCeCommand(c_sBuffer, c_cnSqlceJournal);
                        if (c_cmInsert.ExecuteNonQuery()==0)
                            c_bSucceeded=false;
                    }
                }
                catch {
                    c_bSucceeded=false;
                }                    

                return c_bSucceeded;
            }
        }
    }
}