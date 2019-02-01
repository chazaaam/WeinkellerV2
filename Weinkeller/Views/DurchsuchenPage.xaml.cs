using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Weinkeller.Views
{

    public sealed partial class DurchsuchenPage : Page
    {
        List<Wein> WeinList = new List<Wein>();

        int currentWein;

        string barcode;

        bool _isSwiped;

        public DurchsuchenPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_search_Click(object sender, RoutedEventArgs e)
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

                bool quantity_check = false;

                try
                {
                    if (text_quantity.Text != "")
                    {
                        quantity_check = ((cmb_quantity.SelectedIndex == 0 && (Convert.ToInt32(text_quantity.Text) < temp_quantity)) ||
                                                (cmb_quantity.SelectedIndex == 1 && (Convert.ToInt32(text_quantity.Text) == temp_quantity)) ||
                                                (cmb_quantity.SelectedIndex == 2 && (Convert.ToInt32(text_quantity.Text) > temp_quantity)));
                    }
                }
                catch (Exception ex)
                {
                    Show_Message("Anzahl ist keine Zahl\n" + ex.Message, "Ungültige Eingabe");
                    return;
                }


                if (Text_Name.Text == "" || temp_name.Contains(Text_Name.Text) || temp_detailname.Contains(Text_Name.Text))
                {
                    if (text_quantity.Text == "" || quantity_check)
                    {
                        if (text_origin.Text == "" || temp_origin.Contains(text_origin.Text))
                        {
                            if (text_vendor.Text == "" || temp_vendor.Contains(text_vendor.Text))
                            {
                                if (text_descr.Text == "" || temp_descr.Contains(text_descr.Text))
                                {
                                    if (text_type.Text == "" || temp_type.Contains(text_type.Text))
                                    {
                                        if (barcode == "" || barcode == null || temp_barcode == barcode)
                                        {
                                            WeinList.Add(new Wein(temp_barcode, temp_name, temp_detailname, temp_vendor, temp_origin, temp_descr, temp_type, temp_quantity));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            barcode = "";

            currentWein = 0;
            if (WeinList.Count == 0)
                Show_Message("Keinen Wein gefunden", "Suche fehlgeschlagen");
            else
            {
                Load_Wine(currentWein);
                grid_search.Visibility = Visibility.Visible;
            }
        }

        private async void Show_Message(string Message, string Titel)
        {
            var messageCheck = new MessageDialog(Message, Titel);
            await messageCheck.ShowAsync();
        }

        private void Load_Wine(int currentWein)
        {
            Text_s_Name.Text = WeinList[currentWein].getName();
            if (Text_s_Name.Text == null || Text_s_Name.Text == "")
                Text_s_Name.Text = WeinList[currentWein].getDetailname();
            text_s_Vendor.Text = WeinList[currentWein].getVendor();
            text_s_Origin.Text = WeinList[currentWein].getOrigin();
            text_s_descr.Text = WeinList[currentWein].getDescr();
            text_s_Quantity.Text = WeinList[currentWein].getQuantity().ToString();
            text_s_barcode.Text = WeinList[currentWein].getBarcode();
            text_s_Type.Text = WeinList[currentWein].getBarcode();
            text_s_Quantity.Text = WeinList[currentWein].getQuantity().ToString();

            Load_image(WeinList[currentWein].getBarcode());
            Load_page(currentWein);
        }

        private void Btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (currentWein != WeinList.Count - 1)
                currentWein++;
            else
                currentWein = 0;
            Load_Wine(currentWein);
        }

        private void Btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (currentWein != 0)
                currentWein--;
            else
                currentWein = WeinList.Count - 1;
            Load_Wine(currentWein);
        }

        private void Load_page(int wine_index)
        {
            int current_page = wine_index + 1;
            int max_page = WeinList.Count();

            text_page.Text = current_page.ToString() + "/" + max_page;
        }

        private void Load_image(string image_name)
        {
            FileInfo fInfo = new FileInfo("WeinBilder\\" + image_name + ".jpg");
            if (fInfo.Exists)
            {
                var path = Path.Combine(Environment.CurrentDirectory, "WeinBilder", image_name + ".jpg");
                var uri = new Uri(path);

                var bitmap = new BitmapImage(uri);

                WineImage.Source = bitmap;
            }
        }

        private void Btn_scan_Click(object sender, RoutedEventArgs e)
        {
            InputTextDialogAsync("Barcode zum suchen scannen");
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
                barcode = inputTextBox.Text;
                Load_data();
            }
        }

        private void SwipeableTextBlock_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !_isSwiped)
            {
                var swipedDistance = e.Cumulative.Translation.X;

                if (Math.Abs(swipedDistance) <= 2) return;

                if (swipedDistance > 0)
                {
                    if (currentWein != WeinList.Count - 1)
                        currentWein++;
                    else
                        currentWein = 0;
                    Load_Wine(currentWein);
                }
                else
                {
                    if (currentWein != 0)
                        currentWein--;
                    else
                        currentWein = WeinList.Count - 1;
                    Load_Wine(currentWein);
                }
                _isSwiped = true;
            }
        }

        private void SwipeableTextBlock_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isSwiped = false;
        }
    }

}
