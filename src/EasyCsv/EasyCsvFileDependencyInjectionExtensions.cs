using EasyCsv.Core;
using EasyCsv.Files;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCsv.DependencyInjection
{
    public static class EasyCsvCoreDependencyInjectionExtensions
    {
        public static void AddEasyCsvServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICsvServiceFactory, EasyCsvServiceFactory>();
            serviceCollection.AddSingleton<IEasyFileCsvServiceFactory, EasyFileCsvServiceFactory>();

        }
    }
}