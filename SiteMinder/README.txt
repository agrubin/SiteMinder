There are two projects in the SiteMinder solution:

1) OTA_Console:  This is a small WPF application which simply exercises the OTA requests to SiteMinder pmsXchange server.
2) pmsXchange:  This generates a DLL containing the pmsXchange API.  Each method call is currently configured as an awaitable task.  However, if
necessary, each method can be modified to be a staright synchronous call.  If you do so, make sure to use the ServiceConnection class instead
of the AsyncServiceConnection.

The OTA_Console has some hardwirted authentication info inside the code-behind file for the main window:
        const string username = "SPIOrangeTest";
        const string password = "ymdqMsjBNutXLQMdVvtJZVXe";
        const string pmsID = "SPIORANGE";
        const string hotelCode = "SPI516";


The button click handlers send an OTA request.  The first one is the simplest, a ping.  Note, since this was done while testing was
in progress, the other handlers are not completely fleshed out...but they are relatively easy to follow.  Also note that I removed some buggy code from the
OTA_HotelRateAmountNotifRQ method in the pmsXchange API.  It will also need to be fleshed out.

SiteMinder provides a WSDL file to generate the proxy classes for their service, therefore the OTA SOAP messages are constructed by filling in .NET c# data structures.
These structures are defined at the beginning og the API.cs file, along with associated constants such as error codes that SiteMinder supports.

Here are all the relevant links.  You can start by creating some test reservations in the receptionist portal and then fetching them using
the OTA_Console.

Documentation links for pmsXchange version 2: 
https://siteminder.atlassian.net/wiki/display/PMSXCHANGEV2/Best+Practices+-+READ+ME+FIRST 
https://siteminder.atlassian.net/wiki/display/PMSXCHANGEV2/Home 
https://siteminder.atlassian.net/wiki/display/PMSXCHANGEV2/Developer+Guide 
FAQs for pmsXchange v2: https://siteminder.atlassian.net/wiki/display/PMSXCHANGEV2/FAQs 

For Project Managers 
https://siteminder.atlassian.net/wiki/display/PMSXCHANGEV2/pmsXchange+Connection+Process 

Details to access the pmsXchange version 2 test service: 
pmsXchange v2 service URL: https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE 
WSDL: https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE/pmsxchange.wsdl 
.NET WSDL: https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE/pmsxchange_flat.wsdl 
Requestor ID: SPIORANGE 
Username: SPIOrangeTest 
Password: ymdqMsjBNutXLQMdVvtJZVXe 
HotelCode: SPI516 

Room List 
(NOTE: The below rooms (including mapping the codes) are simply for the purposes of testing. Hotels will setup their own, fully customised 'Room Rates' in production. There is not a set, hardcoded list of 'Rooms' or 'Rate Plans' in The Channel Manager). pmsXchange can adapt to existing codes with your PMS/CRS/RMS if required. 
Room Type Code = TR 
Room Rate Plan Code = BAR 
Inventory Code = TR 
Room/Rate Description = Twin Room - Best Available Rate 
Room Type Code = TR 
Room Rate Plan = S2S 
Inventory Code = TR 
Room/Rate Description = Twin Room - Stay 2 Nights Deal 
Room Type Code = TR 
Room Rate Plan = NFR 
Inventory Code = TR 
Room/Rate Description = Twin Room - Non-Refundable 
Room Type Code = DR 
Room Rate Plan = BAR 
Inventory Code = DR 
Room/Rate Description = Double Room - Best Available Rate 
Room Type Code = DR 
Room Rate Plan = BB 
Inventory Code = DR 
Room/Rate Description = Double Room - Bed & Breakfast 
Room Type Code = DR 
Room Rate Plan = WKE 
Inventory Code = DR 
Room/Rate Description = Double Room - Weekend Special Deal 
As you are deployed at a property level each property will have its own credentials setup in pmsXchange. 

Details to access the GUI Test Environment 
You have been set up with your own test hotel in our SiteMinder test extranet and a reservation tool called 'Receptionist' to create reservations. The login URLs for both are included below. The login credentials included below will allow access to both URLs. 

Channel Manager: https://cmtpi.siteminder.com/web/login 
AND 
Receptionist - https://cmtpi.siteminder.com/receptionist/login/auth 
Username = spiorangetest 
Password = SPIOrange111 

GUI Test Environment User Guides 
Here is a link explaining how to use our Channel Manager extranet: https://thechannelmanagerhelp.zendesk.com/hc/en-us 
Here is a link explaining how to use Receptionist - https://siteminder.atlassian.net/wiki/display/PMSXCHANGEV2/Receptionist+User+Guide 

Creating Custom Reservations 
You have also been created a test website that will allow you to create your own reservations: 
URL = https://bbtpi.siteminder.com/properties/spiorangetestres 
Note: Please use credit card number 4111111111111111 in order to make a reservation. 
