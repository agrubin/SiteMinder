using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

using SiteMinder.pmsXchangeService;

namespace pmsXchange
{
    //
    // The .NET service reference for pmsXchange uses a special version of the SiteMinder
    // WSDL file generated specifically for .NET clients:
    //
    // https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE/pmsxchange_flat.wsdl 
    //

    public enum ResStatus
    {
        All,        // All reservations.
        Book,       // New reservations.
        Modify,     // Modified reservations.
        Cancel      // Cancelled reservations.
    }

    // EWT      Error Type              Description
    // ---      ----------              -----------  
    //    
    // 1	    Unknown                 Indicates an unknown error.
    // 3	    Biz rule                Indicates that the XML message has passed a low-level validation check, but that the business rules for the request message were not met.
    // 4	    Authentication          Indicates the message lacks adequate security credentials.
    // 6	    Authorization           Indicates the message lacks adequate security credentials.
    // 10	    Required field missing  Indicates that an element or attribute that is required in by the schema (or required by agreement between trading partners) is missing from the message.
    //                                  For PmsXchange this type will also be returned if the xml message does not meet the restrictions (e.g data types) specified by the xml schema.
    // 12	    Processing exception    Indicates that during processing of the request that a not further defined exception occurred.

    public enum OTA_EWT
    {
        Unknown = 1,
        Biz_rule = 3,
        Authentication = 4,
        Authorization = 6,
        Required_field_missing = 10,
        Processing_exception = 12
    }

    //  ERR     Error Code                                      Description
    //  ---     ----------                                      -----------
    //  249     Invalid rate code                               Rate does not exist.
    //  375     Hotel not active                                Hotel is not enabled to receive inventory updates.
    //  385     Invalid confirmation or cancellation number     Confirmation or cancellation number does not exist.
    //  392     Invalid hotel code                              Hotel does not exist.
    //  402     Invalid room type                               Room does not exist.
    //  448     System error
    //  450     Unable to process	 
    //  783     Room or rate not found                          Room and rate combination does not exist.

    public enum OTA_ERR
    {
        Invalid_rate_code = 249,
        Hotel_not_active = 375,
        Invalid_confirmation_or_cancellation_number = 385,
        Invalid_hotel_code = 392,
        Invalid_room_type = 402,
        System_error = 448,
        Unable_to_process = 450,
        Room_or_rate_not_found = 783
    }

    public enum OTA_ID_Type
    {
        Customer = 1,
	    CRO,
	    Corporation_representative,
	    Company,
	    Travel_agency,
	    Airline,
	    Wholesaler,
	    Car_rental,
	    Group,
	    Hotel,
	    Tour_operator,
	    Cruise_line,
	    Internet_broker,
	    Reservation,
	    Cancellation,
	    Reference,
	    Meeting_planning_agency,
	    Other,
	    Insurance_agency,
	    Insurance_agent,
	    Profile,
	    ERSP,
	    Provisional_reservation,
	    Travel_Agent_PNR,
	    Associated_reservation,
	    Associated_itinerary_reservation,
	    Associated_shared_reservation,
	    Alliance,
	    Booking_agent,
	    Ticket,
	    Divided_reservation,
	    Merchant,
	    Acquirer,
	    Master_reference,
	    Purged_master_reference,
	    Parent_reference,
	    Child_reference,
	    Linked_reference,
	    Contract,
	    Confirmation_number,
	    Fare_quote,
	    Reissue_refund_quote
    }

    public class ReservationError
    {
        public OTA_ERR err { get; set; }
        public OTA_EWT ewt { get; set; }
        public string errorText { get; set; }
        public ReservationError(OTA_ERR errOTA, OTA_EWT ewtOTA, string errText)
        {
            err = errOTA;
            ewt = ewtOTA;

            //
            // Since this text is going into an XML node, invalid chars must be escaped with XML entities.
            //

            string xml = errText;
            errorText = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }

    //
    // The service connection implemeted as a singleton so it is only instantiated and initalized one time
    // upon the first access, then the same connection is returned on each subsequent access.  Use only for
    // synchronous calls.
    //
    public sealed class ServiceConnection
    {
        public static ServiceConnection Instance { get { return lazyConnection.Value; }  }
        private const string endpointURI = "https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE";  // Provided by SiteMinder.
        private static readonly Lazy<ServiceConnection> lazyConnection = new Lazy<ServiceConnection>(() => new ServiceConnection());
        public PmsXchangeServiceClient service { get; private set; }

        private ServiceConnection()
        {
            InitializeService();
        }

        public void InitializeService()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;

            EndpointAddress address = new EndpointAddress(endpointURI);
            service = new PmsXchangeServiceClient(binding, address);
        }
    }

    //
    // The service connection implemeted as a class, creates a new connectione very time it's instantiated. Use only for asynchronous calls.
    //
    public sealed class AsyncServiceConnection
    {
        private const string endpointURI = "https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE";  // Provided by SiteMinder.
        public PmsXchangeServiceClient service { get; private set; }

