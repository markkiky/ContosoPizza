using Hangfire;
using Hangfire.Processing;

namespace ContosoPizza.Services
{
    public class HangfireTest
    {
        private readonly IBackgroundJobClient backgroundJobs;
        public HangfireTest(IBackgroundJobClient background)
        {
          backgroundJobs = background;
        }

        public void Print()
        {
            backgroundJobs.Enqueue(() => Console.WriteLine("Hello World from Hangfire"));
        }
    }
}
