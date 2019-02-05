using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Weinkeller.Views;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Weinkeller
{

    public sealed partial class MainPage : Page
    {
        List<Wein> WeinList = new List<Wein>();


        public MainPage()
        {
            this.InitializeComponent();
        }

        private void NavMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }

        private void NavMain_Loaded(object sender, RoutedEventArgs e)
        {

            var view = ApplicationView.GetForCurrentView();

            if (view.TryEnterFullScreenMode())
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                // The SizeChanged event will be raised when the entry to full-screen mode is complete.
            }


            // set the initial SelectedItem
            foreach (NavigationViewItemBase item in NavMain.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "weinkeller")
                {
                    NavMain.SelectedItem = item;
                    break;
                }
            }
            MainFrame.Navigate(typeof(WeinkellerPage));
        }

        private void NavMain_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                MainFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                TextBlock ItemContent = args.InvokedItem as TextBlock;
                if (ItemContent != null)
                {
                    switch (ItemContent.Tag)
                    {
                        case "Nav_Weinkeller":
                            MainFrame.Navigate(typeof(WeinkellerPage));
                            break;

                        case "Nav_Durchsuchen":
                            MainFrame.Navigate(typeof(DurchsuchenPage));
                            break;

                        case "Nav_Hinzufuegen":
                            MainFrame.Navigate(typeof(HinzufuegenPage));
                            break;

                        case "Nav_Abziehen":
                            Load_data();
                            InputTextDialogAsync("Barcode scannen");
                            break;
                    }
                }
            }
        }

        private async void InputTextDialogAsync(string title)
        {
            string barcode;

            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                barcode = inputTextBox.Text;

                RemoveBottle(barcode);
            }
            else
                barcode = "";

        }

        private void RemoveBottle(string barcode)
        {
            bool removed;
            for (int i = 0; i < WeinList.Count(); i++)
            {
                if (WeinList[i].getBarcode() == barcode)
                {
                    removed = WeinList[i].removeBottle();

                    if (removed)
                    {
                        CreateFile(WeinList[i].getBarcode(), WeinList[i].getName(), WeinList[i].getDetailname(), WeinList[i].getVendor(), WeinList[i].getOrigin(), WeinList[i].getDescr(), WeinList[i].getTyp(), WeinList[i].getQuantity());
                        return;
                    }
                    else
                    {
                        Show_Message("Flaschenmenge für diesen Barcode ist bereits 0", "Flasche entfernen");
                        MainFrame.Navigate(typeof(WeinkellerPage));
                        return;
                    }
                }

            }

            InputTextDialogAsync("Flasche nicht gefunden. Barcode erneut scannen");
        }

        private async void Load_data()
        {
            string temp_barcode;
            string temp_name;
            string temp_detailname;
            string temp_vendor;
            string temp_origin;
            string temp_descr;
            string temp_type;
            int temp_quantity;
            string temp_string;

            try
            {
                List<string> filenameList = new List<string>();
                StorageFolder dataFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                IReadOnlyList<StorageFile> fileList = await dataFolder.GetFilesAsync();

                foreach (StorageFile file in fileList)
                {
                    if (file.FileType.ToString() == ".txt")
                        filenameList.Add(file.Name);
                }

                for (int i = 0; i < filenameList.Count; i++)
                {
                    Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync(filenameList[i]);

                    string text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

                    temp_barcode = text.Substring(0, text.IndexOf(";"));
                    temp_string = text.Substring(text.IndexOf(";") + 1);
                    temp_name = temp_string.Substring(0, temp_string.IndexOf(";"));
                    temp_string = temp_string.Substring(temp_string.IndexOf(";") + 1);
                    temp_detailname = temp_string.Substring(0, temp_string.IndexOf(";"));
                    temp_string = temp_string.Substring(temp_string.IndexOf(";") + 1);
                    temp_vendor = temp_string.Substring(0, temp_string.IndexOf(";"));
                    temp_string = temp_string.Substring(temp_string.IndexOf(";") + 1);
                    temp_origin = temp_string.Substring(0, temp_string.IndexOf(";"));
                    temp_string = temp_string.Substring(temp_string.IndexOf(";") + 1);
                    temp_descr = temp_string.Substring(0, temp_string.IndexOf(";"));
                    temp_string = temp_string.Substring(temp_string.IndexOf(";") + 1);
                    temp_type = temp_string.Substring(0, temp_string.IndexOf(";"));
                    temp_string = temp_string.Substring(temp_string.IndexOf(";") + 1);
                    temp_quantity = Convert.ToInt32(temp_string);


                    WeinList.Add(new Wein(temp_barcode, temp_name, temp_detailname, temp_vendor, temp_origin, temp_descr, temp_type, temp_quantity));
                }
            }catch(Exception ex)
            {
                Show_Message("Es ist ein Fehler beim Öffnen der Dateien aufgetreten.\nBitte überprüfen Sie den Speicherort. \n\nFehler: " + ex.Message, "Fehler");
            }
        }

        private async void CreateFile(string barcode, string name, string detailname, string vendor, string origin, string descr, string typ, int quantity)
        {
            try
            {
                string data_string = barcode + ";" + name + ";" + detailname + ";" + vendor + ";" + origin + ";" + descr + ";" + typ + ";" + quantity.ToString();
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync(barcode + ".txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, data_string);

                var messageCheck = new MessageDialog("Flasche wurde aus Weinkeller entfernt", "Flasche entfernen");

                var commandChosen = await messageCheck.ShowAsync();

                MainFrame.Navigate(typeof(WeinkellerPage));

            }
            catch (Exception ex)
            {
                var messageCheck = new MessageDialog("Speichern ist fehlgeschlagen\nFehler: " + ex.Message, "Fehler");

                var commandChosen = await messageCheck.ShowAsync();

                MainFrame.Navigate(typeof(WeinkellerPage));
            }

        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            string currentWindow = MainFrame.Content.GetType().ToString();

            switch(currentWindow)
            {
                case "Weinkeller.Views.WeinkellerPage":
                    NavMain.SelectedItem = NavMain.MenuItems[0];
                    break;
                case "Weinkeller.Views.DurchsuchenPage":
                    NavMain.SelectedItem = NavMain.MenuItems[1];
                    break;

                case "Weinkeller.Views.HinzufuegenPage":
                    NavMain.SelectedItem = NavMain.MenuItems[2];
                    break;
                    

            }

            
        }

        private async void Show_Message(string Message, string Titel)
        {
            var messageCheck = new MessageDialog(Message, Titel);
            await messageCheck.ShowAsync();
        }
    }
}
