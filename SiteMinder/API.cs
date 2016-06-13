using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

using pmsXchange.pmsXchangeService;



namespace pmsXchange
{
    //
    // The .NET service reference for pmsXchange uses a special version of the SiteMinder
    // WSDL file generated specifically for .NET clients:
    //
    // https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE/pmsxchange_flat.wsdl 
    //

    public static class API
    {
        public enum Restrictions
        {
            None,
            Stop_Sold,
            Opened_For_Sale,
            Closed_To_Arrival,
            Open_To_Arrival,
            Closed_To_Deaprture,
            Open_To_Departure
        }
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
 
        public sealed class ReservationError
        {
            public OTA_ERR err { get; set; }
            public OTA_EWT ewt { get; set; }
            public string errorText { get; set; }
            public ReservationError(OTA_EWT ewtOTA, OTA_ERR errOTA, string errText)
            {
                err = errOTA;
                ewt = ewtOTA;

                //
                // Since this text is going into an XML node, invalid chars must be escaped with XML entities.
                //

                string xmlText = errText ?? errOTA.ToString().Replace("_", " ");
                errorText = xmlText.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            }
        }
        public sealed class AvailStatusMessages
        {
            public sealed class AvailStatusMessage
            {
                public sealed class StatusApplicationControl
                {
                    public sealed class DestinationSystemCodes
                    {
                        public sealed class DestinationSystemCode
                        {
                            public string innerText { get; private set; }
                        }

                        public List<DestinationSystemCode> DestinationSystemCodeNodeList { get; private set; }

                        public DestinationSystemCodes(List<DestinationSystemCode> destinationSystemCodeNodeList)
                        {
                            DestinationSystemCodeNodeList = destinationSystemCodeNodeList;
                        }
                    }

                    public string Start { get; private set; }
                    public string End { get; private set; }
                    public string RatePlanCode { get; private set; }
                    public string InvTypeCode { get; private set; }
                    public string Mon { get; private set; }
                    public string Tue { get; private set; }
                    public string Weds { get; private set; }
                    public string Thur { get; private set; }
                    public string Fri { get; private set; }
                    public string Sat { get; private set; }
                    public string Sun { get; private set; }
                    public DestinationSystemCodes DestinationSystemCodesNode { get; private set; }

                    public StatusApplicationControl(DateTime start, 
                                                    DateTime end, 
                                                    string ratePlanCode, 
                                                    string invTypeCode, 
                                                    List<DestinationSystemCodes.DestinationSystemCode> destinationSystemCodeList,
                                                    bool mon = true, bool tue = true, bool weds = true, bool thur = true, bool fri = true, bool sat = true, bool sun = true)
                    {
                        if (DateTime.Compare(start, end) > 0 || (end - DateTime.Today).TotalDays > 400)
                        {
                            throw new Exception("StatusApplicationControl: invalid dates.");
                        }
                        Start = start.ToString("yyyy-MM-dd");
                        End = end.ToString("yyyy-MM-dd");
                        RatePlanCode = ratePlanCode;
                        InvTypeCode = invTypeCode;
                        Mon = mon ? "1" : "0";
                        Tue = tue ? "1" : "0";
                        Weds = weds ? "1" : "0";
                        Thur = thur ? "1" : "0";
                        Fri = fri ? "1" : "0";
                        Sat = sat ? "1" : "0";
                        Sun = sun ? "1" : "0";

                        if (destinationSystemCodeList != null)
                        {
                            DestinationSystemCodesNode = new DestinationSystemCodes(destinationSystemCodeList);
                        }
                    }
                }

                public sealed class RestricitionStatus
                {
                    public string Restriction { get; private set; }
                    public string Status { get; private set; }

                    public RestricitionStatus(Restrictions restrictions)
                    {
                        switch (restrictions)
                        {
                            case Restrictions.None:
                                Restriction = null;
                                Status = null;
                                break;

                            case Restrictions.Stop_Sold:
                                Restriction = null;
                                Status = "Close";
                                break;

                            case Restrictions.Opened_For_Sale:
                                Restriction = null;
                                Status = "Open";
                                break;

                            case Restrictions.Closed_To_Arrival:
                                Restriction = "Arrival";
                                Status = "Close";
                                break;

                            case Restrictions.Open_To_Arrival:
                                Restriction = "Arrival";
                                Status = "Open";
                                break;

                            case Restrictions.Closed_To_Deaprture:
                                Restriction = "Departure";
                                Status = "Close";
                                break;

                            case Restrictions.Open_To_Departure:
                                Restriction = "Departure";
                                Status = "Open";
                                break;

                        }
                    }
                }

