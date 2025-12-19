using FTdx101MP_WebApp.Services;

namespace FTdx101MP_WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Register Settings Service
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            // Register CAT services
            builder.Services.AddSingleton<ICatClient, SerialPortCatClient>();
            builder.Services.AddSingleton<IRigStateService, RigStateService>();

            // Register Background Service
            builder.Services.AddHostedService<CatPollingService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}