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

        private async void button_Ping_Click(object sender, RoutedEventArgs e)
        {
            textBlock_Ping.Text = "Sending...";
            PingRQResponse pingResponse = await API.OTA_PingRQ(username, password);
            if (pingResponse.OTA_PingRS.Items[0].GetType() == typeof(SuccessType))
            {
                textBlock_Ping.Text = pingResponse.OTA_PingRS.Items[1].ToString();
            }
            else
            {
                string timestamp = pingResponse.OTA_PingRS.TimeStamp.ToString();
                ErrorsType errors = (ErrorsType)pingResponse.OTA_PingRS.Items[0];
                foreach (var error in errors.Error)
                {
                    textBlock_Ping.Text = "Error";
                    string errLine = string.Format("OTA_PingRS error - Timestamp: {2}  Type: {0}  Value: {1}", error.Type, error.Value, timestamp);
                    listBox_Errors.Items.Add(errLine);
                }
            }


            //ReservationError resErr = new ReservationError(ERR.Hotel_not_active, EWT.Processing_exception, "hello");        
        }

        private async void button_Read_Click(object sender, RoutedEventArgs e)
        {
            ReadRQResponse reservationsResponse = await API.OTA_ReadRQ(pmsID, username, password, hotelCode, ResStatus.All);

            if (reservationsResponse.OTA_ResRetrieveRS.Items[0].GetType() == typeof(SuccessType))
            {
                if(reservationsResponse.OTA_ResRetrieveRS.Items.Length > 1)
                {
                    OTA_ResRetrieveRSReservationsList reservationList = (OTA_ResRetrieveRSReservationsList)reservationsResponse.OTA_ResRetrieveRS.Items[1];

                    //
                    // Got the reservation list, so now process it....
                    //

                    foreach (HotelReservationType hotelReservation in reservationList.Items)
                    {
                        //
                        // Get the pmsXchange reservation reference.
                        //

                        UniqueID_Type[] uniqueIDs = hotelReservation.UniqueID;
                        string resType = uniqueIDs[0].Type;
                        string resIDPMS = uniqueIDs[0].ID;
                        string resIDContext = uniqueIDs[0].ID_Context;

                        string msgType = uniqueIDs[1].Type;
                        string msgID = uniqueIDs[1].ID;
                        string msgIDContext = uniqueIDs[1].ID_Context;

                        string resStatus = hotelReservation.ResStatus;
                        DateTime dateTimeStamp = resStatus == "Book" ? hotelReservation.CreateDateTime : hotelReservation.LastModifyDateTime;

                        //
                        // Send a reservation confirmation.
                        //

                        ReservationError resError = new ReservationError(OTA_ERR.Invalid_rate_code, OTA_EWT.Biz_rule, "Invalid rate entered.");
                        NotifReportRQResponse confirmResponse = await API.OTA_NotifReportRQ(username, password, resError, resStatus, dateTimeStamp, msgID, resIDPMS);

                        //
                        // Make sure that no errors were generated during confirmation!
                        //

                        if(confirmResponse.OTA_NotifReportRS.Items[0].GetType() == typeof(SuccessType))
                        {
                            //
                            // Confirmation was processed correctly.
                            //
                        }
                        else
                        {
                            //
                            // Confirmation error.
                            //
                        }
                    }
                }
            }
            else
            {
                string timestamp = reservationsResponse.OTA_ResRetrieveRS.TimeStamp.ToString();
                ErrorsType errors = (ErrorsType)reservationsResponse.OTA_ResRetrieveRS.Items[0];
                foreach (var error in errors.Error)
                {
                    string errLine = string.Format("OTA_ResRetrieveRS error - Timestamp: {2}  Type: {0}  Value: {1}", error.Type, error.Value, timestamp);
                    listBox_Errors.Items.Add(errLine);
                }
            }
        }
    }
}
