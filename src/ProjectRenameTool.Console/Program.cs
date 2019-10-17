using System;
using System.Diagnostics;
using System.IO;
using Awesome.Net.WritableOptions;
using Awesome.Net.WritableOptions.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectRenameTool.Console.Replacing;
using static System.Console;

namespace ProjectRenameTool.Console
{
    class Program
    {
        public static readonly string BasePath = Directory.GetCurrentDirectory();

        static void Main()
        {
            WindowWidth = 120;
            BufferHeight = 1000;

            ForegroundColor = ConsoleColor.Green;
            WriteLine("该程序通过应用.gitignore文件的忽略规则来选择性查找并替换文件（夹）的名称和内容。");
            WriteLine("关于 .gitignore 请参阅：https://git-scm.com/docs/gitignore。");
            ResetColor();

            var config = BuildConfiguration();
            var services = new ServiceCollection();
            ConfigureServices(services, config);

            #region 开始处理

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"{Environment.NewLine}发生以下错误：");
                WriteLine(((Exception)e.ExceptionObject).Message);
                ResetColor();

                Write($"{Environment.NewLine}任意键退出...");
                ReadKey();
                Environment.Exit(-1);
            };

            var watch = Stopwatch.StartNew();

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IWritableOptions<ReplacementOptions>>().Value;
            WriteLine($@"ReplacementOptions：{Environment.NewLine}{options}{Environment.NewLine}");

            WriteLine($"{Environment.NewLine}是否继续？ Y/任意键退出");
            if (ReadLine().Trim().ToLower() == "y")
            {
                var renamer = serviceProvider.GetService<Renamer>();
                renamer.Run();

                watch.Stop();
                WriteLine($"{Environment.NewLine}已完成，耗时 {watch.Elapsed.TotalSeconds} 秒。");
                ReadKey();
            }

            #endregion
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            const string appConfigJsonName = "appsettings.json";

            var appConfigJsonPath = Path.Combine(BasePath, appConfigJsonName);

            if (!File.Exists(appConfigJsonPath))
            {
                JsonFileHelper.AddOrUpdateSection(appConfigJsonPath, nameof(ReplacementOptions), new ReplacementOptions());
            }
            else
            {
                WriteLine($"{Environment.NewLine}是否重新生成 appsettings.json 文件？ Y/任意键");
                if (ReadLine().Trim().ToLower() == "y")
                {
                    JsonFileHelper.AddOrUpdateSection(appConfigJsonPath, nameof(ReplacementOptions), new ReplacementOptions());
                }
            }

            ForegroundColor = ConsoleColor.Yellow;
            WriteLine($"请先配置程序目录下的 appsettings.json 文件。");

            ResetColor();
            WriteLine($"{Environment.NewLine}配置完成？任意键继续...");
            ReadKey();

            var builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile(appConfigJsonName, false, true);

            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton(config);
            services.AddOptions();

            services.ConfigureWritableOptions<ReplacementOptions>(config.GetSection(nameof(ReplacementOptions)));

            services.AddTransient<Renamer>();
        }
    }
}