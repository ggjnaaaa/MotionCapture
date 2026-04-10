using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotionCapture.Infrastructure.Camera.Extensions;
using MotionCapture.Infrastructure.Grpc.Extensions;
using MotionCapture.Services;
using MotionCapture.ViewModels;
using MotionCapture.Views;
using System.Windows;

namespace MotionCapture
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        public IServiceProvider Services => _host.Services;

        protected override void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddGrpcInfrastructure(ctx.Configuration);
                    services.AddCameraInfrastructure();

                    services.AddTransient<CameraViewModel>();
                    services.AddTransient<TopMenuViewModel>();
                    services.AddTransient<OverlayViewModel>();
                    services.AddTransient<MainViewModel>();

                    services.AddSingleton<ApplicationState>();
                    services.AddSingleton<EmguSkeletonDrawingService>();
                    services.AddSingleton<ProcessingOrchestrator>();
                })
                .Build();

            _host.Start();

            var mainWindow = new MainWindow(
                Services.GetRequiredService<MainViewModel>());

            mainWindow.Show();
        }
    }

}
