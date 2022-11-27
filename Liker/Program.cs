using CommandLine;
using Liker.Instagram;
using Liker.Logic;
using Liker.Persistence;
using Ninject;
using System.Diagnostics;

namespace Liker
{
    internal class Program
    {
        private const int DEFAULT_DELAY_SEED_MILLISECONDS = 2000;

        /// <summary>
        /// Docs on setting up options for CommandLineParser: https://github.com/commandlineparser/commandline
        /// </summary>
        class CommandLineOptions : IInstagramOptions, IProcessOptions
        {
            [Option('c', "csrf-token", Required = true, HelpText = "CSRF Bearer token to use on HTTP requests to Instagram.")]
            public string CSRFToken { get; set; } = string.Empty;

            [Option('s', "session-id", Required = true, HelpText = "Session ID to use on HTTP requests to Instagram.")]
            public string SessionID { get; set; } = string.Empty;

            [Option('a', "accounts", Required = true, HelpText = "Handles of the accounts whose followers should be processed.")]
            public IEnumerable<string> Accounts { get; set; } = Enumerable.Empty<string>();

            [Option('d', "delay", Required = false, Default = DEFAULT_DELAY_SEED_MILLISECONDS, HelpText = "The delay in milliseconds that should be used to slow down HTTP actions taken. Actual delays applied will be randomized +/- 50% of this value.")]
            public int DelaySeed { get; set; } = DEFAULT_DELAY_SEED_MILLISECONDS;

            [Option('r', "runtime", Required = false, Default = 0, HelpText = "The limit placed on the total running time in minutes. If not specified tool will run to completion.")]
            public int RuntimeLimit { get; set; } = 0;

            [Option('h', "hash-tags", Required = false, HelpText = "Hashtags to look for when choosing photos to like.")]
            public IEnumerable<string> HashTagsToLike { get; set; } = Enumerable.Empty<string>();
        }

        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsedAsync(async o =>
                   {
                       if (string.IsNullOrEmpty(o.CSRFToken)) throw new InvalidOperationException($"{nameof(o.CSRFToken)} is null or empty");
                       if (string.IsNullOrEmpty(o.SessionID)) throw new InvalidOperationException($"{nameof(o.SessionID)} is null or empty");

                       if (!o.HashTagsToLike.Any())
                       {
                           o.HashTagsToLike = new[] { "#warhammer40000", "#warhammer40k", "#warhammer", "#warhammercommunity", "#paintingwarhammer", "#wh40k", "#miniature", "#miniatures", "#miniaturepainting", "#painter", "#painting", "#paintingminiatures" };
                       }

                       // Initialize process
                       var services = SetupServiceBindings(o);
                       var process = services.Get<Logic.Process>();

                       // Run process
                       try
                       {
                           if (o.RuntimeLimit > 0)
                           {
                               var tokenSource = new CancellationTokenSource(new TimeSpan(0, o.RuntimeLimit, 0));
                               await process.Run(o.Accounts, tokenSource.Token);
                           }
                           else
                           {
                               await process.Run(o.Accounts);
                           }
                       }
                       catch (OperationCanceledException)
                       {
                           Console.WriteLine("Liking run terminated");
                       }
                       catch (Exception ex)
                       {
                           Console.Write($"Unhandled {ex.GetType()} {ex.Message}\n\n{ex.StackTrace}");
                           Environment.Exit(-1);
                       }
                   });
                   //.WithNotParsed(errors =>
                   //{
                   //    Console.WriteLine(errors);
                   //    Environment.Exit(-1);
                   //});
        }

        private static IKernel SetupServiceBindings(CommandLineOptions commandLineOptions)
        {
            var kernel = new StandardKernel();

            kernel.Bind<IInstagramOptions, IProcessOptions>().ToConstant(commandLineOptions);

            kernel.Bind<Logic.Process>().ToSelf();

            // Initialize database
            kernel.Bind<IDatabase>().To<Database>();

            // Initialize Instagram service
            kernel.Bind<IInstagramService>().To<InstagramService>();

            return kernel;
        }
    }
}