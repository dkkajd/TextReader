using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using TextReader.ViewModel;
using TextReader.View;

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

            var window = new View.MainWindow();

            // Create the ViewModel to which the main window binds
            var viewModel = new MainWindowViewModel();

            // When the ViewModel asks to be closed, close the window
            viewModel.RequestClose += delegate { window.Close(); };

            // set the DataContext to the viewModel
            window.DataContext = viewModel;

            window.Show();
        }
    }
}
