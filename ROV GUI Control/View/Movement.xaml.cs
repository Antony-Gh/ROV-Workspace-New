using System;
using System.Windows;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.View
{
    /// <summary>
    /// Interaction logic for Movement.xaml
    /// </summary>
    public partial class Movement : UserControl, INotifyPropertyChanged, IDisposable
    {
        public static readonly DependencyProperty DirValueProperty = DependencyProperty.Register("MoveDir", typeof(int), typeof(Movement),
          new PropertyMetadata(0, OnDirValuePropertyChanged));
        public int MoveDir 
        {
            get { return (int)GetValue(DirValueProperty); }
            set { SetValue(DirValueProperty, value); }
        }
        private static void OnDirValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Movement f = d as Movement;
            f.OnDirValueChanged(e);
        }
        public virtual void OnDirValueChanged(DependencyPropertyChangedEventArgs e)
        {
            MoveBlink(MoveDir);
        }
        private readonly System.Timers.Timer BlinkTimer;

        private Path _poly;
        public Path POLY
        {
            get => _poly;
            set
            {
                _poly = value;
                OnPropertyChanged(nameof(POLY));
            }
        }
        private bool _isblink = false;
        public bool IsBlink
        {
            get => _isblink;
            set
            {
                _isblink = value;
                OnPropertyChanged(nameof(IsBlink));
            }
        }
        public Movement()
        {
            InitializeComponent();
            BlinkTimer = new System.Timers.Timer(200);
            BlinkTimer.Elapsed += Timer_Tick;
            BlinkTimer.AutoReset = true;
            HiddenAll();
        }
        private void HiddenAll()
        {
            Forward.Visibility = Visibility.Hidden;
            Backward.Visibility = Visibility.Hidden;
            Right.Visibility = Visibility.Hidden;
            Left.Visibility = Visibility.Hidden;
            ForwardR.Visibility = Visibility.Hidden;
            ForwardL.Visibility = Visibility.Hidden;
            BackwardR.Visibility = Visibility.Hidden;
            BackwardL.Visibility = Visibility.Hidden;
            RotateR.Visibility = Visibility.Hidden;
            RotateL.Visibility = Visibility.Hidden;
            RRshadow.Visibility = Visibility.Hidden;
            RLshadow.Visibility = Visibility.Hidden;
            Up.Visibility = Visibility.Hidden;
            Down.Visibility = Visibility.Hidden;
            Ushadow.Visibility = Visibility.Hidden;
            Dshadow.Visibility = Visibility.Hidden;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (POLY != null)
                {
                    POLY.Visibility = IsBlink ? Visibility.Visible : Visibility.Hidden;
                    IsBlink = !IsBlink;
                }
            });
        }
        private void MoveBlink(int i)
        {
            switch(i)
            {
                case 0:
                    Blink(Forward);
                    break;
                case 1:
                    Blink(Backward);
                    break;
                case 2:
                    Blink(Right);
                    break;
                case 3:
                    Blink(Left);
                    break;
                case 4:
                    Blink(ForwardR);
                    break;
                case 5:
                    Blink(ForwardL);
                    break;
                case 6:
                    Blink(BackwardR);
                    break;
                case 7:
                    Blink(BackwardL);
                    break;
                case 8:
                    Blink(RotateR);
                    break;
                case 9:
                    Blink(RotateL);
                    break;
                case 10:
                    Blink(Up);
                    break;
                case 11:
                    Blink(Down);
                    break;
                default:
                    BlinkTimer.Stop();
                    HiddenAll();
                    break;
            }
        }
        private void Blink(Path p)
        {
            BlinkTimer.Stop();
            HiddenAll();
            POLY = p;
            if (POLY != null)
            {
                if (POLY == RotateR)
                {
                    RRshadow.Visibility = Visibility.Visible;
                }
                if (POLY == RotateL)
                {
                    RLshadow.Visibility = Visibility.Visible;
                }
                if (POLY == Up)
                {
                    Ushadow.Visibility = Visibility.Visible;
                }
                if (POLY == Down)
                {
                    Dshadow.Visibility = Visibility.Visible;
                }
                IsBlink = true;
                BlinkTimer.Start();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void Dispose()
        {
            BlinkTimer?.Stop();
            BlinkTimer?.Dispose();
        }
    }
}
