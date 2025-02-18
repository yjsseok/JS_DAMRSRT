using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Configuration;
using System.Globalization;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Npgsql;

using log4net;
using log4net.Config;
using System.Xml;

namespace JS_DAMRSRT
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
        }
        private void MainFrm_Load(object sender, EventArgs e)
        {
            InitializeLogNBuild();

            if (InitializeDatabase() == true)
            {

            }
            else
            {

            }
        }

        private void Btn_Check_code(object sender, EventArgs e)
        {
            Check_code();
        }      
        private void Check_code()
        {
            string dbIP = Config.dbIP;
            string dbName = Config.dbName;
            string dbPort = Config.dbPort;
            string dbId = Config.dbId;
            string dbPassword = Config.dbPassword;

            string strConn = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                    dbIP, dbPort, dbId, dbPassword, dbName);

            using (var conn = new NpgsqlConnection(strConn))
            {
                conn.Open();

                string selectQuery = "SELECT sort, obs FROM drought_code WHERE obs_cd IS NULL OR obs_cd = ''";
                using (var selectCmd = new NpgsqlCommand(selectQuery, conn))
                {
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string sort = reader["sort"].ToString();
                            string obs = reader["obs"].ToString();

                            string updateQuery = string.Empty;
                            string checkQuery = string.Empty;

                            switch (sort)
                            {
                                case "Dam":
                                    checkQuery = "SELECT damcd FROM tb_wamis_mndammain WHERE damnm = @obs";
                                    break;
                                case "AR":
                                    checkQuery = "SELECT obscd FROM tb_wkw_flw_obs WHERE obsnm = @obs";
                                    break;
                                case "FR":
                                    MessageBox.Show("저수지 코드를 입력해야합니다", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    WriteStatus($"obs: {obs}의 obs_cd 업데이트에 실패했습니다. 저수지 코드를 입력해야합니다.");
                                    continue;
                                default:
                                    MessageBox.Show("유효하지 않은 sort입니다", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    WriteStatus($"obs: {obs}의 obs_cd 업데이트에 실패했습니다. 유효하지 않은 sort입니다.");
                                    continue;
                            }

                            if (!string.IsNullOrEmpty(checkQuery))
                            {
                                using (var checkConn = new NpgsqlConnection(strConn))
                                {
                                    checkConn.Open();
                                    using (var checkCmd = new NpgsqlCommand(checkQuery, checkConn))
                                    {
                                        checkCmd.Parameters.AddWithValue("@obs", obs);
                                        using (var checkReader = checkCmd.ExecuteReader())
                                        {
                                            List<string> codes = new List<string>();
                                            while (checkReader.Read())
                                            {
                                                codes.Add(checkReader[0].ToString());
                                            }

                                            if (codes.Count > 1)
                                            {
                                                string combinedCode = string.Join("_", codes);
                                                updateQuery = "UPDATE drought_code SET obs_cd = @combinedCode WHERE obs = @obs";
                                                using (var updateConn = new NpgsqlConnection(strConn))
                                                {
                                                    updateConn.Open();
                                                    using (var updateCmd = new NpgsqlCommand(updateQuery, updateConn))
                                                    {
                                                        updateCmd.Parameters.AddWithValue("@combinedCode", combinedCode);
                                                        updateCmd.Parameters.AddWithValue("@obs", obs);

                                                        int rowsAffected = updateCmd.ExecuteNonQuery();
                                                        if (rowsAffected > 0)
                                                        {
                                                            WriteStatus($"obs: {obs}의 obs_cd가 업데이트되었습니다.");
                                                        }
                                                        else
                                                        {
                                                            WriteStatus($"obs: {obs}의 obs_cd 업데이트에 실패했습니다.");
                                                        }
                                                    }
                                                }
                                            }
                                            else if (codes.Count == 1)
                                            {
                                                updateQuery = "UPDATE drought_code SET obs_cd = @code WHERE obs = @obs";
                                                using (var updateConn = new NpgsqlConnection(strConn))
                                                {
                                                    updateConn.Open();
                                                    using (var updateCmd = new NpgsqlCommand(updateQuery, updateConn))
                                                    {
                                                        updateCmd.Parameters.AddWithValue("@code", codes[0]);
                                                        updateCmd.Parameters.AddWithValue("@obs", obs);

                                                        int rowsAffected = updateCmd.ExecuteNonQuery();
                                                        if (rowsAffected > 0)
                                                        {
                                                            WriteStatus($"obs: {obs}의 obs_cd가 업데이트되었습니다.");
                                                        }
                                                        else
                                                        {
                                                            WriteStatus($"obs: {obs}의 obs_cd 업데이트에 실패했습니다.");
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                WriteStatus($"obs: {obs}에 대한 코드 값이 존재하지 않습니다.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        private void Btn_Process_LoadData(object sender, EventArgs e)
        {

        }






        private bool InitializeDatabase()
        {
            string dbIP = Config.dbIP;
            string dbName = Config.dbName;
            string dbPort = Config.dbPort;
            string dbId = Config.dbId;
            string dbPassword = Config.dbPassword;

            if (PostgreConnectionDB(dbIP, dbName, dbPort, dbId, dbPassword) == true)
            {
                this.WriteStatus("Database 연결 성공");
                return true;
            }
            else
            {
                this.WriteStatus("Database 연결 실패");
                return false;
            }
        }
        private bool PostgreConnectionDB(string dbIP, string dbName, string dbPort, string dbId, string dbPassword)
        {
            try
            {
                string strConn = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                        dbIP, dbPort, dbId, dbPassword, dbName);

                NpgsqlConnection NpgSQLconn = new NpgsqlConnection(strConn);
                NpgSQLconn.Open();

                NpgSQLconn.Close();
                return true;
            }
            catch (Exception ex)
            {
                BaysLogHelper.WriteLog(string.Format("StackTrace : {0}", ex.StackTrace));
                BaysLogHelper.WriteLog(string.Format("Message : {0}", ex.Message));

                return false;
            }

        }


        private void WriteStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteStatus(message)));
            }
            else
            {
                listStatus.Items.Add(string.Format("{0}-{1}", DateTime.Now, message)); // listBox1에 메시지를 추가 (예: 로그 출력)
            }
        }
        private void InitializeLogNBuild()
        {
            //Log설정
            BaysLogManager.ConfigureLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "l4n.xml"));

            this.Text += string.Format(" V{0}.{1}.{2}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build);
        }



        //로그 매니저
        public static class BaysLogManager
        {
            public static void ConfigureLogger(string filePath)
            {
                XmlConfigurator.Configure(new FileInfo(filePath));
            }

            public static void WriteEntry(String message)
            {
                ILog logger = GetLogger();
                logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
            }

            private static ILog GetLogger()
            {
                StackTrace st = new StackTrace();
                MethodBase method = st.GetFrame(2).GetMethod();
                string methodName = method.Name;
                string declareType = method.DeclaringType.Name;
                string callingAssembly = method.DeclaringType.Assembly.FullName;

                return LogManager.GetLogger(callingAssembly + " - " + declareType + "." + methodName);
            }

            public static void WriteEntry(String message, EventLogEntryType type)
            {
                ILog logger = GetLogger();
                logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
                //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type);
            }

            public static void WriteEntry(String message, EventLogEntryType type, int eventID)
            {
                ILog logger = GetLogger();
                logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
                //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type, eventID);
            }

            public static void WriteEntry(String message, EventLogEntryType type, int eventID, short category)
            {
                ILog logger = GetLogger();
                logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
                //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type, eventID, category);
            }

            public static void WriteEntry(String message, EventLogEntryType type, int eventID, short category, byte[] rawData)
            {
                ILog logger = GetLogger();
                logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
                //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type, eventID, category, rawData);
            }
        }
        public class BaysLogHelper
        {
            private ILog logger;
            private static BaysLogHelper instance = new BaysLogHelper();
            private BaysLogHelper()
            {
                log4net.Config.XmlConfigurator.Configure();
                logger = log4net.LogManager.GetLogger("logger");
            }

            public static ILog Logger
            {
                get { return instance.logger; }
            }

            public static void WriteLog(string message)
            {
                BaysLogHelper.Logger.Info(message);
            }

            public static void WriteLog(Exception ex)
            {
                BaysLogHelper.Logger.Error(Trace.CurrentMethodName, ex);
            }
        }
        public class BaysDateTime
        {
            /// <summary>
            /// 생성을 막고, 상속이 가능하게 하기 위한 생성자
            /// </summary>
            protected BaysDateTime()
            {
            }

            #region GetDayNames - 요일명 배열
            /// <summary>
            /// 현재 Culture의 요일의 culture 관련 전체 이름이 들어있는 배열을 반환
            /// </summary>
            /// <returns>일요일,월요일,...,토요일</returns>
            public static string[] GetDayNames()
            {
                return GetDayNames(CultureInfo.CurrentCulture.ToString());
            }
            /// <summary>
            /// 지정된 Culture의 요일의 culture 관련 전체 이름이 들어있는 배열을 반환
            /// </summary>
            /// <returns>일요일,월요일,...,토요일</returns>
            public static string[] GetDayNames(string culture)
            {
                return new CultureInfo(culture, false).DateTimeFormat.DayNames;
            }
            /// <summary>
            /// 지정된 Culture의 요일의 culture 관련 전체 이름이 들어있는 배열을 반환
            /// </summary>
            /// <returns>일요일,월요일,...,토요일</returns>
            public static string[] GetDayNames(CultureInfo culture)
            {
                return culture.DateTimeFormat.DayNames;
            }
            #endregion

            #region GetMonthNames - 월이름 배열
            /// <summary>
            /// 현재 Culture의 월의 culture 관련 전체 이름이 들어있는 배열을 반환
            /// </summary>
            /// <returns>"1월","2월",...,"12월",""</returns>
            public static string[] GetMonthNames()
            {
                return GetMonthNames(CultureInfo.CurrentCulture.ToString());
            }
            /// <summary>
            /// 지정된 Culture의 월의 culture 관련 전체 이름이 들어있는 배열을 반환
            /// </summary>
            /// <returns>"1월","2월",...,"12월",""</returns>
            public static string[] GetMonthNames(string culture)
            {
                return new CultureInfo(culture, false).DateTimeFormat.MonthNames;
            }
            /// <summary>
            /// 지정된 Culture의 월의 culture 관련 전체 이름이 들어있는 배열을 반환
            /// </summary>
            /// <returns>"1월","2월",...,"12월",""</returns>
            public static string[] GetMonthNames(CultureInfo culture)
            {
                return culture.DateTimeFormat.MonthNames;
            }
            #endregion

            #region DayNameNow - 현재 요일명
            /// <summary>
            /// 지정된 culture에 기반한 현재 요일의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>화요일,...</returns>
            public static string DayNameNow()
            {
                return ToDayName(DateTime.Now, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정된 culture에 기반한 현재 요일의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>화요일,...</returns>
            public static string DayNameNow(string culture)
            {
                return ToDayName(DateTime.Now, culture);
            }
            /// <summary>
            /// 지정된 culture에 기반한 현재 요일의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>화요일,...</returns>
            public static string DayNameNow(CultureInfo culture)
            {
                return ToDayName(DateTime.Now, culture);
            }
            #endregion

            #region MonthNameNow - 현재 월이름
            /// <summary>
            /// 지정된 culture에 기반한 현재 월의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>N월,...</returns>
            public static string MonthNameNow()
            {
                return ToMonthName(DateTime.Now, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정된 culture에 기반한 현재 월의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>N월,...</returns>
            public static string MonthNameNow(string culture)
            {
                return ToMonthName(DateTime.Now, culture);
            }
            /// <summary>
            /// 지정된 culture에 기반한 현재 월의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>N월,...</returns>
            public static string MonthNameNow(CultureInfo culture)
            {
                return ToMonthName(DateTime.Now, culture);
            }
            #endregion

            #region WeekOfYearNow - 현재 년에서의 주차
            /// <summary>
            /// 현재 culture의 주시작요일을 기준으로 오늘가 해당해에서 몇번째 주에 속하는지 반환
            /// </summary>
            public static int WeekOfYearNow()
            {
                return ToWeekOfYear(DateTime.Now, CultureInfo.CurrentCulture);
            }
            #endregion

            #region ToDayName - 요일명 구하기
            /// <summary>
            /// 현재 culture에 기반한 지정된 요일의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>화요일,...</returns>
            public static string ToDayName(DateTime date)
            {
                return ToDayName(date, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정된 culture에 기반한 지정된 요일의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>화요일,...</returns>
            public static string ToDayName(DateTime date, string culture)
            {
                return new CultureInfo(culture, false).DateTimeFormat.GetDayName(date.DayOfWeek);
            }
            /// <summary>
            /// 지정된 culture에 기반한 지정된 요일의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>화요일,...</returns>
            public static string ToDayName(DateTime date, CultureInfo culture)
            {
                return culture.DateTimeFormat.GetDayName(date.DayOfWeek);
            }
            #endregion

            #region ToMonthName - 월이름 구하기
            /// <summary>
            /// 현재 culture에 기반한 지정된 월의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>N월,...</returns>
            public static string ToMonthName(DateTime date)
            {
                return ToMonthName(date, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정된 culture에 기반한 지정된 월의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>N월,...</returns>
            public static string ToMonthName(DateTime date, string culture)
            {
                return new CultureInfo(culture, false).DateTimeFormat.GetMonthName(date.Month);
            }
            /// <summary>
            /// 지정된 culture에 기반한 지정된 월의 culture 관련 전체 이름을 반환
            /// </summary>
            /// <returns>N월,...</returns>
            public static string ToMonthName(DateTime date, CultureInfo culture)
            {
                return culture.DateTimeFormat.GetMonthName(date.Month);
            }
            #endregion

            #region ToWeekOfYear - 년에서의 주차
            /// <summary>
            /// 현재 culture의 주시작요일을 기준으로 지정된 날짜가 해당해에서 몇번째 주에 속하는지 반환
            /// </summary>
            public static int ToWeekOfYear(DateTime dt)
            {
                return ToWeekOfYear(dt, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정한 culture의 주시작요일을 기준으로 지정된 날짜가 해당해에서 몇번째 주에 속하는지 반환
            /// </summary>
            public static int ToWeekOfYear(DateTime dt, string culture)
            {
                return ToWeekOfYear(dt, new CultureInfo(culture, false));
            }
            /// <summary>
            /// 지정한 culture의 주시작요일을 기준으로 지정된 날짜가 해당해에서 몇번째 주에 속하는지 반환
            /// </summary>
            public static int ToWeekOfYear(DateTime dt, CultureInfo culture)
            {
                return culture.DateTimeFormat.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
            }
            #endregion

            #region GetFirstDayInMonth, GetLastDayInMonth - 월의 시작일, 종료일
            /// <summary>
            /// 지정한 날짜가 속한 월의 첫 날을 획득
            /// </summary>
            public static DateTime GetFirstDayInMonth(DateTime dtDate)
            {
                return new DateTime(dtDate.Year, dtDate.Month, 1);
            }
            /// <summary>
            /// 지정한 년월의 첫 날을 획득
            /// </summary>
            public static DateTime GetFirstDayInMonth(int year, int month)
            {
                return new DateTime(year, month, 1);
            }
            /// <summary>
            /// 지정한 날짜가 속한 월의 마지막 날을 획득
            /// </summary>
            public static DateTime GetLastDayInMonth(DateTime dtDate)
            {
                return new DateTime(dtDate.Year, dtDate.Month, DateTime.DaysInMonth(dtDate.Year, dtDate.Month));
            }
            /// <summary>
            /// 지정한 년월의 마지막 날을 획득
            /// </summary>
            public static DateTime GetLastDayInMonth(int year, int month)
            {
                return new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }
            #endregion

            #region ToDateTimeString - EzDateTimeFormat 커스텀 형식으로 포맷팅
            /// <summary>
            /// 지정된 날짜를 현재 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
            /// </summary>
            public static string ToDateTimeString(string date, BaysDateTimeFormat format)
            {
                return ToDateTimeString(Convert.ToDateTime(date), format, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정된 날짜를 현재 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
            /// </summary>
            public static string ToDateTimeString(DateTime date, BaysDateTimeFormat format)
            {
                return ToDateTimeString(date, format, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 지정된 날짜를 지정된 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
            /// </summary>
            public static string ToDateTimeString(DateTime date, BaysDateTimeFormat format, string culture)
            {
                return ToDateTimeString(date, format, new CultureInfo(culture, false));
            }
            /// <summary>
            /// 지정된 날짜를 지정된 Culture의 지정된 포맷으로 획득 (null이면 String.Empty)
            /// </summary>
            public static string ToDateTimeString(DateTime date, BaysDateTimeFormat format, CultureInfo culture)
            {
                DateTime dt = DateTime.MinValue;
                //if (date != null && Convert.IsDBNull(date))
                if (date != null)
                {
                    dt = date;
                }

                if (dt > DateTime.MinValue)
                {
                    return culture.DateTimeFormat.Calendar.ToDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond).ToString(GetDateTimeFormatString(format));
                }
                return string.Empty;
            }
            /// <summary>
            /// 오전/오후 제거하고 순수 날짜 포맷 가져오기
            /// </summary>
            /// <param name="dtCurrentDate"></param>
            /// <returns></returns>
            public static String GetPureDateTimeFormat(DateTime dtCurrentDate)
            {
                return BaysConvert.ToString(dtCurrentDate).Replace("오전", "").Replace("오후", "");
            }
            /// <summary>
            /// 단일 포맷 형식의 문자형 날짜 데이터를 날짜 포맷으로 변경
            /// </summary>
            /// <param name="sDate">YYYYMMDDHHMMSS => DateTime Format</param>
            /// <returns></returns>
            public static DateTime SingleFormatStringToDateTime(String sDate)
            {
                DateTime dtResult = DateTime.Now;

                String sYear = sDate.Substring(0, 4);
                String sMonth = sDate.Substring(4, 2);
                String sDay = sDate.Substring(6, 2);
                String sHour = sDate.Substring(8, 2);
                String sMin = sDate.Substring(10, 2);
                String sSec = sDate.Substring(12, 2);

                String sDateTime = sYear + "-" + sMonth + "-" + sDay + " " + sHour + ":" + sMin + ":" + sSec;

                dtResult = Convert.ToDateTime(sDateTime);

                return dtResult;
            }
            /// <summary>
            /// 지정된 형식으로 DateTime형식을 포매팅 하기위한 클래스 (null이면 String.Empty)
            /// </summary>
            private static string GetDateTimeFormatString(BaysDateTimeFormat format)
            {
                switch (format)
                {
                    case BaysDateTimeFormat.yyyyMMdd:
                        return "yyyy-MM-dd";
                    case BaysDateTimeFormat.yyyyMMddHHmmss:
                        return "yyyyMMddHHmmss";
                    case BaysDateTimeFormat.yyyyMMddHH:
                        return "yyyy-MM-dd HH시";

                    case BaysDateTimeFormat.yyyyMd:
                        return "yyyy-M-d";
                    case BaysDateTimeFormat.MMddyyyy:
                        return "MM-dd-yyyy";

                    case BaysDateTimeFormat.Mdyyyy:
                        return "M-d-yyyy";

                    case BaysDateTimeFormat.ddMMyyyy:
                        return "dd-MM-yyyy";

                    case BaysDateTimeFormat.dMyyyy:
                        return "d-M-yyyy";

                    case BaysDateTimeFormat.yyyyMdHHmmss:
                        return "yyyy-M-d HH:mm:ss";

                    case BaysDateTimeFormat.MMddyyyyHHmmss:
                        return "MM-dd-yyyy HH:mm:ss";

                    case BaysDateTimeFormat.MdyyyyHHmmss:
                        return "M-d-yyyy HH:mm:ss";

                    case BaysDateTimeFormat.ddMMyyyyHHmmss:
                        return "dd-MM-yyyy HH:mm:ss";

                    case BaysDateTimeFormat.dMyyyyHHmmss:
                        return "d-M-yyyy HH:mm:ss";
                    case BaysDateTimeFormat.yyyyMM:
                        return "yyyyMM";
                    case BaysDateTimeFormat.yyyyMMddNonSeperator:
                        return "yyyyMMdd";
                    case BaysDateTimeFormat.MMdd:
                        return "MM/dd";
                }
                return "yyyy-MM-dd HH:mm:ss";
            }
            #endregion

            #region ToFullDateTimeString -  ShortDateString + ShortTimeString (yyyy-MM-dd HH:mm:ss) 형식
            /// <summary>
            /// 현재시간을 현재 culture의 ShortDatePattern형식에 HH:mm:ss형식을 더한 형식 반환
            /// 한국 yyyy-MM-dd HH:mm:ss
            /// </summary>
            /// <returns>한국은 yyyy-MM-dd HH:mm:ss</returns>
            public static string ToFullDateTimeString()
            {
                return ToFullDateTimeString(CultureInfo.CurrentCulture.ToString());
            }
            /// <summary>
            /// 현재시간을 지정된 culture의 ShortDatePattern형식에 HH:mm:ss형식을 더한 형식 반환
            /// 한국 yyyy-MM-dd HH:mm:ss
            /// </summary>
            /// <returns>한국은 yyyy-MM-dd HH:mm:ss</returns>
            public static string ToFullDateTimeString(string culture)
            {
                DateTimeFormatInfo info = new CultureInfo(culture, false).DateTimeFormat;
                return DateTime.Now.ToString(string.Format("{0} {1}", info.ShortDatePattern, "HH:mm:ss"));
            }
            #endregion

            #region ToLongDateString -  LongDatePattern(yyyy년 M월 d일 *요일) 형식
            /// <summary>
            /// 현재시간을 현재 culture의 LongDatePattern형식 반환
            /// </summary>
            /// <returns>yyyy년 M월 d일 *요일</returns>
            public static string ToLongDateString()
            {
                return ToLongDateString(CultureInfo.CurrentCulture.ToString());
            }
            /// <summary>
            /// 현재시간을 지정된 culture의 LongDatePattern형식 반환
            /// </summary>
            /// <returns>yyyy년 M월 d일 *요일</returns>
            public static string ToLongDateString(string culture)
            {
                return ToLongDateString(new CultureInfo(culture, false));
            }
            /// <summary>
            /// 현재시간을 지정된 culture의 LongDatePattern형식 반환
            /// </summary>
            /// <returns>yyyy년 M월 d일 *요일</returns>
            public static string ToLongDateString(CultureInfo culture)
            {
                DateTimeFormatInfo info = culture.DateTimeFormat;
                return DateTime.Now.ToString(info.LongDatePattern);
            }
            #endregion

            #region ToLongTimeString - LongTimePattern(한국 - 오후 H:mm:ss) 형식
            /// <summary>
            /// 현재시간을 현재 culture의 LongTimePattern형식 반환
            /// </summary>
            /// <returns>한국 - 오후 H:mm:ss</returns>
            public static string ToLongTimeString()
            {
                return ToLongTimeString(CultureInfo.CurrentCulture.ToString());
            }
            /// <summary>
            /// 현재시간을 현재 culture의 LongTimePattern형식 반환
            /// </summary>
            /// <returns>한국 - 오후 H:mm:ss</returns>
            public static string ToLongTimeString(string culture)
            {
                return ToLongTimeString(new CultureInfo(culture, false));
            }
            /// <summary>
            /// 현재시간을 현재 culture의 LongTimePattern형식 반환
            /// </summary>
            /// <returns>한국 - 오후 H:mm:ss</returns>
            public static string ToLongTimeString(CultureInfo culture)
            {
                DateTimeFormatInfo info = culture.DateTimeFormat;
                return DateTime.Now.ToString(info.LongTimePattern);
            }
            #endregion

            #region [ LocalDateTime , UTC DateTime ]

            //public static DateTime[] GetLocalDateTimeAndGetUTCDateTime()
            //{
            //String sLocalDateTime = ConstFactory.GetCurrentDateTime;
            //DateTime dtLocalDateTime = GMConvert.ToDateTime(sLocalDateTime);
            //DateTime dtUTCDateTime = GMDateTime.ConvertToLocalDateTimeToUTCTime(dtLocalDateTime);
            //sLocalDateTime = dtLocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            //String sUTCDateTime = dtUTCDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            //DateTime[] dtResult = new DateTime[2];

            //dtResult[0] = dtLocalDateTime;
            //dtResult[1] = dtUTCDateTime;

            //return dtResult;
            //}

            /// <summary>
            /// DateTime.Now.ToLocalTime
            /// </summary>
            /// <returns></returns>
            public static DateTime GetLocalTime()
            {
                return DateTime.Now.ToLocalTime();
            }
            /// <summary>
            /// DateTime.Now.ToLocalTime().ToUniversalTime
            /// </summary>
            /// <returns></returns>
            public static DateTime GetUTCTime()
            {
                return DateTime.Now.ToLocalTime().ToUniversalTime();
            }
            /// <summary>
            /// Local Date Time To UTC DateTime
            /// </summary>
            /// <param name="dtLocalDateTime">YYYY-MM-DD HH:MM:SS => UTC Time</param>
            /// <returns></returns>
            public static DateTime ConvertToLocalDateTimeToUTCTime(DateTime dtLocalDateTime)
            {
                return dtLocalDateTime.ToUniversalTime();
            }
            #endregion

            #region ToShortDateString - 한국 yyyy-MM-dd 형식
            /// <summary>
            /// 현재시간을 현재 culture의 ShortDatePattern형식 반환
            /// 한국 yyyy-MM-dd
            /// </summary>
            /// <returns>한국은 yyyy-MM-dd</returns>
            public static string ToShortDateString()
            {
                return ToShortDateString(CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 현재시간을 지정된 culture의 ShortDatePattern형식 반환
            /// 한국 yyyy-MM-dd
            /// </summary>
            /// <returns>한국은 yyyy-MM-dd</returns>
            public static string ToShortDateString(string culture)
            {
                return ToShortDateString(new CultureInfo(culture, false));
            }
            /// <summary>
            /// 현재시간을 지정된 culture의 ShortDatePattern형식 반환
            /// 한국 yyyy-MM-dd
            /// </summary>
            /// <returns>한국은 yyyy-MM-dd</returns>
            public static string ToShortDateString(CultureInfo culture)
            {
                DateTimeFormatInfo info = culture.DateTimeFormat;
                return DateTime.Now.ToString(info.ShortDatePattern);
            }
            #endregion

            #region ToShortTimeString - HH:mm:ss 형식
            /// <summary>
            /// 현재시간을 현재 culture의 HH:mm:ss형식반환
            /// 원래 ShortTimePattern은 - 오전 H:mm 형식인데 여기서는 Custom구현
            /// HH:mm:ss
            /// </summary>
            /// <returns>HH:mm:ss</returns>
            public static string ToShrotTimeString()
            {
                return DateTime.Now.ToString("HH:mm:ss");
            }
            #endregion

            #region GetTimeSpan
            /// <summary>
            /// 두시간사이의 차이 반환. TimeSpan의 절대값 Duration
            /// </summary>
            public static TimeSpan GetTimeSpan(DateTime dt1, DateTime dt2)
            {
                return dt2.Subtract(dt1).Duration();
            }
            /// <summary>
            /// 두시간사이의 차이 반환. TimeSpan의 절대값 Duration
            /// </summary>
            public static TimeSpan GetTimeSpan(string dt1, string dt2)
            {
                DateTime time = Convert.ToDateTime(dt1);
                DateTime time2 = Convert.ToDateTime(dt2);
                return GetTimeSpan(time, time2);
            }
            #endregion


        }
        public class BaysConvert : BaysDateTime
        {
            /// <summary>
            /// 생성을 막고, 상속이 가능하게 하기 위한 생성자
            /// </summary>
            protected BaysConvert()
            {
            }

            #region 언어관련
            /// <summary>
            /// Int를 상속받은 열거형의 숫자값을 문자열로 반환 (기본적인 ToString()은 열거형값의 문자열을 그대로 반환한다.)
            /// </summary>
            /// <param name="enumValue">Int를 상속받은 열거형값</param>
            /// <returns>열거형값 문자열 : "1"...</returns>
            public static string EnumToIntString(object enumValue)
            {
                if (enumValue.GetType().BaseType == null || enumValue.GetType().BaseType.FullName != "System.Enum")
                {
                    throw new ArgumentException("파라미터값이 열거형이 아닙니다.(Enum필요)", "enumValue");
                }
                return ((int)enumValue).ToString();
            }

            /// <summary>
            /// Where절의 Like조건으로 적용할 수 있도록 "%"기호 처리
            /// </summary>
            /// <param name="text">%를 포함하지 않는 Like문자열</param>
            /// <returns>%처리된 조건절 값</returns>
            public static string ToLikeClause(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return "%";
                }
                return "%" + text + "%";
            }
            #endregion

            #region null, DBNull, EmptyString 체크
            /// <summary>
            /// DBNull 또는 null인지 검사
            /// </summary>
            public static bool IsNull(object val)
            {
                return (val == null || Convert.IsDBNull(val));
            }
            /// <summary>
            /// DBNull 또는 null인지 검사
            /// </summary>
            public static bool IsNullOrEmpty(object val)
            {
                return (val == null || Convert.IsDBNull(val) || string.Format("{0}", val) == string.Empty);
            }
            /// <summary>
            /// DBNull 또는 null이 아닌지 검사
            /// </summary>
            public static bool IsNotNull(object val)
            {
                return !IsNull(val);
            }
            /// <summary>
            /// DBNull 또는 null이 아닌지 검사
            /// </summary>
            public static bool IsNotNullOrEmpty(object val)
            {
                return !IsNullOrEmpty(val);
            }
            /// <summary>
            /// 문자열이 Null 또는 비었는지 검사
            /// </summary>
            public static bool IsStringNullOrEmpty(object val)
            {
                return (val == null || string.IsNullOrEmpty(val.ToString()));
            }
            /// <summary>
            /// 문자열이 Null 또는 비었는지 검사
            /// </summary>
            public static bool IsStringNullOrEmpty(string val)
            {
                return string.IsNullOrEmpty(val.ToString());
            }
            #endregion null, DBNull, EmptyString 체크

            #region TypeConvert ///////////////////////////////////////////////
            //참고 ADO.NET 
            // Generic version : public static T ReadValue<T>(object value)
            // Runtime Type version : public static object ReadValue(object value, Type targetType)

            /// <summary>
            /// Object별 기본값 반환 (값, 참조타입 지원)
            /// </summary>
            public static T DefaultValue<T>()
            {
                return (T)((typeof(T).IsValueType) ? Activator.CreateInstance(typeof(T)) : default(T));
            }

            /// <summary>
            /// Object -> CustomType 변환 (기본값 처리)
            /// </summary>
            public static T ChangeType<T>(object val) where T : IConvertible
            {
                T defaultValue = DefaultValue<T>();
                if (IsNotNullOrEmpty(val))
                {
                    //T typedval = (T)val;  //cast error
                    T typedval = (T)Convert.ChangeType(val, typeof(T));
                    return (typedval != null) ? typedval : defaultValue;
                }
                return defaultValue;
            }

            /// <summary>
            /// Object -> CustomType 변환 (기본값 처리)
            /// </summary>
            public static T ChangeType<T>(object val, T defaultValue) where T : IConvertible
            {
                if (IsNotNullOrEmpty(val))
                {
                    //T typedval = (T)val;  //cast error
                    T typedval = (T)Convert.ChangeType(val, typeof(T));
                    return (typedval != null) ? typedval : defaultValue;
                }
                return defaultValue;
            }

            /// <summary>
            /// Object -> CustomType 변환 (기본값 처리)
            /// </summary>
            public static T ChangeType<T>(object val, T defaultValue, IFormatProvider provider) where T : IConvertible
            {
                if (IsNotNullOrEmpty(val))
                {
                    //T typedval = (T)val;  //cast error
                    T typedval = (T)Convert.ChangeType(val, typeof(T), provider);
                    return (typedval != null) ? typedval : defaultValue;
                }
                return defaultValue;
            }

            #region ToBoolean
            /// <summary>
            /// Object -> bool 변환 (기본값 처리)
            /// </summary>
            public static bool ToBoolean(object val, bool defaultValue)
            {
                return ChangeType<bool>(val, defaultValue);
            }
            /// <summary>
            /// Object -> bool 변환 (기본값 false - null인경우)
            /// </summary>
            public static bool ToBoolean(object val)
            {
                return ChangeType<bool>(val, false);
            }
            #endregion

            #region ToByte
            /// <summary>
            /// Object -> byte변환 (기본값 처리)
            /// </summary>
            public static byte ToByte(object val, byte defaultValue)
            {
                return ChangeType<byte>(val, defaultValue);
            }
            /// <summary>
            /// Object -> byte변환 (기본값 0 - null인경우)
            /// </summary>
            public static byte ToByte(object val)
            {
                return ChangeType<byte>(val, 0);
            }
            #endregion

            #region ToChar
            /// <summary>
            /// Object -> char변환 (기본값 처리)
            /// </summary>
            public static char ToChar(object val, char defaultValue)
            {
                return ChangeType<char>(val, defaultValue);
            }
            /// <summary>
            /// Object -> char변환 (기본값 \0 - null인경우)
            /// </summary>
            public static char ToChar(object val)
            {
                return ChangeType<char>(val, '\0');
            }
            #endregion

            #region ToDateTime
            /// <summary>
            /// Object -> DateTime 변환 (기본값 처리)
            /// </summary>
            public static DateTime ToDateTime(object val, DateTime defaultValue)
            {
                return ChangeType<DateTime>(val, defaultValue);
            }
            /// <summary>
            /// Object -> DateTime 변환 (기본값 DateTime.MinValue - null인경우)
            /// </summary>
            public static DateTime ToDateTime(object val)
            {
                return ChangeType<DateTime>(val, DateTime.MinValue);
            }
            #endregion

            #region ToDecimal
            /// <summary>
            /// Object -> decimal 변환 (기본값 처리)
            /// </summary>
            public static decimal ToDecimal(object val, decimal defaultValue)
            {
                return ChangeType<decimal>(val, defaultValue);
            }
            /// <summary>
            /// Object -> decimal 변환 (기본값 0M - null인경우)
            /// </summary>
            public static decimal ToDecimal(object val)
            {
                return ChangeType<decimal>(val, 0M);
            }
            #endregion

            #region ToDouble
            /// <summary>
            /// Object -> double변환 (기본값 처리)
            /// </summary>
            public static double ToDouble(object val, double defaultValue)
            {
                return ChangeType<double>(val, defaultValue);
            }
            /// <summary>
            /// Object -> double변환 (기본값 -1 - null인경우)
            /// </summary>
            public static double ToDouble(object val)
            {
                return ChangeType<double>(val, -1);
            }
            #endregion

            #region ToInt16, ToInt32, ToInt64, ToInt (32)
            /// <summary>
            /// Object -> Int16변환 (기본값 처리)
            /// </summary>
            public static Int16 ToInt16(object val, Int16 defaultValue)
            {
                return ChangeType<Int16>(val, defaultValue);
            }
            /// <summary>
            /// Object -> Int16변환 (기본값 -1 - null인경우)
            /// </summary>
            public static Int16 ToInt16(object val)
            {
                return ChangeType<Int16>(val, -1);
            }

            /// <summary>
            /// Object -> Int32변환 (기본값 처리)
            /// </summary>
            public static Int32 ToInt32(object val, Int32 defaultValue)
            {
                return ChangeType<Int32>(val, defaultValue);
            }
            /// <summary>
            /// Object -> Int32변환 (기본값 -1 - null인경우)
            /// </summary>
            public static Int32 ToInt32(object val)
            {
                return ChangeType<Int32>(val, -1);
            }

            /// <summary>
            /// Object -> Int64변환 (기본값 처리)
            /// </summary>
            public static Int64 ToInt64(object val, Int64 defaultValue)
            {
                return ChangeType<Int64>(val, defaultValue);
            }
            /// <summary>
            /// Object -> Int64변환 (기본값 -1 - null인경우)
            /// </summary>
            public static Int64 ToInt64(object val)
            {
                return ChangeType<Int64>(val, -1);
            }

            /// <summary>
            /// Object -> int변환 (기본값 처리)
            /// </summary>
            public static int ToInt(object val, int defaultValue)
            {
                return ChangeType<int>(val, defaultValue);
            }
            /// <summary>
            /// Object -> int변환 (기본값 -1 - null인경우)
            /// </summary>
            public static int ToInt(object val)
            {
                return ChangeType<int>(val, -1);
            }
            #endregion

            #region ToSByte
            /// <summary>
            /// Object -> sbyte변환 (기본값 처리)
            /// </summary>
            public static sbyte ToSByte(object val, sbyte defaultValue)
            {
                return ChangeType<sbyte>(val, defaultValue);
            }
            /// <summary>
            /// Object -> sbyte변환 (기본값 127 - null인경우)
            /// </summary>
            public static sbyte ToSByte(object val)
            {
                return ChangeType<sbyte>(val, 127);
            }
            #endregion

            #region ToSingle
            /// <summary>
            /// Object -> Single변환 (기본값 처리)
            /// </summary>
            public static Single ToSingle(object val, Single defaultValue)
            {
                return ChangeType<Single>(val, defaultValue);
            }
            /// <summary>
            /// Object -> Single변환 (기본값 Single.NaN - null인경우)
            /// </summary>
            public static Single ToSingle(object val)
            {
                return ChangeType<Single>(val, Single.NaN);
            }
            #endregion

            #region ToString
            /// <summary>
            /// Object -> String변환 (기본값, 포맷 지원)
            /// </summary>
            public static string ToString(object val, object defaultValue, string format)
            {
                return (IsNotNullOrEmpty(val)) ? string.Format(format, val) : string.Format(format, defaultValue);
            }
            /// <summary>
            /// Object -> String변환 (기본값 지원)
            /// </summary>
            public static string ToString(object val, object defaultValue)
            {
                return (IsNotNullOrEmpty(val)) ? val.ToString() : defaultValue.ToString();
            }
            /// <summary>
            /// Object -> String변환 (기본값 string.Empty)
            /// </summary>
            public static string ToString(object val)
            {
                return ToString(val, string.Empty);
            }
            #endregion

            #region ToUInt16, ToUInt32, ToUInt64, ToUInt (32)
            /// <summary>
            /// Object -> UInt16변환 (기본값 처리)
            /// </summary>
            public static UInt16 ToUInt16(object val, UInt16 defaultValue)
            {
                return ChangeType<UInt16>(val, defaultValue);
            }
            /// <summary>
            /// Object -> UInt16변환 (기본값 0 - null인경우)
            /// </summary>
            public static UInt16 ToUInt16(object val)
            {
                return ChangeType<UInt16>(val, 0);
            }

            /// <summary>
            /// Object -> UInt32변환 (기본값 처리)
            /// </summary>
            public static UInt32 ToUInt32(object val, UInt32 defaultValue)
            {
                return ChangeType<UInt32>(val, defaultValue);
            }
            /// <summary>
            /// Object -> UInt32변환 (기본값 0 - null인경우)
            /// </summary>
            public static UInt32 ToUInt32(object val)
            {
                return ChangeType<UInt32>(val, 0);
            }

            /// <summary>
            /// Object -> UInt64변환 (기본값 처리)
            /// </summary>
            public static UInt64 ToUInt64(object val, UInt64 defaultValue)
            {
                return ChangeType<UInt64>(val, defaultValue);
            }
            /// <summary>
            /// Object -> UInt64변환 (기본값 0 - null인경우)
            /// </summary>
            public static UInt64 ToUInt64(object val)
            {
                return ChangeType<UInt64>(val, 0);
            }

            /// <summary>
            /// Object -> uint변환 (기본값 처리)
            /// </summary>
            public static uint ToUInt(object val, uint defaultValue)
            {
                return ChangeType<uint>(val, defaultValue);
            }
            /// <summary>
            /// Object -> uint변환 (기본값 0 - null인경우)
            /// </summary>
            public static uint ToUInt(object val)
            {
                return ChangeType<uint>(val, 0);
            }
            #endregion

            #region ToCurrencyFormat : 숫자형Object -> 통화형 문자열
            /// <summary>
            /// Object -> 통화형 문자열
            /// </summary>
            public static string ToCurrencyFormat(object number)
            {
                return ToCurrencyFormat(number, CultureInfo.CurrentCulture);
            }

            /// <summary>
            /// Object -> 통화형 문자열
            /// </summary>
            public static string ToCurrencyFormat(object number, string culture)
            {
                return string.Format("{0:N" + CurrencyDecimalDigits(culture) + "}", number);
            }

            /// <summary>
            /// Object -> 통화형 문자열
            /// </summary>
            public static string ToCurrencyFormat(object number, CultureInfo culture)
            {
                return string.Format("{0:N" + CurrencyDecimalDigits(culture) + "}", number);
            }

            #region 문화권 통화 소수자리수
            /// <summary>
            /// 현재 문화권에 따른 통화 값에 사용할 소수 자릿수
            /// </summary>
            private static int CurrencyDecimalDigits()
            {
                return CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits;
            }

            /// <summary>
            /// 문화권에 따른 통화 값에 사용할 소수 자릿수
            /// </summary>
            private static int CurrencyDecimalDigits(string culture)
            {
                return new CultureInfo(culture, false).NumberFormat.CurrencyDecimalDigits;
            }

            /// <summary>
            /// 문화권에 따른 통화 값에 사용할 소수 자릿수
            /// </summary>
            private static int CurrencyDecimalDigits(CultureInfo culture)
            {
                return culture.NumberFormat.CurrencyDecimalDigits;
            }
            #endregion
            #endregion

            #endregion TypeConvert

            #region Dictionary 헬퍼 (Dictionary->ListItem, Dictionary의 키와 값을 치환)

            /// <summary>
            /// Dictionary를 ListItem목록으로 변환한다.
            /// </summary>
            /// <typeparam name="T1">Dictionary Key Type</typeparam>
            /// <typeparam name="T2">Dictionary Value Type</typeparam>
            /// <param name="dic">dictionary</param>
            /// <returns>Dictionary에서 변환된 ListItem리스트</returns>
            //public static List<ListItem> ConvertToListItems<T1, T2>(Dictionary<T1, T2> dic)
            //{
            //    List<ListItem> items = new List<ListItem>();
            //    foreach (T1 key in dic.Keys)
            //    {
            //        items.Add(new ListItem(GMConvert.ToString(dic[key]), GMConvert.ToString(key)));
            //    }
            //    return items;
            //}

            /// <summary>
            /// Dictionary의 키와 값을 치환한다.
            /// </summary>
            /// <typeparam name="T1">Dictionary Key Type</typeparam>
            /// <typeparam name="T2">Dictionary Value Type</typeparam>
            /// <param name="dic">dictionary</param>
            /// <returns>키와 값이 치환된 사전</returns>
            public static Dictionary<T2, T1> SwitchKeyAndValueInDictionary<T1, T2>(Dictionary<T1, T2> dic)
            {
                Dictionary<T2, T1> revDic = new Dictionary<T2, T1>();
                foreach (T1 key in dic.Keys)
                {
                    if (revDic.ContainsKey(dic[key]))
                    {
                        throw new ArgumentException("사전의 값에 중복되는 값이 있어서, 키와 값을 치환할수 없습니다.", "dic");
                    }
                    revDic.Add(dic[key], key);
                }
                return revDic;
            }

            #endregion

            /// <summary>
            /// 파일사이즈를 문자열로 변환 (n -> nGB, nMB, nKB, nByte) 
            /// 1024단위로 나누어질때만 Unit을 상향조정
            /// </summary>
            /// <param name="fileLength">파일사이즈</param>
            /// <returns>파일 사이즈 문자열</returns>
            public static string ToFileSizeString(int fileLength)
            {
                if (fileLength % (1024 * 1024 * 1024) == 0)
                    return (fileLength / (1024 * 1024 * 1024)).ToString() + "GB";
                if (fileLength % (1024 * 1024) == 0)
                    return (fileLength / (1024 * 1024)).ToString() + "MB";
                if (fileLength % (1024) == 0)
                    return (fileLength / (1024)).ToString() + "KB";
                return fileLength.ToString() + "Bytes";
            }

            /// <summary>
            /// 태그가 제거된 문자열로 변환합니다.
            /// </summary>
            /// <param name="contents">태그를 포함한 내용</param>
            /// <returns>태그가 제거된 문자열</returns>
            public static string ToTagRemovedString(string contents)
            {
                return Regex.Replace(contents, "<(/)?([a-zA-Z]*)(\\s[a-zA-Z]*=[^>]*)?(\\s)*(/)?>", "");
            }

            /// <summary>
            /// 태그가 제거되고 생략처리(...)된 문자열로 변환합니다.
            /// </summary>
            /// <param name="contents">태그를 포함한 내용</param>
            /// <param name="limitLength">전체 제한 길이</param>
            /// <param name="ellipsis">생략기호</param>
            /// <returns>태그가 제거되고 생략처리된 문자열</returns>
            public static string ToTagRemovedEllipsisString(string contents, int limitLength, string ellipsis)
            {
                if (string.IsNullOrEmpty(contents))
                    return contents;

                contents = ToTagRemovedString(contents);

                if (contents.Length > limitLength)
                {
                    //생략처리
                    contents = contents.Substring(0, limitLength - ellipsis.Length) + ellipsis;
                }

                return contents;
            }

            /// <summary>
            /// 태그가 제거되고 생략처리(...)된 문자열로 변환합니다.
            /// </summary>
            /// <param name="contents">태그를 포함한 내용</param>
            /// <param name="limitLength">전체 제한 길이</param>
            /// <param name="ellipsis">생략기호</param>
            /// <returns>태그가 제거되고 생략처리된 문자열</returns>
            public static string ToTagRemovedEllipsisString(string contents, int limitLength)
            {
                return ToTagRemovedEllipsisString(contents, limitLength, "...");
            }

            /// <summary>
            /// null체크
            /// </summary>
            /// <param name="value">피대상자</param>
            /// <param name="returnVal">null인경우 대체값</param>
            /// <returns></returns>
            public static string IsNull(object value, string returnVal)
            {
                return (value == null ? returnVal : (string)value);
            }

            /// <summary>
            /// bool의 값을 1,0으로 대체
            /// </summary>
            /// <param name="val">bool value</param>
            /// <returns></returns>
            public static string Bool2String(bool val)
            {
                return (val ? "1" : "0");
            }

            /// <summary>
            /// 1,0의 값을 bool형으로 대체
            /// </summary>
            /// <param name="val">string value</param>
            /// <returns></returns>
            public static bool String2Bool(string val)
            {
                return (val == "1" ? true : false);
            }

            /// <summary>
            /// 2자리 문자로 맞춰주는 메소드
            /// </summary>
            /// <param name="val"></param>
            /// <returns></returns>
            public static String SetTwoString(String val)
            {
                if (val.Length == 1)
                    val = "0" + val;

                return val;
            }
            /// <summary>
            /// 앞자리를 가져오는 메소드
            /// 11, 13, 15, 18, 25, 32, 35
            /// 10, 10, 10, 10, 20, 30, 30
            /// </summary>
            /// <param name="val"></param>
            /// <returns></returns>
            public static String GetTenOfTheMultiplier(String val)
            {
                if (val.Length == 1)
                {
                    val = "0" + val;
                }
                else if (val.Length == 2)
                {
                    val = val.Substring(0, 1) + "0";
                }
                else
                {
                    val = "";
                }

                return val;
            }
        }
        public enum BaysDateTimeFormat
        {
            /// <summary>2007-02-19</summary>
            yyyyMMdd,
            /// <summary>
            /// 2010-02-03 23시
            /// </summary>
            yyyyMMddHH,
            /// <summary>
            /// 20070205
            /// </summary>
            yyyyMMddNonSeperator,
            /// <summary>2007-2-19</summary>
            yyyyMd,
            /// <summary>201001</summary>
            yyyyMM,
            /// <summary>02-19-2007</summary>
            MMddyyyy,
            /// <summary>2-19-2007</summary>
            Mdyyyy,
            /// <summary>19-02-2007</summary>
            ddMMyyyy,
            /// <summary>19-2-2007</summary>
            dMyyyy,
            /// <summary>2007-02-19 18:44:53</summary>
            yyyyMMddHHmmss,
            /// <summary>2007-2-19 18:44:53</summary>
            yyyyMdHHmmss,
            /// <summary>02-19-2007 18:44:53</summary>
            MMddyyyyHHmmss,
            /// <summary>2-19-2007 18:44:53</summary>
            MdyyyyHHmmss,
            /// <summary>19-02-2007 18:44:53</summary>
            ddMMyyyyHHmmss,
            /// <summary>19-2-2007 18:44:53</summary>
            dMyyyyHHmmss,
            /// <summary>
            /// 01/09
            /// </summary>
            MMdd,
        }



        public sealed class Trace
        {
            /// <summary>
            /// 현재 함수명
            /// </summary>
            public static string CurrentMethodName
            {
                get { return new System.Diagnostics.StackFrame(1).GetMethod().Name; }
            }
            /// <summary>
            /// 현재 함수명 출력
            /// </summary>
            public static void EzTraceMethodName()
            {
                WriteLine(new System.Diagnostics.StackFrame(1).GetMethod().Name);
            }
            /// <summary>
            /// 현재 함수명 출력(메세지 포함)
            /// </summary>
            public static void EzTraceMethodName(string message)
            {
                WriteLine(new System.Diagnostics.StackFrame(1).GetMethod().Name + " : " + message);
            }
            /// <summary>
            /// 현재 함수명 출력(메세지 포함)
            /// </summary>
            public static void EzTraceMethodName(string format, params object[] args)
            {
                WriteLine(new System.Diagnostics.StackFrame(1).GetMethod().Name + " : " + string.Format(format, args));
            }

            //
            // 요약:
            //     조건을 확인한 다음 해당 조건이 false이면 메시지를 표시합니다.
            //
            // 매개 변수:
            //   message:
            //     표시할 메시지입니다.
            //
            //   condition:
            //     메시지를 표시하지 않으려면 true이고, 그렇지 않으면 false입니다.
            [Conditional("TRACE")]
            public static void Assert(bool condition, string message)
            {
                if (condition) return;

                Trace.Assert(condition, message);
            }
            public static void WriteLine(string message)
            {
                Trace.WriteLine(message);
            }

            //
        }
        public static class Config
        {
            #region [Postgre Database 설정관련]
            public static string dbIP
            {
                get { return BaysConvert.ToString(ConfigurationManager.AppSettings["dbIP"]); }
            }

            public static string dbName
            {
                get { return BaysConvert.ToString(ConfigurationManager.AppSettings["dbName"]); }
            }

            public static string dbPort
            {
                get { return BaysConvert.ToString(ConfigurationManager.AppSettings["dbPort"]); }
            }

            public static string dbId
            {
                get { return BaysConvert.ToString(ConfigurationManager.AppSettings["dbId"]); }
            }

            public static string dbPassword
            {
                get { return BaysConvert.ToString(ConfigurationManager.AppSettings["dbPassword"]); }
            }
            #endregion
        }


    }
}
