using Microsoft.EntityFrameworkCore;
using Prometheus;
using RetroGameExchangeApi.Services;

namespace RetroGameExchangeApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ExchangeDbContext>(options =>
                options.UseSqlite("Data Source=/app/data/exchange.db"));

            // Register Kafka producer
            builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
            builder.Services.AddSingleton<IEmailNotificationProducer, EmailNotificationProducer>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ExchangeDbContext>();
                try
                {
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    // Another instance already created the tables - safe to ignore
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "Database already initialized by another instance");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseHttpMetrics();

            app.UseAuthorization();


            app.MapControllers();

            app.MapMetrics();

            app.Run();
        }
    }
}
