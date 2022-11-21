using CommandLine;

namespace Liker
{
    internal class Program
    {
        private const int DEFAULT_DELAY_SEED_MILLISECONDS = 2000;

        class CommandLineOptions
        {
            [Option('t', "token", Required = true, HelpText = "Bearer token to use on HTTP requests to Instagram.")]
            public string BearerToken { get; set; }

            [Option('a', "accounts", Required = true, HelpText = "Handles of the accounts whose followers should be processed.")]
            public IEnumerable<string> Accounts { get; set; }

            [Option('d', "delay", Required = false, Default = DEFAULT_DELAY_SEED_MILLISECONDS, HelpText = "The delay in milliseconds that should be used to slow down HTTP actions taken. Actual delays applied will be randomized +/- 50% of this value.")]
            public int DelaySeed { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(o =>
                   {
                       // Initialize database
                       // Initialize Instagram service

                       // foreach account
                       //   foreach follower page
                       //     foreach follower
                       //       insert into database
                       //       if !private AND !blocked AND !seenBefore
                       //         retrieve first NUM (default 9) follower posts (and thumbnails)
                       //         if can find posts with known hashtags
                       //           Like random selection of <5 posts
                       //         else
                       //           find any photos that don't contain a face
                       //           Like one random post
                   })
                   .WithNotParsed(errors =>
                   {
                       Console.WriteLine(errors);
                       Environment.Exit(-1);
                   });
        }
    }
}