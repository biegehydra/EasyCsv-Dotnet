using EasyCsv.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCsv.DependencyInjection
{
    public static class EasyCsvCoreDependencyInjectionExtensions
    {
        public static void AddEasyCsvServiceFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICsvServiceFactory, EasyCsvServiceFactory>();
        }
    }
}