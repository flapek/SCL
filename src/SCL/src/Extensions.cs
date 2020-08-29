using System;
using System.Linq;
using System.Threading.Tasks;
using SCL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Figgle;

namespace SCL
{
    public static class Extensions
    {
        private const string SectionName = "app";

        public static ISCLBuilder AddSCL(this IServiceCollection services, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var builder = SCLBuilder.Create(services);
            var options = builder.GetOptions<AppOptions>(sectionName);
            builder.Services.AddMemoryCache();
            services.AddSingleton(options);
            services.AddSingleton<IServiceId, ServiceId>();
            if (!options.DisplayBanner || string.IsNullOrWhiteSpace(options.Name))
            {
                return builder;
            }

            var version = options.DisplayVersion ? $" {options.Version}" : string.Empty;
            Console.WriteLine(FiggleFonts.Doom.Render($"{options.Name}{version}"));

            return builder;
        }

        public static IApplicationBuilder UseSCL(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
                Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
            }

            return app;
        }

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
            where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(sectionName).Bind(model);
            return model;
        }

        public static TModel GetOptions<TModel>(this ISCLBuilder builder, string settingsSectionName)
            where TModel : new()
        {
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            return configuration.GetOptions<TModel>(settingsSectionName);
        }

        [Obsolete("Call 'UseSCL()' instead, this method might be removed in the future.")]
        public static IApplicationBuilder UseInitializers(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
                Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
            }

            return app;
        }

        public static string Underscore(this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToLowerInvariant();
    }
}