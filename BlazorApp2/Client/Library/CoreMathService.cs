using BlazorWorker.WorkerCore;
using System.Text.RegularExpressions;

namespace BlazorApp2.Client.Library
{
    /// <summary>
    /// This service runs in the worker.
    /// Uses hand-written seriaization of messages.
    /// </summary>
    public class CoreMathsService
    {
        public static readonly string EventsPi = $"Events.{nameof(MathsService.Pi)}";
        public static readonly string ResultMessage = $"Methods.{nameof(MathsService.EstimatePI)}.Result";

        private readonly MathsService mathsService;
        private readonly IWorkerMessageService messageService;

        public CoreMathsService(IWorkerMessageService messageService)
        {
            this.messageService = messageService;
            this.messageService.IncomingMessage += OnMessage;
            mathsService = new MathsService();
            mathsService.Pi += (s, progress) => messageService.PostMessageAsync($"{EventsPi}:{progress.Progress}");
        }

        private void OnMessage(object sender, string message)
        {
            if (message.StartsWith(nameof(mathsService.EstimatePI)))
            {
                var messageParams = message.Substring(nameof(mathsService.EstimatePI).Length).Trim();
                var rx = new Regex(@"\((?<arg>[^\)]+)\)");
                var arg0 = rx.Match(messageParams).Groups["arg"].Value.Trim();
                var iterations = int.Parse(arg0);
                mathsService.EstimatePI(iterations).ContinueWith(t =>
                    messageService.PostMessageAsync($"{ResultMessage}:{t.Result}"));
                return;
            }
        }
    }


    public class PiProgress
    {
        public int Progress { get; set; }
    }


    /// <summary>
    /// This service runs insinde the worker.
    /// </summary>
    public class MathsService
    {
        public event EventHandler<PiProgress> Pi;

        private IEnumerable<int> AlternatingSequence(int start = 0)
        {
            int i;
            bool flip;
            if (start == 0)
            {
                yield return 1;
                i = 1;
                flip = false;
            }
            else
            {
                i = (start * 2) - 1;
                flip = start % 2 == 0;
            }

            while (true) yield return ((flip = !flip) ? -1 : 1) * (i += 2);
        }

        public async Task<double> EstimatePI(int sumLength)
        {
            var lastReport = 0;
            await Task.Delay(100);
            return (4 * AlternatingSequence().Take(sumLength)
                .Select((x, i) =>
                {
                    // Keep reporting events down a bit, serialization is expensive!
                    var progressDelta = (Math.Abs(i - lastReport) / (double)sumLength) * 100;
                    if (progressDelta > 3 || i >= sumLength - 1)
                    {
                        lastReport = i;
                        Pi?.Invoke(this, new PiProgress() { Progress = i });
                    }
                    return x;
                })
                .Sum(x => 1.0 / x));
        }

        public double EstimatePISlice(int sumStart, int sumLength)
        {
            Console.WriteLine($"EstimatePISlice({sumStart},{sumLength})");
            var lastReport = 0;
            return AlternatingSequence(sumStart)
                .Take(sumLength)
                .Select((x, i) =>
                {

                    // Keep reporting events down a bit, serialization is expensive!
                    var progressDelta = (Math.Abs(i - lastReport) / (double)sumLength) * 100;
                    if (progressDelta > 3 || i >= sumLength - 1)
                    {
                        lastReport = i;
                        Pi?.Invoke(this, new PiProgress() { Progress = i });
                    }
                    return x;
                })
                .Sum(x => 1.0 / x);

        }
    }
}
