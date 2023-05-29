using Client.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls;
/// <summary>
/// Логика взаимодействия для MessageControl.xaml
/// </summary>
public partial class MessageControl : UserControl
{
    public readonly MessageViewModel MessageViewModel;

    public MessageControl(MessageViewModel messageViewModel)
    {
        MessageViewModel = messageViewModel;
        DataContext = MessageViewModel;
        InitializeComponent();
    }
}
