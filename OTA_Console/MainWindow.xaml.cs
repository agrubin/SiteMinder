using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using pmsXchange;
using SiteMinder.pmsXchangeService;

namespace OTA_Console
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string username = "SPIOrangeTest";
        const string password = "ymdqMsjBNutXLQMdVvtJZVXe";
        const string pmsID = "SPIORANGE";
        const string hotelCode = "SPI516";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Ping_Click(object sender, RoutedEventArgs e)
        {
            //OTA_ResRetrieveRS reservationsResponse = API.OTA_ReadRQ(pmsID, username, password, hotelCode, ResStatus.All);

            PingRQResponse pingResponse = await API.OTA_PingRS(username, password);
            if (pingResponse.OTA_PingRS.Items[0].GetType() == typeof(SuccessType))
            {
                string echo = pingResponse.OTA_PingRS.Items[1].ToString();
            }
            else
            {
                ErrorsType errors = (ErrorsType)pingResponse.OTA_PingRS.Items[0];
                foreach (var error in errors.Error)
                {

                }
            }


            //ReservationError resErr = new ReservationError(ERR.Hotel_not_active, EWT.Processing_exception, "hello");        
        }
    }
}
