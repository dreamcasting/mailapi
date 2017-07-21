Mail API
============

This is the Mail API. It is a work in progress. This system will generate an API Key for you to use, then accept a JSON formatted string to send emails.

Setup
-------------------
This application is set up for deployment in Visual Studio 2017.

Creating the API Key
--------------------

1. Navigate to the API page, currently where you set it in Visual Studio and click "Create API Key".
2. Enter the name of your application (write it down EXACTLY as you put it in, you'll need it later).
3. Click the "Create API Key" button.
4. The system will display your key. **Once you leave the screen it will not be able to be shown again! Carefully note this key!**
5. Update your system to process JSON strings and send them to http://address:99/Message/.
6. The API will return a "true" string or a failure message string, which your system can consume as needed.

**Note 1:** Messages are sent from *applicationname*help@domain.com.

**Note 2:** The system only accepts one attachment per email.

JSON String Specification
-------------------------
The following is the JSON string specification. Requests to the API should be formatted in this way:
```json
{
    "AssociatedApplication":"<ApplicationNameAsYouTypedIt>",
    "Key":"<KeyProvided>", 
    "Subject":"<Subject>", 
    "Body":"<SafelyEscapedHTMLBody>",
    "EmailAddresses":
    [
        {"From":"<email>"},
        {"To":"<email>"},
        {"To":"<email>"},
        {"Cc":"<email>"},
        {"Cc":"<email>"},
        {"Bcc":"<email>"}
    ],
    "AttachmentName" : "<TheFileNameWithExtension>",
    "AttachmentContent":"<ABase64EncodedString - Encode your Attachment!>"                                                
}
```
***HTML should be safely escaped prior to sending to the API!*** *You may omit the "AttachmentName" and "AttachmentContent" when not sending an attachment.*

Pending Enhancements
--------------------
The system will be implementing security to control key revocation.
