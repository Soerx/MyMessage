using Client.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls;
/// <summary>
/// Логика взаимодействия для MessageControl.xaml
/// </summary>
public partial class MessageControl : UserControl
{
    public MessageViewModel? MessageViewModel
    {
        get
        {
            if (DataContext is MessageViewModel viewModel)
                 return viewModel;

            return null;
        }
        set
        {
            if (value is MessageViewModel viewModel)
                DataContext = viewModel;
        }
    }

    public MessageControl()
    {
        InitializeComponent();
    }
}
