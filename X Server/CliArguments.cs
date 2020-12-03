using Fclp;

namespace X_Server
{
    public class CliArguments
    {
        public string Config { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class CliArgumentsBuilder
    {
        private FluentCommandLineParser<CliArguments> _builder;

        internal CliArgumentsBuilder()
        {
            _builder = new FluentCommandLineParser<CliArguments>();

            _builder.Setup(x => x.Config)
                    .As('c', "config");
        }


        internal ICommandLineParserResult Parse(string[] args)
        {
            return _builder.Parse(args);
        }


        internal CliArguments Object => _builder.Object;
    }
}
