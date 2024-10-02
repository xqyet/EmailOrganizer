using System;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;

namespace gmail_sorter
{
    class Program
    {
        static void Main(string[] args)
        {
            string imapServer = "imap.gmail.com"; // Gmail IMAP server
            int port = 993;
            string username = "your-email"; // Your email
            string password = "you app-password"; // Your app password

            using (var client = new ImapClient())
            {
                try
                {
                    // Step 1: Connect to the IMAP server
                    client.Connect(imapServer, port, true);

                    // Step 2: Authenticate using the app password
                    client.Authenticate(username, password);

                    // Step 3: Access the inbox folder
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    // Step 4: Create a search query for emails with "test" in the subject, received today
                    var today = DateTime.UtcNow;
                    var query = SearchQuery.SubjectContains("test")
                        .And(SearchQuery.DeliveredAfter(today.Date));

                    // Step 5: Search for matching emails
                    var uids = inbox.Search(query);

                    // Step 6: Locate the "Test" folder
                    IMailFolder testFolder = null;
                    foreach (var personalNamespace in client.PersonalNamespaces)
                    {
                        var folder = client.GetFolder(personalNamespace);
                        testFolder = folder.GetSubfolder("Test");
                        if (testFolder != null)
                        {
                            break;
                        }
                    }

                    // If the folder is not found
                    if (testFolder == null)
                    {
                        Console.WriteLine("Folder 'Test' not found.");
                        return;
                    }

                    // Step 7: Open the "Test" folder
                    testFolder.Open(FolderAccess.ReadWrite);

                    // Step 8: Move the matching emails to the "Test" folder
                    foreach (var uid in uids)
                    {
                        var message = inbox.GetMessage(uid);
                        Console.WriteLine($"Found new message: {message.Subject}");

                        // Move the email
                        inbox.MoveTo(uid, testFolder);
                        Console.WriteLine($"Moved message to 'Test' folder: {message.Subject}");
                    }

                    // Step 9: Disconnect
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            // Pause the console to read the output
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
