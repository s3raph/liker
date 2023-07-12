using CommandLine;
using Liker.Instagram;
using Liker.Logic;
using Liker.Persistence;
using Ninject;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Extensions.Interception.Request;
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
            /// <inheritdoc/>
            [Option('c', "csrf-token", Required = true, HelpText = "CSRF Bearer token to use on HTTP requests to Instagram.")]
            public string CSRFToken { get; set; } = string.Empty;

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            [Option('w', "ig-www-claim", Required = true, HelpText = "Value for the x-ig-www-claim header")]
            public string IGWWWClaim { get; set; } = string.Empty;

            /// <inheritdoc/>
            [Option('j', "ig-ajax", Required = true, HelpText = "Value for the x-instagram-ajax header")]
            public string IGAjax { get; set; } = string.Empty;
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
                           CancellationTokenSource tokenSource;
                           Task runTask;

                           if (options.RuntimeLimit > 0)
                           {
                               Console.WriteLine($"Launching run at {DateTime.Now} - runtime limit is {options.RuntimeLimit} minutes\n\nHit any key to quit\n");

                               tokenSource = new CancellationTokenSource(new TimeSpan(0, options.RuntimeLimit, 0));
                               runTask     = process.Run(options.Accounts, tokenSource.Token);
                           }
                           else
                           {
                               Console.WriteLine("Launching run - no runtime limit specified\n\nHit any key followed by Enter to quit\n");

                               tokenSource = new CancellationTokenSource();
                               runTask     = process.Run(options.Accounts, tokenSource.Token);
                           }

                           await Task.WhenAll(runTask, WatchForUserKeyPress(tokenSource, runTask));
                       }
                       catch (OperationCanceledException)
                       {
                           runTime.Stop();
                           Console.WriteLine($"Liking run terminated - ran for {runTime.Elapsed}");
                       }
                       catch (Exception ex)
                       {
                           runTime.Stop();
                           Console.Write($"Unhandled {ex.GetType()} {ex.Message}\n\n{ex.StackTrace}\n\nRan for {runTime.Elapsed}");
                           Environment.Exit(-1);
                       }
                   });

        private static async Task WatchForUserKeyPress(CancellationTokenSource tokenSource, Task runTask)
        {
            while (!tokenSource.IsCancellationRequested && !runTask.IsFaulted)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    tokenSource.Cancel();
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private static IKernel SetupServiceBindings(CommandLineOptions commandLineOptions)
        {
            var kernel = new StandardKernel();

            kernel.Bind<IInstagramOptions, IProcessOptions>().ToConstant(commandLineOptions);

            kernel.Bind<Logic.Process>().ToSelf();

            // Initialize database
            kernel.Bind<IDatabase>().To<Database>();

            // Initialize Instagram service
            kernel.Bind<IInstagramService>().To<InstagramService>()
                .Intercept().With<RetryInterceptor>();

            return kernel;
        }
    }

    public class RetryInterceptor : IInterceptor
    {
        private const int TIMES_TO_RETRY = 1;

        private readonly TaskScheduler Scheduler;

        public RetryInterceptor()
        {
            Scheduler = TaskScheduler.Current;
        }

        /// <summary>
        /// Constructor for injecting custom task scheduler for unit testing
        /// </summary>
        /// <param name="scheduler"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal RetryInterceptor(TaskScheduler scheduler)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public void Intercept(IInvocation invocation)
        {
            //AsyncInterceptor
            //var exceptions = new List<Exception>();

            //invocation.Clone();

            //for (int i = 0; i <= TIMES_TO_RETRY; i++)
            //{
            //    try
            //    {
            //        invocation.Proceed();
            //        return;
            //    }
            //    catch (InstagramRESTException ex) when (ex.StatusCode == 0)
            //    {
            //        exceptions.Add(ex);

            //        if (i < TIMES_TO_RETRY)
            //        {
            //            Console.WriteLine($"Warning: Handled {nameof(InstagramRESTException)} (HTTP status {ex.StatusCode}) - {ex.Message}");
            //            //await Utility.DelayByRandom(5000);
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //}

            //IInvocation invocationClone = invocation.Clone();
            //invocation.ReturnValue = Task.Factory.StartNew(delegate
            //{
            //    //BeforeInvoke(invocation);
            //}).ContinueWith(delegate
            //{
            //    invocationClone.Proceed();
            //    return invocationClone.ReturnValue as Task;
            //}).Unwrap()
            //    .ContinueWith(delegate (Task t)
            //    {
            //        //AfterInvoke(invocation);
            //        //AfterInvoke(invocation, t);
            //    });

            int i = 0;

            Func<Task> CloneAndInvoke = () =>
            {
                i++;
                IInvocation invocationClone = invocation.Clone();
                invocationClone.Proceed();
                return invocationClone.ReturnValue as Task;
            };

            Task returnVal = Task.Factory.StartNew(CloneAndInvoke, CancellationToken.None, TaskCreationOptions.None, Scheduler)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted && i <= TIMES_TO_RETRY && t?.Exception.Flatten().InnerExceptions.First() is InstagramRESTException ex && ex.StatusCode == 0)
                    {
                        Console.WriteLine($"Warning: Handled {nameof(InstagramRESTException)} (HTTP status {ex.StatusCode}) - {ex.Message}");

                        return Utility.DelayByRandom(5000)
                            .ContinueWith(t => CloneAndInvoke(), Scheduler);
                    }
                    else
                    {
                        return t;
                    }
                }, Scheduler);

            invocation.ReturnValue = returnVal;
        }

        //private static Func<Task> CloneAndInvoke(IInvocation invocation)
        //{
        //    return () =>
        //    {
        //        IInvocation invocationClone = invocation.Clone();
        //        invocationClone.Proceed();
        //        return invocationClone.ReturnValue as Task;
        //    };
        //}
    }

    public class RetryAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request) => request.Context.Kernel.Get<RetryInterceptor>();
    }
}