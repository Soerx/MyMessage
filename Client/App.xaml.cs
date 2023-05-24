using Client.Models;
using System.Windows;

namespace Client;

public partial class App : Application
{
    public static App Instance => (App)Current;
    public User? CurrentUser { get; set; }
}
