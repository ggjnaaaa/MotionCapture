using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotionCapture.Core.Interfaces;
using MotionCapture.Infrastructure.Camera.Extensions;
using MotionCapture.Infrastructure.GrpcClient.Extensions;
using MotionCapture.Infrastructure.GrpcServer.Extensions;
using MotionCapture.Infrastructure.GrpcServer.Services;
using MotionCapture.Processing.Extensions;
using MotionCapture.Processing.Services;
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

        protected async override void OnStartup(StartupEventArgs e)
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
                    services.AddProcessing();
                    services.AddGrpcServer();

                    services.AddTransient<CameraViewModel>();
                    services.AddTransient<TopMenuViewModel>();
                    services.AddTransient<OverlayViewModel>();
                    services.AddTransient<MainViewModel>();

                    services.AddSingleton<ApplicationState>();
                    services.AddSingleton<EmguSkeletonDrawingService>();
                    services.AddSingleton<ProcessingOrchestrator>();
                })
                .ConfigureWebHostDefaults(web =>
                {
                    web.ConfigureServices(services =>
                    {
                        services.AddGrpc();
                        services.AddSingleton<ISkeletonState, SkeletonState>();
                    });

                    web.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(5001, o =>
                        {
                            o.Protocols = HttpProtocols.Http2;
                        });
                    });

                    web.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGrpcService<SkeletonGrpcService>();
                        });
                    });
                })
                .Build();

            await _host.StartAsync();

            var mainWindow = new MainWindow(
                Services.GetRequiredService<MainViewModel>());

            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }

}
