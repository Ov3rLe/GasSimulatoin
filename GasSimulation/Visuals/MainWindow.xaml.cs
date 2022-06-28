using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using System.Timers;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace GasSimulation.Visuals
{
    public partial class MainWindow : Window
    {
        private CanvasField CanvasField;

        private Timer CanvasUpdateTimer;

        private int CanvasUpdateInterval = 16;

        private bool CanChangeSettings = true;

        public MainWindow()
        {
            InitializeComponent();

            // Container is initialised inside CanvasField when the size is known
            CanvasField = new CanvasField();
            CanvasContainer.Child = CanvasField;

            CanvasField.ParticleInfoUpdateEvent += 
                (e) => ParticleInfoBlock.Text = e.Text;

            CanvasField.PerformanceInfoUpdateEvent += 
                (e) => PerformanceInfoBlock.Text = e.Text;

            CanvasUpdateTimer = new Timer(CanvasUpdateInterval) { AutoReset = true };
            CanvasUpdateTimer.Elapsed += (o, e) 
                => CanvasField.Dispatcher.Invoke(CanvasField.Update);
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            CanvasField.StartContainerUpdate();
            CanvasUpdateTimer.Start();
            SwitchButtonState();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            CanvasField.StopContainerUpdate();
            CanvasUpdateTimer.Stop();
            SwitchButtonState();
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            CanvasField.ResetField();
        }

        private void Step(object sender, RoutedEventArgs e)
        {
            CanvasField.UpdateFrame();
        }

        private void SwitchButtonState()
        {
            StartButton.IsEnabled = !StartButton.IsEnabled;
            ResetButton.IsEnabled = !ResetButton.IsEnabled;
            StopButton.IsEnabled = !StopButton.IsEnabled;
            StepButton.IsEnabled = !StepButton.IsEnabled;
            CanChangeSettings = !CanChangeSettings;
        }

        private void InfoCheckChanged(object sender, RoutedEventArgs e)
        {
            BrushConverter conv = new BrushConverter();
            Brush checkedColor = (Brush)conv.ConvertFromString("#FF75A5D4");
            Brush notCheckedColor = (Brush)conv.ConvertFromString("#FFA8A8A8");

            TextBlock t = sender as TextBlock;
            if (t.Name == VelocityCheck.Name)
            {
                CanvasField.ShowVelocity = !CanvasField.ShowVelocity;
                t.Foreground = CanvasField.ShowVelocity ? checkedColor : notCheckedColor;
            }

            if (t.Name == TrajectoryCheck.Name)
            {
                CanvasField.ShowTrajectory = !CanvasField.ShowTrajectory;
                t.Foreground = CanvasField.ShowTrajectory ? checkedColor : notCheckedColor;
            }

            if (t.Name == HideCheck.Name)
            {
                CanvasField.HideNotTracked = !CanvasField.HideNotTracked;
                t.Foreground = CanvasField.HideNotTracked ? checkedColor : notCheckedColor;
            }

            if (t.Name == BigParticleCheck.Name)
            {
                CanvasField.BigParticle = !CanvasField.BigParticle;
                t.Foreground = CanvasField.BigParticle ? checkedColor : notCheckedColor;
            }

            CanvasField.Refresh();
        }

        private void SettingsChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox t) || CanvasField == null)
                return;

            if (string.IsNullOrWhiteSpace(t.Text))
            {
                t.Text = "0";
                t.CaretIndex = 1;
            }

            if (t.Name == AmountBox.Name)
            {
                if (Int32.Parse(t.Text) > 500)
                    t.Text = "500";
                CanvasField.ParticleAmount = Int32.Parse(t.Text);
            }

            else if (t.Name == SpeedBox.Name)
            {
                if (Int32.Parse(t.Text) > 10)
                    t.Text = "10";
                CanvasField.MaxSpeed = Int32.Parse(t.Text);
            }

            else if (t.Name == SizesBox.Name)
            {
                if (Int32.Parse(t.Text) > 5)
                    t.Text = "5";
                CanvasField.SizeScatter = Int32.Parse(t.Text);
            }

            else if (t.Name == SizeMultBox.Name)
            {
                if (Int32.Parse(t.Text) > 25)
                    t.Text = "25";
                if (Int32.Parse(t.Text) < 1)
                    t.Text = "1";
                CanvasField.SizeMultiplyer = Int32.Parse(t.Text);
            }
        }

        private void CheckSettingInput(object sender, TextCompositionEventArgs e)
        {
            Regex numOnly = new Regex("[0-9]");
            if (!numOnly.IsMatch(e.Text) || !CanChangeSettings)
                e.Handled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CanvasField.StopContainerUpdate();
            CanvasUpdateTimer.Stop();
        }
    }
}
