using AutomationTool.Model;
using FlaUI.Core.AutomationElements;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
namespace AutomationTool.DataSource.Steps
{
    public class UnknownStep(AutoStep _autoStep) : IStep
    {
        AutomationElement? elementUI;
        public AutomationElement? GetElementUI() => elementUI;

        public async Task<bool> Action()
        {
            bool result = false;
            switch (_autoStep.ActionType)
            {
                case ActionTypes.ShowMessageBox:
                    App.Bus.Publish<ShowMessage>(new(_autoStep.Param0, "Run Process"));
                    result = true;
                    break;

                case ActionTypes.DeleteFile:
                    result = DeleteFile(_autoStep.Param0);
                    break;

                case ActionTypes.CopyFile:
                    result = CopyFile(_autoStep.Param0, _autoStep.Param1);
                    break;

                case ActionTypes.CompareFile:
                    result = await CompareFile(_autoStep.Param0, _autoStep.Param1, _autoStep.Param2);
                    break;
                    
                case ActionTypes.ExistedFile:
                    result = ExistedFile(_autoStep.Param0, _autoStep.Param1);
                    break;

                case ActionTypes.ChangeDateTime:
                    result = ChangeDateTime(_autoStep.Param0, _autoStep.Param1);
                    break;

                case ActionTypes.ResetDateTime:
                    result = ChangeDateTime(string.Empty, string.Empty);
                    break;

                case ActionTypes.RestartService:
                    result = RestartService(_autoStep.Param0);
                    break;

                case ActionTypes.WaitTime:
                    result = await WaitTime(_autoStep.Param0);
                    break;
            }
            return result;
        }

        private bool DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ExistedFile(string file, string exited)
        {
            try
            {
                bool.TryParse(exited, out bool exitedBool);
                return File.Exists(file) == exitedBool;
            }
            catch
            {
                return false;
            }
        }

        private bool CopyFile(string source, string target)
        {
            try
            {
                if (File.Exists(source))
                {
                    File.Copy(source, target, true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CompareFile(string file1, string file2, string ignores)
        {
            try
            {
                int retry = 5;
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    retry--;
                }
                while (retry > 0 && (File.Exists(file1) == false || File.Exists(file2) == false));

                while (DateTime.Now.Subtract(File.GetLastWriteTime(file1)).TotalSeconds < 5
                    || DateTime.Now.Subtract(File.GetLastWriteTime(file2)).TotalSeconds < 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                if (File.Exists(file1) && File.Exists(file2))
                {
                    string textFile1 = await File.ReadAllTextAsync(file1);
                    string textFile2 = await File.ReadAllTextAsync(file2);
                    if (string.IsNullOrWhiteSpace(ignores) == false)
                    {
                        foreach (var item in ignores.Split(",", StringSplitOptions.RemoveEmptyEntries).Reverse())
                        {
                            var split = item.Split("-", StringSplitOptions.RemoveEmptyEntries);
                            if (int.TryParse(split.First(), out int start)
                                && int.TryParse(split.Last(), out int end))
                            {
                                try
                                {
                                    textFile1 = textFile1.Remove(start, end - start);
                                    textFile2 = textFile2.Remove(start, end - start);
                                }
                                catch { }
                            }
                        }
                    }
                    if (textFile1 == textFile2)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool ChangeDateTime(string date, string time)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date) && string.IsNullOrWhiteSpace(time))
                {
                    EnableAutoTime();
                }
                else if (int.TryParse(date, out int intDate))
                {
                    DisableAutoTime();
                    RunCmd($"date {DateTime.Now.AddDays(intDate).ToShortDateString()}");
                    RunCmd($"time {time}");
                }
                return true;
            }
            catch
            {
                return false;
            }

            void RunCmd(string args)
            {
                var p = new ProcessStartInfo("cmd.exe", "/C " + args) { CreateNoWindow = true, UseShellExecute = false };
                Process.Start(p)?.WaitForExit();
            }

            void DisableAutoTime()
            {
                RunCmd("net stop w32time");
                RunCmd("sc config w32time start= disabled");
                using (var key = Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Services\W32Time\Parameters", writable: true))
                {
                    if (key != null) key.SetValue("Type", "NoSync", RegistryValueKind.String);
                }
            }

            void EnableAutoTime()
            {
                RunCmd("sc config w32time start= auto");
                RunCmd("net start w32time");
                RunCmd("w32tm /config /update");
                RunCmd("w32tm /resync /force");
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\W32Time\Parameters", writable: true))
                {
                    if (key != null) key.SetValue("Type", "NTP", RegistryValueKind.String);
                }
            }
        }

        private bool RestartService(string serviceName)
        {
            try
            {
                var service = new ServiceController(serviceName);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> WaitTime(string minutes)
        {
            try
            {
                if (int.TryParse(minutes, out int intMinutes))
                {
                    await Task.Delay(TimeSpan.FromSeconds(intMinutes));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<ActionTypes> ActionType()
        {
            return [ActionTypes.DeleteFile, 
                    ActionTypes.CompareFile, 
                    ActionTypes.ExistedFile, 
                    ActionTypes.CopyFile, 
                    ActionTypes.ShowMessageBox, 
                    ActionTypes.ChangeDateTime, 
                    ActionTypes.ResetDateTime, 
                    ActionTypes.RestartService, 
                    ActionTypes.WaitTime];
        }
    }

    public interface IStep
    {
        Task<bool> Action();

        List<ActionTypes> ActionType();

        AutomationElement? GetElementUI();
    }
}
