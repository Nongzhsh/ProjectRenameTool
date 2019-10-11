using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
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
            var config = BuildConfiguration();
            var services = new ServiceCollection();
            ConfigureServices(services, config);

            #region 开始处理

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                WriteLine($"{Environment.NewLine}发生以下错误：");
                var message = $"{Environment.NewLine}{DateTime.Now} [Error]:{Environment.NewLine}{e.ExceptionObject}";
                WriteLine(message);
                WriteLine($"{Environment.NewLine}任意键退出...");
                ReadKey();
                Environment.Exit(-1);
            };

            var watch = Stopwatch.StartNew();

            var serviceProvider = services.BuildServiceProvider();
            var renamer = serviceProvider.GetService<Renamer>();
            renamer.Run();

            watch.Stop();

            WriteLine($"{Environment.NewLine}已完成，耗时 {watch.Elapsed.TotalSeconds} 秒。");

            #endregion
            ReadKey();
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            const string appConfigJsonName = "appsettings.json";

            var appConfigJsonPath = Path.Combine(BasePath, appConfigJsonName);

            if (!File.Exists(appConfigJsonPath))
            {
                JsonFileHelper.AddOrUpdateSection(appConfigJsonPath, nameof(ReplacementOptions), new ReplacementOptions());
            }

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