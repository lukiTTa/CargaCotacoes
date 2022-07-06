using System;
using FluentScheduler;

namespace CargaCotacoes
{
    class Program
    {
        static void Main(string[] args)
        {
            JobManager.Initialize(new RegistroTarefasAgendadas());

            Console.WriteLine("Digite 'stop' para interromper o programa");
            var stop = Console.ReadLine();

            if (stop == "stop")
                JobManager.StopAndBlock();
        }
    }
}