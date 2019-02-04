using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Weinkeller.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        List<Wein> WeinList = new List<Wein>();
        List<Wein> WeinListEmpty = new List<Wein>();

        int count_barcode;
        int count_barcode_gt_zero;
        int count_barcode_is_zero;
        int count_flaschen;

        bool result;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Load_data();

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

            count_flaschen = 0;

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


                if (temp_quantity != 0)
                    WeinList.Add(new Wein(temp_barcode, temp_name, temp_detailname, temp_vendor, temp_origin, temp_descr, temp_type, temp_quantity));
                else
                    WeinListEmpty.Add(new Wein(temp_barcode, temp_name, temp_detailname, temp_vendor, temp_origin, temp_descr, temp_type, temp_quantity));

                count_flaschen += temp_quantity;
            }
            count_barcode_gt_zero = WeinList.Count();
            count_barcode_is_zero = WeinListEmpty.Count();
            count_barcode = count_barcode_gt_zero + count_barcode_is_zero;

            text_anzahl_barcodes.Text = count_barcode.ToString();
            text_anzahl_barcodes_gt_zero.Text = count_barcode_gt_zero.ToString();
            text_anzahl_barcodes_is_zero.Text = count_barcode_is_zero.ToString();

            text_anzahl_flaschen.Text = count_flaschen.ToString();
        }

        private async void Btn_data_delete_Click(object sender, RoutedEventArgs e)
        {
            await User_Check("Sollen wirklich alle gespeicherten Barcodes gelöscht werden?\n\n Dies kann nicht rückgängig gemacht werden.", "Daten löschen");
            if(result)
            {
                List<Wein> portList = new List<Wein>();
                portList.AddRange(WeinList);
                portList.AddRange(WeinListEmpty);
                Delete_Data(portList);
            }
        }

        private async void Btn_data_delete_zero_Click(object sender, RoutedEventArgs e)
        {
            await User_Check("Sollen wirklich alle Barcodes von denen keine Flaschen eingelagert sind gelöscht werden?\n\n Dies kann nicht rückgängig gemacht werden", "Daten löschen");
            if(result)
            {
                Delete_Data(WeinListEmpty);
            }
        }

        private async void Btn_data_delete_one_Click(object sender, RoutedEventArgs e)
        {
            InputTextDialogAsync("Barcode zum löschen eingeben");
        }

        private async void Btn_app_beenden_Click(object sender, RoutedEventArgs e)
        {
            await User_Check("Soll Weinkeller wirklich beendet werden?", "App beenden");
            if (result)
            {
                CoreApplication.Exit();
            }
        }

        private async void Btn_app_neustart_Click(object sender, RoutedEventArgs e)
        {
            await User_Check("Soll Weinkeller wirklich neu gestartet werden?", "App neu starten");
            if (result)
            {
                await CoreApplication.RequestRestartAsync("Weinkeller wird neu gestartet");
            }
        }

        private async void Btn_system_beenden_Click(object sender, RoutedEventArgs e)
        {
            await User_Check("Soll Windows wirklich heruntergefahren werden?", "Windows herunterfahren");
            if(result)
            {
                ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
            }
        }

        private async void Btn_system_neustart_Click(object sender, RoutedEventArgs e)
        {
            await User_Check("Soll Windows wirklich neu gestartet werden?", "Windows neu starten");
            if(result)
            {
                ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));
            }
        }

        private async Task User_Check(string text, string title)
        {
            result = false;
            var messageCheck = new MessageDialog(text, title);
            messageCheck.Commands.Add(new UICommand("Ja", null, 0));
            messageCheck.Commands.Add(new UICommand("Nein", null, 1));

            messageCheck.DefaultCommandIndex = 1;

            var commandChosen = await messageCheck.ShowAsync();

            if (commandChosen.Label == "Ja")
            {
                result = true;
            }
            else if(commandChosen.Label == "Nein")
            {
                result = false;
            }           
        }


        private async void Delete_Data(List<Wein> e_weinList)
        {
            StorageFolder dataFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            IReadOnlyList<StorageFile> fileList = await dataFolder.GetFilesAsync();

            int counter = 0;

            foreach (StorageFile file in fileList)
            {
                if (file.FileType.ToString() == ".txt" || file.FileType.ToString() == ".jpg")
                {
                    for (int i = 0; i < e_weinList.Count(); i++)
                    {
                        if(file.Name == e_weinList[i].getBarcode() + ".txt" || file.Name == e_weinList[i].getBarcode() + ".jpg")
                        {
                            await file.DeleteAsync();
                            counter++;
                        }
                    }
                }
            }

            await User_Check("Es wurden " + counter + " Einträge gelöscht.\n\n Zur Hauptseite zurückkeheren?", "Einträge gelöscht");

            if(result)
                this.Frame.Navigate(typeof(WeinkellerPage));


        }

        private async void InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";

            List<Wein> portListe = new List<Wein>();

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Wein tempwein = new Wein(inputTextBox.Text, "tempwein", "tempwein", "tempwein", "tempwein", "tempwein", "tempwein", 1);
                portListe.Add(tempwein);
                Delete_Data(portListe);
            }
        }

    }
}
