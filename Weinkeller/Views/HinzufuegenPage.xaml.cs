using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Devices.PointOfService;
using System.Net.Http;

namespace Weinkeller.Views
{
    public sealed partial class HinzufuegenPage : Page
    {
        List<Wein> WeinList = new List<Wein>();

        string barcode;
        string error;
        string name;
        string detailname;
        string vendor;
        string origin;
        string descr;
        string typ;
        int quantity;


        public HinzufuegenPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Load_data();

            InputTextDialogAsync("Barcode scannen");
        }

        private async void barcodeLookup()
        {

            LoadingGrid.Visibility = Visibility.Visible;
            ScanningGrid.Visibility = Visibility.Collapsed;
            // Überprüfen ob Barcode bereits vorhanden in eigener Datenbank
            int barcodeID = CheckDataBase(barcode);

            if (barcodeID == -1)
            {
                //string url = "http://opengtindb.org/?ean=" + barcode + "&cmd=query&queryid=400000000";
                Uri url = new Uri("http://opengtindb.org/?ean=" + barcode + "&cmd=query&queryid=400000000");
                //WebClient c = new WebClient();
                var c = new HttpClient();
                //byte[] response = await c.DownloadDataAsync(url);
                //string result = Encoding.UTF7.GetString(response);

                
                byte[] response = await c.GetByteArrayAsync(url);

                string result = Encoding.UTF7.GetString(response);

                error = result.Substring(result.IndexOf("error=") + 6);
                error = error.Substring(0, error.IndexOf("---"));
                error = error.Replace("\n", String.Empty);

                

                if (error == "0")
                    Show_Message("Operation war erfolgreich.\nDatenbank Inhalte wurden geladen.", "Datenbank Ergebniss");

                if (error == "0")
                {
                    name = result.Substring(result.IndexOf("name=") + 5);
                    name = name.Substring(0, name.IndexOf("detailname"));
                    name = name.Replace("\n", String.Empty);
                    detailname = result.Substring(result.IndexOf("detailname=") + 11);
                    detailname = detailname.Substring(0, detailname.IndexOf("vendor"));
                    detailname = detailname.Replace("\n", String.Empty);
                    vendor = result.Substring(result.IndexOf("vendor=") + 7);
                    vendor = vendor.Substring(0, vendor.IndexOf("maincat"));
                    vendor = vendor.Replace("\n", String.Empty);
                    origin = result.Substring(result.IndexOf("origin=") + 7);
                    origin = origin.Substring(0, origin.IndexOf("descr"));
                    origin = origin.Replace("\n", String.Empty);
                    descr = result.Substring(result.IndexOf("descr=") + 6);
                    descr = descr.Substring(0, descr.IndexOf("name_en"));
                    descr = descr.Replace("\n", String.Empty);
                    quantity = 1;
                    typ = "";


                    WeinList.Add(new Wein(barcode, name, detailname, vendor, origin, descr, typ, quantity));

                    Load_Wine(WeinList.Count - 1);
                }
                else
                {
                    string datenbankergebniss = "Unbekannter Fehler";

                    if (error == "1")
                        datenbankergebniss = "Die EAN konnte nicht gefunden werden";
                    else if (error == "2")
                        datenbankergebniss = "Die EAN war fehlerhaft (Checksummenfehler)";
                    else if (error == "3")
                        datenbankergebniss = "Die EAN war fehlerhaft (ungültiges Format / fehlerhafte Ziffernanzahl)";
                    else if (error == "4")
                        datenbankergebniss = "Es wurde eine für interne Anwendungen reservierte EAN eingegeben (In-Store, Coupon etc.)";
                    else if (error == "5")
                        datenbankergebniss = "Zugriffslimit auf die Datenbank wurde überschritten";
                    else if (error == "6")
                        datenbankergebniss = "Es wurde kein Produktname angegeben";
                    else if (error == "7")
                        datenbankergebniss = "Der Produktname ist zu lang (max. 20 Zeichen)";
                    else if (error == "8")
                        datenbankergebniss = "Die Nummer für die Hauptkategorie fehlt oder liegt außerhalb des erlaubten Bereiches";
                    else if (error == "9")
                        datenbankergebniss = "Die Nummer für die zugehörige Unterkategorie fehlt oder liegt außerhalb des erlaubten Bereiches";
                    else if (error == "10")
                        datenbankergebniss = "Unerlaubte Daten im Herstellerfeld";
                    else if (error == "11")
                        datenbankergebniss = "Unerlaubte Daten im Beschreibungsfeld";
                    else if (error == "12")
                        datenbankergebniss = "Daten wurden bereits übertragen";
                    else if (error == "13")
                        datenbankergebniss = "Die UserID/queryid fehlt in der Abfrage oder ist für diese Funktion nicht freigeschaltet";
                    else if (error == "14")
                        datenbankergebniss = "Es wurde mit dem Parameter \"cmd\" ein unbekanntes Kommando übergeben";

                    ManuellesAnlegenCheck(datenbankergebniss);
                }
            }
            else
            {
                WeinList[barcodeID].addBottle();
                barcode = WeinList[barcodeID].getBarcode();
                name = WeinList[barcodeID].getName();
                detailname = WeinList[barcodeID].getDetailname();
                vendor = WeinList[barcodeID].getVendor();
                origin = WeinList[barcodeID].getOrigin();
                descr = WeinList[barcodeID].getDescr();
                typ = WeinList[barcodeID].getTyp();
                quantity = WeinList[barcodeID].getQuantity();

                Show_Message("Datenbankeintrag war bereits vorhanden.\nLagerstand wurde um eins erhöht.", "Bekannter Wein gefunden");

                Load_Wine(barcodeID);
            }
            LoadingGrid.Visibility = Visibility.Collapsed;
        }

