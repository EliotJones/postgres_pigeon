using System;

namespace PostgresPigeon.ConsoleHarness
{
    using System.Threading.Tasks;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var connectionString = args[0];

            Console.WriteLine($"Running a select command against database: {connectionString}.");

            using (var connection = new PostgresConnection(connectionString))
            {
                await connection.Open();

                var results = await connection.ExecuteCommand("SELECT * FROM information_schema.tables where table_schema = 'information_schema';");

                Console.WriteLine(results);
            }

            Console.WriteLine("Complete with success.");

            Console.ReadKey();
        }
    }
}
