using log4net;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI.WebControls;

namespace Utils
{

    public class LogUtil
    {

        private static readonly ILog logInfo = LogManager.GetLogger("Log4j");
        private static readonly ILog logErr = LogManager.GetLogger("Err");
        private static DateTime PrevTime=DateTime.Now;
        public static bool IsDebug { get; set; }
        public static void Info(string msg) {
            logInfo.Info(msg);
        }
        public static void Warn(string msg) {
            logInfo.Warn(msg);
        }
        public static void Error(string msg) {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            MethodBase methodBase = stackFrame.GetMethod();
            logErr.Error("类名:" + methodBase.ReflectedType.Name + " 方法名:" + methodBase.Name + " 信息:" + msg);
        }
        public static void Debug(string msg) {
            if (!IsDebug) 
                return;
            logInfo.Debug(msg+$"执行时间{(DateTime.Now-PrevTime).TotalMilliseconds.ToString("0")}ms");
            PrevTime=DateTime.Now;
        }
    }
}