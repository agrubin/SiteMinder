using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
                    WriteResponseLine(string.Format("OTA_PingRS error - Timestamp: {2}  Type: {0}  Value: {1}", error.Type, error.Value, timestamp));
                }
            }


            //ReservationError resErr = new ReservationError(ERR.Hotel_not_active, EWT.Processing_exception, "hello");        
        }

        private void WriteResponseLine(string responseLine)
        {
            listBox_Responses.Items.Add(responseLine);
            ListBoxAutomationPeer svAutomation = (ListBoxAutomationPeer)ScrollViewerAutomationPeer.CreatePeerForElement(listBox_Responses);

            IScrollProvider scrollInterface = (IScrollProvider)svAutomation.GetPattern(PatternInterface.Scroll);
            ScrollAmount scrollVertical = ScrollAmount.LargeIncrement;
            ScrollAmount scrollHorizontal = ScrollAmount.NoAmount;
            if (scrollInterface.VerticallyScrollable)
                scrollInterface.Scroll(scrollHorizontal, scrollVertical);
        }

        private async void button_Read_Click(object sender, RoutedEventArgs e)
        {
            ResStatus resStatus = ResStatus.All;
            if (radioButton_Modify.IsChecked == true) resStatus = ResStatus.Modify;
            if (radioButton_Cancel.IsChecked == true) resStatus = ResStatus.Cancel;
            if (radioButton_Book.IsChecked == true) resStatus = ResStatus.Book;

            ReadRQResponse reservationsResponse = await API.OTA_ReadRQ(pmsID, username, password, hotelCode, resStatus);

            if (reservationsResponse.OTA_ResRetrieveRS.Items[0].GetType() == typeof(SuccessType))
            {
                WriteResponseLine(string.Format("OTA_ResRetrieveRS success..."));

                if (reservationsResponse.OTA_ResRetrieveRS.Items.Length > 1)
                {
                    WriteResponseLine(string.Format("# of reservations: {0} ", reservationsResponse.OTA_ResRetrieveRS.Items.Length));
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

                        string msgType = uniqueIDs[1].Type;
                        string msgID = uniqueIDs[1].ID;

                        string resStatusText = hotelReservation.ResStatus;
                        DateTime dateTimeStamp = resStatusText == "Book" ? hotelReservation.CreateDateTime : hotelReservation.LastModifyDateTime;

                        WriteResponseLine(string.Format("Reservation - resID: {0} msgID: {1} Status: {4} TimeStamp: {3}", resIDPMS, msgID, dateTimeStamp.ToString(), resStatusText));

                        //
                        // Send a reservation confirmation.
                        //

                        //ReservationError resError = new ReservationError(OTA_ERR.Invalid_rate_code, OTA_EWT.Biz_rule, "Invalid rate entered.");
                        //NotifReportRQResponse confirmResponse = await API.OTA_NotifReportRQ(username, password, resError, resStatusText, dateTimeStamp, msgID, resIDPMS);

                        //
                        // Make sure that no errors were generated during confirmation!
                        //

                        //if(confirmResponse.OTA_NotifReportRS.Items[0].GetType() == typeof(SuccessType))
                        //{
                        //
                        // Confirmation was processed correctly.
                        //
                        //}
                        //else
                        //{
                        //
                        // Confirmation error.
                        //
                        //}
                    }
                }
                else
                {
                    WriteResponseLine("No reservation list available.");
                }
            }
            else
            {
                string timestamp = reservationsResponse.OTA_ResRetrieveRS.TimeStamp.ToString();
                ErrorsType errors = (ErrorsType)reservationsResponse.OTA_ResRetrieveRS.Items[0];
                foreach (var error in errors.Error)
                {
                    WriteResponseLine(string.Format("OTA_ResRetrieveRS error - Timestamp: {2}  Type: {0}  Value: {1}", error.Type, error.Value, timestamp));
                }
            }
        }
    }
}
