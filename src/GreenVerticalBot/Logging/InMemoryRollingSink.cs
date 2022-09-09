using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting;

namespace GreenVerticalBot.Logging
{
    internal class InMemorySink : ILogEventSink
    {
        readonly ITextFormatter textFormatter = 
            new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss} [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}");

        public static RollingList<string> Events { get; } = new RollingList<string>(500);

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }
            var renderSpace = new StringWriter();
            this.textFormatter.Format(logEvent, renderSpace);
            InMemorySink.Events.Add(renderSpace.ToString());
        }
    }
}
