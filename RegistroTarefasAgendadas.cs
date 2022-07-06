using FluentScheduler;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CargaCotacoes
{
    public class RegistroTarefasAgendadas : Registry
    {
        public RegistroTarefasAgendadas()
        {
            Schedule<Job>()
                .NonReentrant()
                .ToRunOnceAt(DateTime.Now.AddSeconds(2))
                .AndEvery(30).Seconds();
        }
    }

    public class Job : IJob
    {
        private IConfigurationRoot _configuration;

        public Job()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");
            _configuration = builder.Build();
        }

        public void Execute()
        {
            Console.WriteLine("Conectando com a base de dados...");
            var cotacoesContext = new CotacoesContext(_configuration);
            var paginaCotacoes = new PaginaCotacoes();

            paginaCotacoes.ObterCotacoes(cotacoesContext);
            paginaCotacoes.ObterRendaFixa(cotacoesContext);

            paginaCotacoes.Fechar();
            cotacoesContext.Fechar();
        }
    }
}