        private void Load_Wine(int wine_index)
        {
            Text_Name.Text = WeinList[wine_index].getName();
            if (Text_Name.Text == null || Text_Name.Text == "")
                Text_Name.Text = WeinList[wine_index].getDetailname();
            text_Vendor.Text = WeinList[wine_index].getVendor();
            text_Origin.Text = WeinList[wine_index].getOrigin();
            text_Descr.Text = WeinList[wine_index].getDescr();
            text_Quantity.Text = WeinList[wine_index].getQuantity().ToString();
            text_Barcode.Text = WeinList[wine_index].getBarcode();
            text_Type.Text = WeinList[wine_index].getTyp();
            text_Quantity.Text = WeinList[wine_index].getQuantity().ToString();
        }

        private async void ManuellesAnlegenCheck(string datenbankergebniss)
        {

            var messageCheck = new MessageDialog("Barcode " + barcode + " ist in der Datenbank nicht vorhanden.\nFehler: " + datenbankergebniss +"\n\nManuell anlegen?", "Datenbank Ergebniss");
            messageCheck.Commands.Add(new UICommand("Ja", null, 0));
            messageCheck.Commands.Add(new UICommand("Nein", null, 1));

            messageCheck.DefaultCommandIndex = 1;

            var commandChosen = await messageCheck.ShowAsync();

            if (commandChosen.Label == "Ja")
            {
                // Auf manuelles Anlegen Seite wechseln
                text_Barcode.Text = barcode;
            }
            else if (commandChosen.Label == "Nein")
            {
                // Auf Hauptseite wechseln
                this.Frame.Navigate(typeof(WeinkellerPage));
            }
        }

        private int CheckDataBase(string e_barcode)
        {

            for (int i = 0; i < WeinList.Count; i++)
            {
                if (WeinList[i].getBarcode() == e_barcode)
                    return i;
            }

            return (-1);

        }

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            barcode = text_Barcode.Text;
            name = Text_Name.Text;
            vendor = text_Vendor.Text;
            origin = text_Origin.Text;
            descr = text_Descr.Text;
            typ = text_Type.Text;
            quantity = Convert.ToInt32(text_Quantity.Text);

            CreateFile();

        }

        private async void CreateFile()
        {
            try
            {
                string data_string = barcode + ";" + name + ";" + detailname + ";" + vendor + ";" + origin + ";" + descr + ";" + typ + ";" + quantity.ToString();
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile = await storageFolder.CreateFileAsync(barcode + ".txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, data_string);

                var messageCheck = new MessageDialog("Neuer Wein hinzugefügt", "Speichern");
                messageCheck.Commands.Add(new UICommand("Weiteren Barcode einscannen", null, 0));
                messageCheck.Commands.Add(new UICommand("Zu Hauptseite zurückkehren", null, 1));

                messageCheck.DefaultCommandIndex = 1;

                var commandChosen = await messageCheck.ShowAsync();

                if (commandChosen.Label == "Weiteren Barcode einscannen")
                {
                    // Auf manuelles Anlegen Seite wechseln
                    this.Frame.Navigate(typeof(HinzufuegenPage));
                }
                else if (commandChosen.Label == "Zu Hauptseite zurückkehren")
                {
                    // Auf Hauptseite wechseln
                    this.Frame.Navigate(typeof(WeinkellerPage));
                }

            }
            catch (Exception ex)
            {
                var messageCheck = new MessageDialog("Speichern ist fehlgeschlagen\nFehler: " + ex.Message, "Fehler");
                messageCheck.Commands.Add(new UICommand("Zur Eingabe zurückkehren", null, 0));
                messageCheck.Commands.Add(new UICommand("Zur Hauptseite zurückkehren", null, 1));

                messageCheck.DefaultCommandIndex = 1;

                var commandChosen = await messageCheck.ShowAsync();

                if (commandChosen.Label == "Zur Eingabe zurückkehren")
                {
                    // 
                }
                else if (commandChosen.Label == "Zur Hauptseite zurückkehren")
                {
                    // Auf Hauptseite wechseln
                    this.Frame.Navigate(typeof(WeinkellerPage));
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            test_wein();
        }

        private async void test_open()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync("sample.txt");

            string text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
        }

        private void test_wein()
        {
            barcode = "4003301029387";
            name = "test Wein";
            detailname = "test Wein zum testen der Speicherung";
            vendor = "Test Inc.";
            origin = "Testingen";
            descr = "Dieser Wein wurde zum Testen der Speicherung angelegt";
            typ = "Rotwein";
            quantity = 34;

            CreateFile();
        }

        private void Btn_scan_Click(object sender, RoutedEventArgs e)
        {
            InputTextDialogAsync("Barcode scannen");
        }

        private async void Show_Message(string Message, string Titel)
        {
            var messageCheck = new MessageDialog(Message, Titel);
            await messageCheck.ShowAsync();
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
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                barcode = text_Barcode.Text = inputTextBox.Text;
                barcodeLookup();
            }
            else
                text_Barcode.Text = "";
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
        }
    }
}
