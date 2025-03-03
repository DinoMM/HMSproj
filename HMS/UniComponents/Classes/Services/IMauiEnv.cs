using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents.Services
{
    public interface IMauiEnv
    {
        public Task RunSpecific(
            Action? method = null,
            Action? methodR = null,
            Action? methodD = null,
            Func<Task>? asyncMethod = null,
            Func<Task>? asyncMethodR = null,
            Func<Task>? asyncMethodD = null,
            bool except = false,
            params Platform[] runOnThose);

        public Task RunOnlyWindows(
           Action? method = null,
           Action? methodR = null,
           Action? methodD = null,
           Func<Task>? asyncMethod = null,
           Func<Task>? asyncMethodR = null,
           Func<Task>? asyncMethodD = null);

        /// <summary>
        /// Spusti vybrané metody na zaklade zvolenej platformy Android.<para/>
        /// ' ' - spusti sa vzdy pred Release a Debug<para/>R - Release<para/>D - Debug
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodR"></param>
        /// <param name="methodD"></param>
        /// <param name="asyncMethod"></param>
        /// <param name="asyncMethodR"></param>
        /// <param name="asyncMethodD"></param>
        /// <returns></returns>
        public Task RunOnlyAndroid(
            Action? method = null,
            Action? methodR = null,
            Action? methodD = null,
            Func<Task>? asyncMethod = null,
            Func<Task>? asyncMethodR = null,
            Func<Task>? asyncMethodD = null);

        /// <summary>
        /// Spusti vybrané metody na zaklade zvolenej platformy IOS.<para/>
        /// ' ' - spusti sa vzdy pred Release a Debug<para/>R - Release<para/>D - Debug
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodR"></param>
        /// <param name="methodD"></param>
        /// <param name="asyncMethod"></param>
        /// <param name="asyncMethodR"></param>
        /// <param name="asyncMethodD"></param>
        /// <returns></returns>
        public Task RunOnlyIos(
            Action? method = null,
            Action? methodR = null,
            Action? methodD = null,
            Func<Task>? asyncMethod = null,
            Func<Task>? asyncMethodR = null,
            Func<Task>? asyncMethodD = null);

        /// <summary>
        /// Spusti vybrané metody na zaklade zvolenej platformy MACCATALYST.<para/>
        /// ' ' - spusti sa vzdy pred Release a Debug<para/>R - Release<para/>D - Debug
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodR"></param>
        /// <param name="methodD"></param>
        /// <param name="asyncMethod"></param>
        /// <param name="asyncMethodR"></param>
        /// <param name="asyncMethodD"></param>
        /// <returns></returns>
        public Task RunOnlyMac(
            Action? method = null,
            Action? methodR = null,
            Action? methodD = null,
            Func<Task>? asyncMethod = null,
            Func<Task>? asyncMethodR = null,
            Func<Task>? asyncMethodD = null);

        public bool IsWindows();
        public bool IsAndroid();
        public bool IsIos();
        public bool IsMac();
    }
    public enum Platform
    {
        Windows,
        Android,
        Ios,
        Mac
    }
}
