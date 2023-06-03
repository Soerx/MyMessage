using Client.Controls;
using Client.Models;
using Client.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace Client.Views;
/// <summary>
/// Логика взаимодействия для ChatView.xaml
/// </summary>
public partial class ChatView : UserControl
{
    private bool _isautoScrollEnabled = true;

    public ChatView()
    {
        InitializeComponent();
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer is false)
            return;

        if (e.ExtentHeightChange == 0)
        {
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _isautoScrollEnabled = true;
            }
            else
            {
                _isautoScrollEnabled = false;
            }
        }

        if (_isautoScrollEnabled && e.ExtentHeightChange != 0)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }

        ReadVisibleMessages();
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox textBox is false)
            return;

        if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Enter))
        {
            textBox.Text += "\r\n";
            textBox.SelectionStart = textBox.Text.Length;
            e.Handled = true;
        }
        else if (Keyboard.IsKeyDown(Key.Enter))
        {
            var textBinding = BindingOperations.GetBindingExpression(
                textBox, TextBox.TextProperty);

            if (textBinding != null)
                textBinding.UpdateSource();

            e.Handled = false;
        }
    }

    private async void ReadVisibleMessages()
    {
        Rect scrollViewerBounds = new Rect(new Point(0, 0), new Point(scroll.ActualWidth, scroll.ActualHeight));

        foreach (MessageViewModel messageViewModel in itemsControl.Items)
        {
            UIElement element = (UIElement)itemsControl.ItemContainerGenerator.ContainerFromItem(messageViewModel);
            GeneralTransform transform = element.TransformToVisual(scroll);
            Rect elementBounds = transform.TransformBounds(new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height));

            if (scrollViewerBounds.IntersectsWith(elementBounds))
            {
                if (messageViewModel.Message.IsRead is false && messageViewModel.Message.SenderUsername != App.Instance.CurrentUser.Username)
                {
                    messageViewModel.Message.IsRead = true;
                    await ((ChatViewModel)DataContext).ChatService.UpdateMessage(messageViewModel.Message);
                }
            }
        }
    }
}