                public sealed class LengthsOfStay
                {
                    public sealed class LengthOfStay
                    {
                        public string MinMaxMessageType { get; private set; }
                        public string Time { get; private set; }

                        public LengthOfStay(string minMaxMessageType, string time)
                        {
                            MinMaxMessageType = minMaxMessageType;
                            Time = time;
                        }
                    }

                    public List<LengthOfStay> LengthOfStayNodeList { get; private set; }

                    public LengthsOfStay(int minTime, int maxTime)
                    {
                        //
                        // Set minTime OR maxTime to 0 if not used; otherwise, at least one of them must be in the range of 1 to 999.
                        // minStay = 1 - no minimum stay requirement.
                        // maxStay = 999 - no maximum stay requirement.
                        //

                        if ((minTime <= 0 || minTime > 999) && (maxTime <= 0 || maxTime > 999))
                        {
                            throw new Exception("LengthsOfStay: invalid arguments.");
                        }

                        if (minTime > 1 && (maxTime < 999 && maxTime >= 1) && minTime > maxTime)
                        {
                            throw new Exception("LengthsOfStay: minTime can't be greater than maxTime.");
                        }
                        

                        LengthOfStayNodeList = new List<LengthOfStay>();

                        if (minTime > 0)
                        {
                            LengthOfStay lengthOfStay = new LengthOfStay("SetMinLOS", minTime.ToString());
                            LengthOfStayNodeList.Add(lengthOfStay);
                        }

                        if (maxTime > 0)
                        {
                            LengthOfStay lengthOfStay = new LengthOfStay("SetMaxLOS", maxTime.ToString());
                            LengthOfStayNodeList.Add(lengthOfStay);
                        }

                    }
                }

                public string BookingLimit { get; private set; }
                public LengthsOfStay LengthsOfStayNode { get; private set; }
                public RestricitionStatus RestricitionStatusNode { get; private set; }
                public StatusApplicationControl StatusApplicationControlNode { get; private set; }

                public AvailStatusMessage(StatusApplicationControl statusApplicationControl, Restrictions restricitions)
                {
                    BookingLimit = null;
                    RestricitionStatusNode = new RestricitionStatus(restricitions);
                    StatusApplicationControlNode = statusApplicationControl;
                }

                //
                // Optional minimum and maxumum lengths of stay specified by minTime and maxTime.
                //

                public AvailStatusMessage(StatusApplicationControl statusApplicationControl, Restrictions restricitions, int minTime, int maxTime)
                {
                    BookingLimit = null;    
                    LengthsOfStayNode = new LengthsOfStay(minTime, maxTime);
                    RestricitionStatusNode = new RestricitionStatus(restricitions);
                    StatusApplicationControlNode = statusApplicationControl;              
                }

                //
                // Set availability by specifing the booking limit.
                //

                public AvailStatusMessage(StatusApplicationControl statusApplicationControl, Restrictions restricitions, int bookingLimit)
                {
                    if(bookingLimit <= 0)
                    {
                        throw new Exception("AvailStatusMessage: bookingLimit must be a positive integer.");
                    }

                    if(statusApplicationControl.DestinationSystemCodesNode != null)
                    {
                        throw new Exception("BookingLimit may not be used with DestinationSystemCodesNode because it is not possible to update availability per channel.");
                    }

                    BookingLimit = bookingLimit.ToString();
                    RestricitionStatusNode = new RestricitionStatus(restricitions);
                    StatusApplicationControlNode = statusApplicationControl;
                }
            }

            public string HotelCode { get; private set; }
            public List<AvailStatusMessage> AvailStatusMessageNodeList { get; private set; }

            public AvailStatusMessages(string hotelCode, List<AvailStatusMessage> availStatusMessageList)
            {
                HotelCode = hotelCode;
                AvailStatusMessageNodeList = availStatusMessageList;
            }
        }

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

        static public async Task<HotelAvailNotifRQResponse> OTA_HotelAvailNotifRQ(string pmsID, string usernameAuthenticate, string passwordAuthenticate)
        {
            HotelAvailNotifRQResponse response = null;

            PmsXchangeServiceClient service = new AsyncServiceConnection().service;

            try
            {
                OTA_HotelAvailNotifRQ body = new OTA_HotelAvailNotifRQ();
                body.Version = 1.0M;
                body.EchoToken = Guid.NewGuid().ToString();  // Echo token must be unique.            
                body.TimeStamp = DateTime.Now;
                body.TimeStampSpecified = true;
                body.POS = CreatePOS(pmsID);

            }
            catch(Exception ex)
            {
                response = new HotelAvailNotifRQResponse();
                response.OTA_HotelAvailNotifRS = new MessageAcknowledgementType();
                response.OTA_HotelAvailNotifRS.Items = new object[] { ProcessingException(ex) };
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
