// Predrag Živić
// 20.0.5.2024
// Pozivanje forme za Import dokumenata u OnBase

using System;

using System.Net.Http;
using System.Text;

using System.Windows.Forms;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace WindowsFormsApp
{
    public partial class frmDocumentImport : Form
    {
        #region frmDocumentImport
        public frmDocumentImport()
        {
            InitializeComponent();
        }
        #endregion

        #region Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {
            //toolStripStatusLabel1.Visible= false;
            toolStripStatusLabel1.Text = "";

            //txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Request/SetStatus";
            //txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Documents/SetDocumentKeyword?requestID=202304031";

            txtUrl.Text = @"http://localhost/AlgotechUnityREST-ckb/Api/Documents/DocumentImport";
        }
        #endregion

        #region Klasa DocumentImport
        public class DocumentImport
        {

            public long? RequestID { get; set; }
            public DateTime DocumentDate { get; set; }
            public string DocumentTypeName { get; set; }

            public string GroupType { get; set; }
            public string GroupID { get; set; }
            public string DocID { get; set; }
            public string ClientID { get; set; }
            public string ClientName { get; set; }
            public string GroupBarcode { get; set; }
            public string DataSource { get; set; }
            public string DocStatus { get; set; }
            public string DefaultFileTypeName { get; set; }
            public string OrganizationalUnit { get; set; }
            public string UserName { get; set; }

            public List<string> FileList { get; set; }
        }
        #endregion

        #region DocumentsImportResult
        private class DocumentsImportResult
        {
            public long RequestID { get; set; }
            public string DocBarcode { get; set; }
            public string GroupBarcode { get; set; }
            public string Message { get; set; }
        }
        #endregion

        #region butSubmit_Click
        private void butSubmit_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Empty;
            txtResults.Text = string.Empty;

            var task = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse1.Text));

            toolStripStatusLabel1.Text = toolStripStatusLabel1.Text + " TaskID: " + task.Id.ToString() + " Status: " +  task.Status.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                toolStripStatusLabel1.Text = string.Empty;
                txtResults.Text = string.Empty;

                //Task task1 = Task.Run(() => DocumentImportToOnBaseAsync(2002));
                //Task task2 = Task.Run(() => DocumentImportToOnBaseAsync(2004));
                //Task task3 = Task.Run(() => DocumentImportToOnBaseAsync(2006));

                txtResponse1.Text = (Convert.ToInt32(txtResponse1.Text) + 2).ToString();
                txtResponse2.Text = (Convert.ToInt32(txtResponse2.Text) + 2).ToString();

                var task1 = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse1.Text));
                var task2 = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse2.Text));

                //var task3 = DocumentImportToOnBaseAsync(2006);

                // Use the Task.WhenAll method to execute multiple tasks concurrently.
                // Čekao da se tavrše svi zahtevi
                //Task.WhenAll(task1, task2);

                toolStripStatusLabel1.Text = task1.Status.ToString() + " " + task2.Status.ToString();

                txtCounter.Text = (Convert.ToInt16(txtCounter.Text) + 1).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška");
            }

        }
        #endregion

        #region 1
        private void butSubmitByCounter_Click(object sender, EventArgs e)
        {
            try
            {
                toolStripStatusLabel1.Text = string.Empty;
                txtResults.Text = string.Empty;

                Task task;
                for (int i = 0; i < Convert.ToInt16(txtCounter.Text); i++)
                {
                    task = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse1.Text) + i);
                    toolStripStatusLabel1.Text = string.Format("Broj poziva: {0}.", i);
                }

                toolStripStatusLabel1.Text = toolStripStatusLabel1.Text + " *** završeno!";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška");
            }
        }
        #endregion

        #region DocumentImport
        private async Task DocumentImportToOnBaseAsync(long requestID)
        {
            var url = txtUrl.Text.Trim();

            // Body
            var list = new List<string> { @"D:\Temp\OnBase\Upload\dummy-file.pdf" };
            var data = new DocumentImport()
            {
                RequestID = requestID,
                DocumentDate = DateTime.Now,
                DocumentTypeName = "NETBANK",
                GroupType = "EBANKING",
                GroupID = "BIIS;KLS;6;598",
                DocID = "BIIS;KLS;6;598",
                ClientID = "06121983723229",
                ClientName = "Jovan Jovanović",
                GroupBarcode = "F0456000001595",
                DataSource = "BS",
                DocStatus = "2 - SKENIRANO",
                DefaultFileTypeName = "PDF",
                OrganizationalUnit = "CKB",
                UserName = "pzivic",
                FileList = list
            };

            // Serijalizacija u JSon format
            var json = JsonConvert.SerializeObject(data);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                try
                {
                    // Header
                    //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


                    // Dodavanje zaglavlja
                    //client.DefaultRequestHeaders.Accept.Clear();
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                    //client.DefaultRequestHeaders.Add("Set-Cookie", string.Format("Session=ID={0}&RequestID={1}", Guid.NewGuid(), requestID));

                    // Autorizacija
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    //Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password))));

                    //// Asinhroni poziv 1, način await se koristi da bi se sačekao odgovor bez blokiranja niti 
                    //var response = await client.PostAsync(url, content);
                    //response.EnsureSuccessStatusCode();
                    //var responseBody = await response.Content.ReadAsStringAsync();
                    //txtResults.Text = txtResults.Text + responseBody + "\r\n";
                    ////.

                    // Asinhroni poziv 2 način, await se koristi da bi se sačekao odgovor bez blokiranja niti
                    using (HttpResponseMessage response = client.PostAsync(url, content).Result)
                    {
                        response.EnsureSuccessStatusCode();
                        var responseBody = await response.Content.ReadAsStringAsync();

                        txtResults.Text = txtResults.Text + responseBody + "\r\n";
                        Console.WriteLine(responseBody);

                        // Deserijalizacija json objekata
                        var documentsImportResult = JsonConvert.DeserializeObject<DocumentsImportResult>(responseBody);
                        txtRequestID.Text = documentsImportResult.RequestID.ToString();
                        txtDocBarcode.Text = documentsImportResult.DocBarcode;
                        txtMessage.Text = documentsImportResult.Message;
                        //.
                    }
                    //.

                    //// Asinhroni poziv 3 način
                    //using (HttpResponseMessage response = await client.PostAsync(url, content))
                    //{
                    //    response.EnsureSuccessStatusCode();
                    //    var responseBody = await response.Content.ReadAsStringAsync();
                    //    txtResults.Text = txtResults.Text + responseBody + "\r\n";
                    //    Console.WriteLine(responseBody);
                    //}

                    //// Ovaj primer neće hvatati u UI txtResults.Text ali radi brzo
                    //using (HttpResponseMessage response = await client.PostAsync(url, content).ConfigureAwait(false))
                    //{
                    //    response.EnsureSuccessStatusCode();
                    //    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //    Console.WriteLine("responseBody: " + responseBody);

                    //    // Deserijalizacija json objekata
                    //    var documentsImportResult = JsonConvert.DeserializeObject<DocumentsImportResult>(responseBody);
                    //    txtRequestID.Text = documentsImportResult.RequestID.ToString();
                    //    txtDocBarcode.Text = documentsImportResult.DocBarcode;
                    //    txtMessage.Text = documentsImportResult.Message;
                    //    //.
                    //}



                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                    txtResults.Text = $"Request error: {ex.Message}"; ;
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    txtResults.Text = $"Unexpected error: {ex.Message}";
                }
            }
        }

        #endregion
    }
}
