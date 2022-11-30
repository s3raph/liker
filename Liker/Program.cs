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
        private const int DEFAULT_DELAY_SEED_MILLISECONDS = 1500;

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

            /// <inheritdoc/>
            public int MaxAllowedUserProfileInfoCalls => 400;
        }

        static Task Main(string[] args) =>
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsedAsync(async options =>
                   {
                       if (string.IsNullOrEmpty(options.CSRFToken)) throw new InvalidOperationException($"{nameof(options.CSRFToken)} is null or empty");
                       if (string.IsNullOrEmpty(options.SessionID)) throw new InvalidOperationException($"{nameof(options.SessionID)} is null or empty");

                       if (!options.HashTagsToLike.Any())
                       {
                           options.HashTagsToLike = new[] { "#ageofsigmar", "#aos","#warhammer40000", "#warhammer40k", "#warhammer", "#warhammercommunity", "#paintingwarhammer", "#wh40k", "#miniature", "#miniatures", "#miniaturepainting", "#painter", "#painting", "#paintingminiatures" };
                       }

                       // Initialize process
                       var services = SetupServiceBindings(options);
                       var process  = services.Get<Logic.Process>();

                       var runTime = Stopwatch.StartNew();

                       // Run process
                       try
                       {
                           if (options.RuntimeLimit > 0)
                           {
                               Console.WriteLine($"Launching run at {DateTime.Now} - runtime limit is {options.RuntimeLimit} minutes");
                               var tokenSource = new CancellationTokenSource(new TimeSpan(0, options.RuntimeLimit, 0));
                               await process.Run(options.Accounts, tokenSource.Token);
                           }
                           else
                           {
                               Console.WriteLine("Launching run - no runtime limit specified");
                               await process.Run(options.Accounts);
                           }
                       }
                       catch (OperationCanceledException)
                       {
                           runTime.Stop();
                           Console.WriteLine($"Liking limit hit - ran for {runTime.Elapsed}");
                       }
                       catch (Exception ex)
                       {
                           runTime.Stop();
                           Console.Write($"Unhandled {ex.GetType()} {ex.Message}\n\n{ex.StackTrace}\n\nRan for {runTime.Elapsed}");
                           Environment.Exit(-1);
                       }
                   });

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