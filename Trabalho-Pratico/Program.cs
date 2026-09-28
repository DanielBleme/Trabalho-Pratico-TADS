using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Suporte para Controllers e Views (MVC)
        builder.Services.AddControllersWithViews();

        // Configuração do DbContext com SQL Server
        builder.Services.AddDbContext<ApplicationContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        // Pipeline HTTP
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}