        public AsyncServiceConnection()
        {
            InitializeService();
        }

        public void InitializeService()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;

            EndpointAddress address = new EndpointAddress(endpointURI);
            service = new PmsXchangeServiceClient(binding, address);
        }
    }

    public static class API
    {
        private const string textType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";

        static public async Task<NotifReportRQResponse> OTA_NotifReportRQ(string usernameAuthenticate, string passwordAuthenticate, ReservationError resError, string resStatus, DateTime dateTimeStamp, string msgID, string resIDPMS)
        {
            NotifReportRQResponse response = null;

            try
            {
                PmsXchangeServiceClient service = new AsyncServiceConnection().service;

                OTA_NotifReportRQ body = new OTA_NotifReportRQ();
                body.Version = 1.0M;
                body.EchoToken = Guid.NewGuid().ToString();  // Echo token must be unique.            
                body.TimeStamp = DateTime.Now;
                body.TimeStampSpecified = true;
                if (resError == null)
                {
                    body.Items = new object[] { new SuccessType() };
                }
                else
                {
                    ErrorType errorType = API.CreateErrorType(resError.err, resError.ewt, resError.errorText);

                    ErrorsType errors = new ErrorsType();
                    ErrorType[] error = { errorType };
                    errors.Error = error;

                    body.Items = new object[] { errors };
                }

                body.NotifDetails = new OTA_NotifReportRQNotifDetails();
                body.NotifDetails.HotelNotifReport = new OTA_NotifReportRQNotifDetailsHotelNotifReport();
                OTA_NotifReportRQNotifDetailsHotelNotifReportHotelReservations hotelReservations = new OTA_NotifReportRQNotifDetailsHotelNotifReportHotelReservations();
                body.NotifDetails.HotelNotifReport.Item = hotelReservations;

                OTA_NotifReportRQNotifDetailsHotelNotifReportHotelReservationsHotelReservation[] hotelReservationList = new OTA_NotifReportRQNotifDetailsHotelNotifReportHotelReservationsHotelReservation[1];
                hotelReservationList[0] = new OTA_NotifReportRQNotifDetailsHotelNotifReportHotelReservationsHotelReservation();
     
                hotelReservations.HotelReservation = hotelReservationList;

                hotelReservations.HotelReservation[0].ResStatus = resStatus;
                if (resStatus == "Book")
                {
                    hotelReservations.HotelReservation[0].CreateDateTime = dateTimeStamp;
                }
                else
                {
                    hotelReservations.HotelReservation[0].LastModifyDateTime = dateTimeStamp;
                }
                hotelReservations.HotelReservation[0].UniqueID = new UniqueID_Type[1];
                hotelReservations.HotelReservation[0].UniqueID[0] = new UniqueID_Type();
                hotelReservations.HotelReservation[0].UniqueID[0].Type = OTA_ID_Type.Reference.ToString("d");
                hotelReservations.HotelReservation[0].UniqueID[0].ID = msgID;

                //
                // Only include the reservation ID info if there was no error processin this reservation.
                //

                if (resError == null)
                {
                    hotelReservations.HotelReservation[0].ResGlobalInfo = new ResGlobalInfoType();
                    hotelReservations.HotelReservation[0].ResGlobalInfo.HotelReservationIDs = new HotelReservationIDsTypeHotelReservationID[1];
                    hotelReservations.HotelReservation[0].ResGlobalInfo.HotelReservationIDs[0] = new HotelReservationIDsTypeHotelReservationID();
                    hotelReservations.HotelReservation[0].ResGlobalInfo.HotelReservationIDs[0].ResID_Type = OTA_ID_Type.Reservation.ToString("d");
                    hotelReservations.HotelReservation[0].ResGlobalInfo.HotelReservationIDs[0].ResID_Value = resIDPMS;
                }

                body.NotifDetails.HotelNotifReport.Item = hotelReservations;
                response = await service.NotifReportRQAsync(CreateSecurityHeader(usernameAuthenticate, passwordAuthenticate), body).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                response = new NotifReportRQResponse();
                response.OTA_NotifReportRS = new MessageAcknowledgementType();
                response.OTA_NotifReportRS.Items = new object[] { ProcessingException(ex) };
            }

            return response;
        }
        static public async Task<ReadRQResponse> OTA_ReadRQ(string pmsID, string usernameAuthenticate, string passwordAuthenticate, string hotelCode, ResStatus resStatus)
        {
            ReadRQResponse response = null;

            try
            {
                PmsXchangeServiceClient service = new AsyncServiceConnection().service;

                OTA_ReadRQ body = new OTA_ReadRQ();
                body.Version = 1.0M;
                body.EchoToken = Guid.NewGuid().ToString();  // Echo token must be unique.            
                body.TimeStamp = DateTime.Now;
                body.TimeStampSpecified = true;
                body.POS = CreatePOS(pmsID);
                OTA_ReadRQReadRequests readRequests = new OTA_ReadRQReadRequests();
                OTA_ReadRQReadRequestsHotelReadRequest hotelReadRequest = new OTA_ReadRQReadRequestsHotelReadRequest();
                hotelReadRequest.HotelCode = hotelCode;
                readRequests.Items = new object[] { hotelReadRequest };

                OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteria selectionCriteria = new OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteria();
                selectionCriteria.SelectionType = OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteriaSelectionType.Undelivered;
                selectionCriteria.SelectionTypeSpecified = true;  // Must be set to true, or ReadRQ returns an error.

                if (resStatus != ResStatus.All)
                {
                    selectionCriteria.ResStatus = resStatus.ToString();
                }

                hotelReadRequest.SelectionCriteria = selectionCriteria;
                body.ReadRequests = readRequests;

                //
                // Send a retrieve reservations request.
                //

                response = await service.ReadRQAsync(CreateSecurityHeader(usernameAuthenticate, passwordAuthenticate), body).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                response = new ReadRQResponse();
                response.OTA_ResRetrieveRS = new OTA_ResRetrieveRS();
                response.OTA_ResRetrieveRS.Items = new object[] { ProcessingException(ex) };
            }

            return response;
        }

        static public async Task<PingRQResponse> OTA_PingRQ(string usernameAuthenticate, string passwordAuthenticate)
        {
            PingRQResponse response = null;

            try
            {
                PmsXchangeServiceClient service = new AsyncServiceConnection().service;

                OTA_PingRQ body = new OTA_PingRQ();
                body.Version = 1.0M;
                body.EchoToken = Guid.NewGuid().ToString();  // Echo token must be unique.            
                body.TimeStamp = DateTime.Now;
                body.TimeStampSpecified = true;
                body.EchoData = "good echo";
                
                //
                // Send an asynchronous ping request.
                //

                response = await service.PingRQAsync(CreateSecurityHeader(usernameAuthenticate, passwordAuthenticate), body).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                //
                // Add a single error block to the response with a processing exception in case of an unusual error condition.
                //

                response = new PingRQResponse();
                response.OTA_PingRS = new OTA_PingRS();
                response.OTA_PingRS.Items = new object[] { ProcessingException(ex) };
            }

            return response;
        }

        //
        // Create an error block with a single undefined processing exception.
        //
        private static ErrorsType ProcessingException(Exception ex)
        {
            ErrorsType errors = new ErrorsType();
            ErrorType[] error = { CreateErrorType(OTA_ERR.System_error, OTA_EWT.Processing_exception, ex.Message) };
            errors.Error = error;

            return errors;
        }

        private static ErrorType CreateErrorType(OTA_ERR errCode, OTA_EWT errType, string errMsg)
        {
            ErrorType error = new ErrorType();
            error.Type = errType.ToString();
            error.Code = errCode.ToString();
            error.Value = errMsg;

            return error;
        }

        static private SecurityHeaderType CreateSecurityHeader(string usernameAuthenticate, string passwordAuthenticate)
        {
            SecurityHeaderType securityHeader = new SecurityHeaderType();
            securityHeader.Any = CreateUserNameToken(usernameAuthenticate, passwordAuthenticate);
            return securityHeader;
        }

        //
        // In this case, create a POS with an ERSP (Electronic Reservation Service Provider) type and the PMS ID provided
        // by SiteMinder.
        //

        static private SourceType[] CreatePOS(string pmsID)
        {
            SourceTypeRequestorID strid = new SourceTypeRequestorID();
            strid.Type = OTA_ID_Type.ERSP.ToString();
            strid.ID = pmsID;

            SourceType sourcetype = new SourceType();
            sourcetype.RequestorID = strid;

            return new SourceType[] { sourcetype };
        }

        static private System.Xml.XmlElement[] CreateUserNameToken(string usernameAuthenticate, string passwordAuthenticate)
        {
            //
            // Not all the SOAP envelope elements are available through the request block intellisense.
            // Those that aren't must be added as XmlElements.
            //

            XmlDocument doc = new XmlDocument();
            XmlElement usernametoken = doc.CreateElement("UsernameToken");
            XmlElement password = doc.CreateElement("Password");
            XmlElement username = doc.CreateElement("Username");

            //
            // Password is transmitted in plain text.
            //

            password.SetAttribute("Type", textType);

            XmlText usernameText = doc.CreateTextNode(usernameAuthenticate);
            XmlText passwordText = doc.CreateTextNode(passwordAuthenticate);

            username.AppendChild(usernameText);
            password.AppendChild(passwordText);
            usernametoken.AppendChild(username);
            usernametoken.AppendChild(password);

            return new XmlElement[] { usernametoken };
        }

        static private System.Xml.XmlElement[] CreateHotelReservations()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement hotelReservations = doc.CreateElement("HotelReservations");

            return new XmlElement[] { hotelReservations };
        }
    }
}
