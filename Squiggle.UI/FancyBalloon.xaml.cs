using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;

namespace StackOverflowClient
{
  /// <summary>
  /// Interaction logic for FancyBalloon.xaml
  /// </summary>
  public partial class FancyBalloon : UserControl
  {      
      bool isClosing = false;

    #region BalloonText dependency property

    /// <summary>
    /// Description
    /// </summary>
    public static readonly DependencyProperty BalloonTextProperty =
        DependencyProperty.Register("BalloonText",
                                    typeof (string),
                                    typeof (FancyBalloon),
                                    new FrameworkPropertyMetadata(""));

    /// <summary>
    /// A property wrapper for the <see cref="BalloonTextProperty"/>
    /// dependency property:<br/>
    /// Description
    /// </summary>
    public string BalloonText
    {
      get { return (string) GetValue(BalloonTextProperty); }
      set { SetValue(BalloonTextProperty, value); }
    }

    #endregion

    Timer timer;
    public FancyBalloon()
    {
      InitializeComponent();
      TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
    }

    public FancyBalloon(int timeout):this()
    {
        timer = new Timer(timeout);
        timer.Elapsed += (sender, e) => Close();
        timer.Start();
    }
      
    /// <summary>
    /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
    /// and setting the "Handled" property to true, we suppress the popup
    /// from being closed in order to display the fade-out animation.
    /// </summary>
    private void OnBalloonClosing(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      isClosing = true;
    }


    /// <summary>
    /// Resolves the <see cref="TaskbarIcon"/> that displayed
    /// the balloon and requests a close action.
    /// </summary>
    private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        Close();
    }

    private void Close()
    {
        timer.Stop();

        if (Application.Current == null)
            return;

        if (Application.Current.Dispatcher.CheckAccess())
        {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }
        else
            Application.Current.Dispatcher.Invoke(new Action(Close));        
    }

    /// <summary>
    /// If the users hovers over the balloon, we don't close it.
    /// </summary>
    private void grid_MouseEnter(object sender, MouseEventArgs e)
    {
      //if we're already running the fade-out animation, do not interrupt anymore
      //(makes things too complicated for the sample)
      if (isClosing) return;

      //the tray icon assigned this attached property to simplify access
      TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
      //taskbarIcon.ResetBalloonCloseTimer();
    }


    /// <summary>
    /// Closes the popup once the fade-out animation completed.
    /// The animation was triggered in XAML through the attached
    /// BalloonClosing event.
    /// </summary>
    private void OnFadeOutCompleted(object sender, EventArgs e)
    {
          Popup pp = (Popup)Parent;
          pp.IsOpen = false;
    }

    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
            Close();
        
    }

    private void Baloon_MouseEnter(object sender, MouseEventArgs e)
    {
    }

    private void OnFadeInCompleted(object sender, EventArgs e)
    {
        
    }

    private void me_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Close();
    }
  }
}
