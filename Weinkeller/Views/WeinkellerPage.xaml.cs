using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Weinkeller.Views
{
    public sealed partial class WeinkellerPage : Page
    {

        List<Wein> WeinList = new List<Wein>();
        List<Wein> WeinListEmpty = new List<Wein>();
        int currentWein;

        private bool _isSwiped;

        public WeinkellerPage()
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
                if (filenameList.Count > 0)
                {
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
                    }

                    currentWein = 0;
                    Load_Wine(currentWein);
                }

                if (WeinList.Count == 0)
                {
                    grid_amazon.Visibility = Visibility.Collapsed;
                    grid_empty.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_amazon.Visibility = Visibility.Visible;
                    grid_empty.Visibility = Visibility.Collapsed;
                }

            }catch(Exception ex)
            {
                Show_Message("Es ist ein Fehler beim Öffnen der Dateien aufgetreten.\nBitte überprüfen Sie den Speicherort. \n\nFehler: " + ex.Message, "Fehler");
            }
        }

        private void Load_Wine(int wine_index)
        {
            Text_Name.Text = WeinList[wine_index].getName();
            if (Text_Name.Text == null || Text_Name.Text == "")
                Text_Name.Text = WeinList[wine_index].getDetailname();
            text_Vendor.Text = WeinList[wine_index].getVendor();
            text_Origin.Text = WeinList[wine_index].getOrigin();
            text_descr.Text = WeinList[wine_index].getDescr();
            text_Quantity.Text = WeinList[wine_index].getQuantity().ToString();
            text_barcode.Text = WeinList[wine_index].getBarcode();
            text_type.Text = WeinList[wine_index].getTyp();
            text_Quantity.Text = WeinList[wine_index].getQuantity().ToString();

            Load_image(WeinList[wine_index].getBarcode());
            Load_page(wine_index);
        }

        private void Load_page(int wine_index)
        {
            int current_page = wine_index + 1;
            int max_page = WeinList.Count();

            text_page.Text = current_page.ToString() + "/" + max_page;
        }

        private async void Load_image(string image_name)
        {
            FileInfo fInfo = new FileInfo(Windows.Storage.ApplicationData.Current.LocalFolder.ToString() + "." + image_name + ".jpg");

            List<string> filenameList = new List<string>();
            StorageFolder dataFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            IReadOnlyList<StorageFile> fileList = await dataFolder.GetFilesAsync();

            StorageFile imageFile;

            WineImage.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/kein-bild-vorhanden.png"));

            foreach (StorageFile file in fileList)
            {
                if (file.FileType.ToString() == ".jpg" && file.DisplayName.ToString() == WeinList[currentWein].getBarcode())
                {
                    imageFile = file;

                    var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.ToString(), "WeinBilder", image_name + ".jpg");
                    var uri = new Uri(imageFile.Path.ToString());

                    var bitmap = new BitmapImage(uri);

                    WineImage.Source = bitmap;
                }
            }
        }

        private void Btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (currentWein != 0)
                currentWein--;
            else
                currentWein = WeinList.Count - 1;
            Load_Wine(currentWein);
        }

        private void Btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (currentWein != WeinList.Count - 1)
                currentWein++;
            else
                currentWein = 0;
            Load_Wine(currentWein);
        }

        private void SwipeableTextBlock_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !_isSwiped)
            {
                var swipedDistance = e.Cumulative.Translation.X;

                if (Math.Abs(swipedDistance) <= 2) return;

                if (swipedDistance > 0)
                {

                    if (currentWein != 0)
                        currentWein--;
                    else
                        currentWein = WeinList.Count - 1;
                    Load_Wine(currentWein);
                }
                else
                {
                    if (currentWein != WeinList.Count - 1)
                        currentWein++;
                    else
                        currentWein = 0;
                    Load_Wine(currentWein);
                }
                _isSwiped = true;
            }
        }

        private void SwipeableTextBlock_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isSwiped = false;
        }

        private void SwipeableTextBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            pickImage();
        }

        private async void pickImage()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".jpg");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    await file.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, WeinList[currentWein].getBarcode() + file.FileType.ToString(), NameCollisionOption.ReplaceExisting);
                    Show_Message("Bild wurde erfolgreich gespeichert.\n\nWurde ein altes Bild überspeichert muss das Program neu gestartet werden.", "Speichern erfolgreich");
                    Load_image(WeinList[currentWein].getBarcode());
                }
                catch(Exception ex)
                {
                    Show_Message("Bild speichern konnte nicht durchgeführt werden. \n" + ex.Message, "Fehler bei Speichern");
                }
            }
        }

        private async void Show_Message(string Message, string Titel)
        {
            var messageCheck = new MessageDialog(Message, Titel);
            await messageCheck.ShowAsync();
        }

        private void Btn_amazon_Click(object sender, RoutedEventArgs e)
        {
            if (webView_amazon.Visibility == Visibility.Collapsed)
            {
                webView_amazon.Visibility = Visibility.Visible;
                var uri = new Uri("https://www.amazon.de/s/ref=nb_sb_noss_2?__mk_de_DE=%C3%85M%C3%85%C5%BD%C3%95%C3%91&url=search-alias%3Daps&field-keywords=" + Text_Name.Text);
                webView_amazon.Navigate(uri);
            }
            else
            {
                webView_amazon.Visibility = Visibility.Collapsed;
            }
        }
    }
}
