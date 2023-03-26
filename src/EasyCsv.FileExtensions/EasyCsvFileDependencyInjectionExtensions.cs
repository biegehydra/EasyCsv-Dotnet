using EasyCsv.Files;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EasyCsv")]
namespace EasyCsv.DependencyInjection
{
    public static class EasyCsvFileDependencyInjectionExtensions
    {
        public static void AddEasyFileCsvServiceFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEasyFileCsvServiceFactory, EasyFileCsvServiceFactory>();
        }
    }
}