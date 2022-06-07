using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecurringScheduler
{
    public class RecurringTaskScheduler : ITaskScheduler
    {
        private readonly Action task;
        private readonly Action cancelledTask;
        private readonly int interval;

        private Timer timer;
        private Thread taskThread;
        private AutoResetEvent runTaskEvent;
        private volatile bool taskRunning = false;

        public RecurringTaskScheduler(Action task, Action cancelledTask = null, int interval = 1000)
        {
            if (interval < 1000)
            {
                throw new ArgumentException("Interval must be more then 1000ms", nameof(interval));
            }

            this.task = task ?? throw new ArgumentException("Task is null.", nameof(task));
            this.cancelledTask = cancelledTask;
            this.interval = interval;
            
            runTaskEvent = new AutoResetEvent(false);
        }

        private void TaskThreadStart(object obj)
        {
            try
            {
                while (true)
                {
                    runTaskEvent.WaitOne();
                    Console.WriteLine(FormattableString.Invariant($"Running task {Thread.CurrentThread.ManagedThreadId} at {DateTime.Now:dd-MM-yyy HH:mm:ss.fff}"));

                    if (taskRunning)
                    {
                        throw new InvalidOperationException(FormattableString.Invariant($"Invalid task {Thread.CurrentThread.ManagedThreadId} state!"));
                    }
                    
                    taskRunning = true;
                    try
                    {
                        task.Invoke();
                    }
                    finally
                    {
                        taskRunning = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex is ThreadInterruptedException
                    ? FormattableString.Invariant($"Task {Thread.CurrentThread.ManagedThreadId} is interrupted.")
                    : FormattableString.Invariant(
                        $"Error occurred while running task {Thread.CurrentThread.ManagedThreadId}. {ex}"));
            }
            finally
            {
                taskRunning = false;
            }
        }

        private void TimerCallback(object state)
        {
            if (taskRunning || !taskThread.IsAlive)
            {
                if (taskThread.IsAlive)
                {
                    Console.WriteLine(FormattableString.Invariant(
                        $"Task {taskThread.ManagedThreadId} is still running. Interrupting it since it took more then {interval}ms."));
                    taskThread.Interrupt();
                    taskThread.Join();
                }
                else
                {
                    Console.WriteLine(FormattableString.Invariant(
                        $"Task {taskThread.ManagedThreadId} is dead due to exception. Restart new thread."));
                }

                taskThread = new Thread(TaskThreadStart) { IsBackground = true };
                taskThread.Start();

                try
                {
                    this.cancelledTask?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(FormattableString.Invariant(
                        $"Error occurred while running cancelled task. {ex}"));
                }
            }
            runTaskEvent.Set();
        }

        public void Start()
        {
            if (timer != null || 
                taskThread != null)
            {
                throw new InvalidOperationException("Scheduler has already started!");
            }

            taskThread = new Thread(TaskThreadStart) { IsBackground = true };
            taskThread.Start();
            timer = new Timer(TimerCallback, null, 0, interval);
        }

        public void Stop()
        {
            if (timer == null || 
                taskThread == null)
            {
                throw new InvalidOperationException("Scheduler is not started");
            }
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;
            taskThread.Interrupt();
            taskThread = null;
        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                timer = null;
            }

            if (taskThread != null)
            {
                taskThread.Interrupt();
                taskThread = null;
            }

            if (runTaskEvent != null)
            {
                runTaskEvent.Dispose();
                runTaskEvent = null;
            }
        }
    }
}
