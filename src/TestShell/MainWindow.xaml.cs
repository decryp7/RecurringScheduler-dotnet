using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RecurringScheduler;

namespace TestShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ITaskScheduler recurringScheduler;
        private readonly APISimulator simulator = new APISimulator();
        private object lockObject = new object();

        public MainWindow()
        {
            InitializeComponent();

            NativeMethods.AllocConsole();
            
            this.Closing += OnClosing;

            recurringScheduler = new RecurringTaskScheduler(() =>
            {
                lock (lockObject)
                {
                    //1. Get the result
                    DateTime result = simulator.MethodA();
                    //2. Write to cache
                    //3. Fire data change
                    Console.WriteLine(FormattableString.Invariant(
                        $"{nameof(APISimulator.MethodA)} return {result:dd-MM-yyy HH:mm:ss.fff}. Task {Thread.CurrentThread.ManagedThreadId}."));
                }
            }, () =>
            {
                //1. Set ******* to cache
                //2. Fire data change
                Console.WriteLine(FormattableString.Invariant($"Task is cancelled because it took too long."));
            });
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            NativeMethods.FreeConsole();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            recurringScheduler.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            recurringScheduler.Stop();
        }
    }
}
