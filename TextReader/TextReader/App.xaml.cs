using System.Windows;
using TextReader.ViewModel;
using TextReader.View;
using TextReader.Model;
using System.Collections.Generic;
using System.Windows.Documents;

namespace TextReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();

            var welcomeText = new ReadDocument((FlowDocument)Application.Current.Resources["WelcomeDocument"]) { Name = "Welcome" };

            var docMan = new ReadDocumentManager(new List<ReadDocument>() { welcomeText });

            // Create the ViewModel to which the main window binds
            var viewModel = new MainWindowViewModel(docMan);

            // When the ViewModel asks to be closed, close the window
            viewModel.RequestClose += delegate { window.Close(); };

            // set the DataContext to the viewModel
            window.DataContext = viewModel;

            window.Show();
        }
    }
}
