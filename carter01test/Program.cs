using Carter;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Topshelf;

class Program : ServiceControl
{
    private BackgroundWorker thread;
    private WebApplication? app;
    static void Main(string[] args)
    {
        var service = HostFactory.New(x =>
        {
            x.SetServiceName("Carter Test");
            x.SetDisplayName("Carter Test");
            x.SetDescription("Carter Test");
            x.Service<Program>();
        });
        service.Run();
    }

    public bool Start(HostControl hostControl)
    {

        thread = new BackgroundWorker();
        thread.DoWork += RunMain;
        thread.RunWorkerAsync();
        return true;
    }

    private void RunMain(object? sender, DoWorkEventArgs e)
    {
        var builder = WebApplication.CreateBuilder();

        string certThumbprint = builder.Configuration["Kestrel:Certificates:Default:Thumbprint"];

        // Configure Kestrel to use HTTPS with the certificate from the Local Machine store
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5001, listenOptions =>
            {
                listenOptions.UseHttps(httpsOptions =>
                {
                    httpsOptions.ServerCertificateSelector = (context, name) =>
                    {
                        // Find the certificate in the Local Machine store
                        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        store.Open(OpenFlags.ReadOnly);
                        var certs = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, validOnly: false);

                        // Use the first certificate found (ensure there’s only one or filter specifically)
                        return certs.Count > 0 ? certs[0] : null;
                    };
                });
            });
        });
        builder.Services.AddCarter();

        app = builder.Build();

        //app.MapGet("/", () => "Hello World!");

        app.MapCarter();

        app.Run();
    }

    public bool Stop(HostControl hostControl)
    {
        app?.StopAsync();
        return true;
    }
}