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
using pmsXchange.pmsXchangeService;

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

        private async void button_HotelAvailNotif_Click(object sender, RoutedEventArgs e)
        {
            WriteResponseLine(string.Format("Sending OTA_HotelAvailNotifRQ..."));

            DateTime start = new DateTime(2016, 8, 15);
            DateTime end = new DateTime(2016, 8, 18);
            API.AvailStatusMessages.AvailStatusMessage.StatusApplicationControl statusApplicationControl
                = new API.AvailStatusMessages.AvailStatusMessage.StatusApplicationControl(start, end, "S2S", "TR", new List<API.AvailStatusMessages.AvailStatusMessage.StatusApplicationControl.DestinationSystemCodes.DestinationSystemCode> { new API.AvailStatusMessages.AvailStatusMessage.StatusApplicationControl.DestinationSystemCodes.DestinationSystemCode("ATL") });
            API.AvailStatusMessages.AvailStatusMessage availStatusMessage = new API.AvailStatusMessages.AvailStatusMessage(statusApplicationControl, API.Restrictions.Stop_Sold, 1, 30, null);

            API.AvailStatusMessages availStatusMessages = new API.AvailStatusMessages(hotelCode, new List<API.AvailStatusMessages.AvailStatusMessage> { availStatusMessage });
            HotelAvailNotifRQResponse availResponse = await API.OTA_HotelAvailNotifRQ(pmsID, username, password, availStatusMessages);


            if (availResponse.OTA_HotelAvailNotifRS.Items[0].GetType() == typeof(SuccessType))
            {
                WriteResponseLine(string.Format("OTA_HotelAvailNotifRS success!"));
            }
            else
            {
                string timestamp = availResponse.OTA_HotelAvailNotifRS.TimeStamp.ToString();
                ErrorsType errors = (ErrorsType)availResponse.OTA_HotelAvailNotifRS.Items[0];

                foreach (var error in errors.Error)
                {
                    WriteResponseLine(string.Format("OTA_HotelAvailNotifRS error - Timestamp: {2},  Type: {0},  Value: {1}", error.Type, error.Value, timestamp));
                }
            }

            WriteResponseLine(string.Format(""));

        }
        private async void button_Ping_Click(object sender, RoutedEventArgs e)
        {
            WriteResponseLine(string.Format("Sending OTA_PingRQ..."));
            textBlock_Ping.Text = "Sending...";
            PingRQResponse pingResponse = await API.OTA_PingRQ(username, password);
            if (pingResponse.OTA_PingRS.Items[0].GetType() == typeof(SuccessType))
            {
                textBlock_Ping.Text = pingResponse.OTA_PingRS.Items[1].ToString();
                WriteResponseLine(string.Format("OTA_PingRS success!"));
            }
            else
            {
                string timestamp = pingResponse.OTA_PingRS.TimeStamp.ToString();
                ErrorsType errors = (ErrorsType)pingResponse.OTA_PingRS.Items[0];

                foreach (var error in errors.Error)
                {
                    textBlock_Ping.Text = "Error";
                    WriteResponseLine(string.Format("OTA_PingRS error - Timestamp: {2},  Type: {0},  Value: {1}", error.Type, error.Value, timestamp));
                }
            }

            WriteResponseLine(string.Format(""));

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

        OTA_ResRetrieveRSReservationsList reservationList;

        private async void button_Read_Click(object sender, RoutedEventArgs e)
        {
            WriteResponseLine(string.Format("Sending OTA_ResRetrieveRQ..."));
            button_NotifReport.IsEnabled = false;

            API.ResStatus resStatus = API.ResStatus.All;
            if (radioButton_Modify.IsChecked == true) resStatus = API.ResStatus.Modify;
            if (radioButton_Cancel.IsChecked == true) resStatus = API.ResStatus.Cancel;
            if (radioButton_Book.IsChecked == true) resStatus = API.ResStatus.Book;

            ReadRQResponse reservationsResponse = await API.OTA_ReadRQ(pmsID, username, password, hotelCode, resStatus);

            if (reservationsResponse.OTA_ResRetrieveRS.Items[0].GetType() == typeof(SuccessType))
            {
                WriteResponseLine(string.Format("OTA_ResRetrieveRS success!"));

                if (reservationsResponse.OTA_ResRetrieveRS.Items.Length > 1)
                {
                    reservationList = (OTA_ResRetrieveRSReservationsList)reservationsResponse.OTA_ResRetrieveRS.Items[1];
                    WriteResponseLine(string.Format("# of reservations: {0} ", reservationList.Items.Length));

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

                        WriteResponseLine(string.Format("Reservation - resID: {0}, msgID: {1}, Status: {2}, TimeStamp: {3}", resIDPMS, msgID, resStatusText, dateTimeStamp.ToString()));
                        button_NotifReport.IsEnabled = true;
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
                    WriteResponseLine(string.Format("OTA_ResRetrieveRS error - Timestamp: {2},  Type: {0},  Value: {1}", error.Type, error.Value, timestamp));
                }
            }

            WriteResponseLine(string.Format(""));

        }

        private async void button_NotifReport_Click(object sender, RoutedEventArgs e)
        {
            WriteResponseLine(string.Format("Sending OTA_NotifReportRQ..."));

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

                //
                // Send a reservation confirmation.
                //

                NotifReportRQResponse confirmResponse = null;

                if (checkBox_Conf_Errror.IsChecked == false)
                {
                    confirmResponse = await API.OTA_NotifReportRQ(username, password, null, resStatusText, dateTimeStamp, msgID, resIDPMS);
                }
                else
                {
                    API.ReservationError resError = new API.ReservationError((API.OTA_EWT)comboBox_OTA_EWT.SelectedValue, (API.OTA_ERR)comboBox_OTA_ERR.SelectedValue, null);
                    confirmResponse = await API.OTA_NotifReportRQ(username, password, null, resStatusText, dateTimeStamp, msgID, resIDPMS);
                }

                //
                // Make sure that no errors were generated during confirmation!
                //

                if (confirmResponse.OTA_NotifReportRS.Items[0].GetType() == typeof(SuccessType))
                {
                    //
                    // Confirmation was processed correctly.
                    //

                    WriteResponseLine(string.Format("Reservation resID: {0} confirmed successfully.", resIDPMS));
                    button_NotifReport.IsEnabled = false;
                }
                else
                {
                    //
                    // Confirmation error.
                    //

                    string timestamp = confirmResponse.OTA_NotifReportRS.TimeStamp.ToString();
                    ErrorsType errors = (ErrorsType)confirmResponse.OTA_NotifReportRS.Items[0];

                    foreach (var error in errors.Error)
                    {
                        WriteResponseLine(string.Format("OTA_NotifReportRS error - Timestamp: {2},  Type: {0},  Value: {1}", error.Type, error.Value, timestamp));
                    }
                }
            }

            WriteResponseLine(string.Format(""));

        }

        private void comboBox_OTA_EWT_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox_OTA_EWT.ItemsSource = Enum.GetValues(typeof(API.OTA_EWT));
            comboBox_OTA_EWT.SelectedIndex = 0;
        }

        private void comboBox_OTA_ERR_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox_OTA_ERR.ItemsSource = Enum.GetValues(typeof(API.OTA_ERR));
            comboBox_OTA_ERR.SelectedIndex = 0;
        }
    }
}
