
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static WindowsFormsApp.frmDocumentImport;

namespace WindowsFormsApp
{
    /// <summary>
    /// Dokumentacija - REST servis
    /// </summary>
    public partial class frmDocuments : Form
    {

        #region frmDocuments
        public frmDocuments()
        {
            InitializeComponent();

            txtDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            txtDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

            comboBoxDocumentType.Items.AddRange(new object[]
                {
                    "LSG - Activation Invoice",
                    "LSG - Credit Bureau Consent Form (1)",
                    "LSG - Credit Bureau Report",
                    "LSG - Delivery Report",
                    "LSG - Financial Leasing Agreement",
                    "LSG - Financial Statements",
                    "LSG - Financing Approval Decision",
                    "LSG - General Terms and Conditions (4)",
                    "LSG - Guarantee Agreement (2)",
                    "LSG - Internal Credit Opinion",
                    "LSG - Personal Identification Documents",
                    "LSG - Proforma Invoice (5)",
                    "LSG - Promissory Note Authorization (3)",
                    "LSG - Proof of Income",
                    "LSG - Registration Application with the SBRA",
                    "LSG - Registration Authorization",
                    "LSG - Request Form - Individuals",
                    "LSG - Risk Scoring Sheet",
                    "INS - Authorization Letter",
                    "INS - Damage assessment report",
                    "INS - EU Traffic Accident Report",
                    "INS - Insurance Claim",
                    "INS - Insurance policy document",
                    "INS - MUP Traffic Accident Report",
                    "INS - Personal Identification Documents",
                    "INS - Request to file a travel insurance claim",
                    "INS - Vehicle Registration Document"
                });

            //comboBoxDocumentType.SelectedIndex = 0;
            comboBoxDocumentType.SelectedItem = "INS - Personal Identification Documents";

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;

            listView1.Columns.Add("Document ID", 100);
            listView1.Columns.Add("Document Type", 280);
            listView1.Columns.Add("Document Date", 100);
            listView1.Columns.Add("Client Name", 180);
            listView1.Columns.Add("Client AK", 100);
            listView1.Columns.Add("File Type", 80);
        }
        #endregion

        #region Klase za deserializaciju JSON odgovora
        public class DocumentListResponse
        {
            public List<DocumentInfo> documents { get; set; }
        }

        public class DocumentInfo
        {
            public string DocumentTypeName { get; set; }
            public DateTime DocumentDate { get; set; }
            public string DefaultFileTypeName { get; set; }
            public long DocumentID { get; set; }
            public string Name { get; set; }
            public DateTime DateStored { get; set; }
            public string DocumentFullPath { get; set; }
            public string DocumentUrlByID { get; set; }
            public int RevisionID { get; set; }
            public DateTime RevisionDate { get; set; }
            public KeywordList KeywordList { get; set; }
        }

        public class KeywordList
        {
            public string ClientName { get; set; }
            public string Address { get; set; }
            public string PhoneNum { get; set; }
            public string EMail { get; set; }
            public string DataSource { get; set; }
            public string ClientAK { get; set; }
        }
        private class DocumentsImportResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
        }

        #endregion


        public class DocumentImportRequest
        {
            public string DocumentTypeName { get; set; }
            public string DocumentDate { get; set; }
            public string DefaultFileTypeName { get; set; }
            public string DataSource { get; set; }
            public List<string> FileName { get; set; }
            public List<DocumentKeyword> Keywords { get; set; }
        }

        public class DocumentKeyword
        {
            public string KeywordName { get; set; }
            public object KeywordValue { get; set; }
        }

        #region OAuth2
        /// <summary>
        /// Token response class for deserializing the OAuth2 token response.
        /// </summary>
        public class OAuthTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        /// <summary>
        /// Dobijanje OAuth2 access token-a za autentifikaciju prema REST servisu.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> GetAccessTokenAsync()
        {
            var tokenUrl = "http://localhost/AlgotechUnityREST-ckb/oauth2/token";
            var clientId = "Algotech_DEV";
            var clientSecret = "F861F83BE9F64F91B5B8FDE9C67406F410DC440DDE68496D892E8B3EB9464B93";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret }
                };

                using (var content = new FormUrlEncodedContent(parameters))
                using (var response = await client.PostAsync(tokenUrl, content))
                {

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<OAuthTokenResponse>(responseBody);

                    response.EnsureSuccessStatusCode();

                    if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                    {
                        throw new Exception("OAuth2 access token nije dobijen.");
                    }

                    return tokenResponse.AccessToken;
                }
            }
        }
        #endregion

        #region butSubmit_Click
        private async void butSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                toolStripStatusLabel1.Text = string.Empty;

                string baseUrl = "http://localhost";
                string accessToken = await GetAccessTokenAsync();

                DocumentListResponse result = await GetDocumentList(baseUrl, accessToken);

                if (result?.documents != null)
                {
                    listView1.Items.Clear();

                    foreach (DocumentInfo document in result.documents)
                    {
                        var item = new ListViewItem(document.DocumentID.ToString());

                        item.SubItems.Add(document.DocumentTypeName);
                        item.SubItems.Add(document.DocumentDate.ToString("dd.MM.yyyy"));
                        item.SubItems.Add(document.KeywordList?.ClientName ?? string.Empty);
                        item.SubItems.Add(document.KeywordList?.ClientAK ?? string.Empty);
                        item.SubItems.Add(document.DocumentFullPath ?? string.Empty);

                        listView1.Items.Add(item);
                    }

                    toolStripStatusLabel1.Text =
                        $"Broj pronađenih dokumenata: {result.documents.Count}";
                }
                else
                {
                    toolStripStatusLabel1.Text = "Nema dokumenata.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Greška",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        #endregion

        #region Task
        /// <summary>
        /// Preuzimanje podataka o dokumentima iz REST servisa.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DocumentListResponse> GetDocumentList(
            string baseUrl,
            string accessToken)
        {
            string fromDate = txtDateFrom.Text; // "2024-09-24";
            string toDate = txtDateTo.Text; // "2024-09-24";
            string documentTypeName = comboBoxDocumentType.SelectedItem?.ToString() ?? string.Empty;

            string url =
                baseUrl.TrimEnd('/') +
                "/AlgotechUnityREST-ckb/api/DocumentsUniversal/DocumentList" +
                "?fromDate=" + Uri.EscapeDataString(fromDate) +
                "&toDate=" + Uri.EscapeDataString(toDate) +
                "&documentTypeName=" + Uri.EscapeDataString(documentTypeName);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(url);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"DocumentList error {(int)response.StatusCode} " +
                        $"{response.ReasonPhrase}: {json}");
                }

                return JsonConvert.DeserializeObject<DocumentListResponse>(json);
            }
        }
        #endregion

        #region butDocumentImport_Click
        private async void butDocumentImport_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                string baseUrl = "http://localhost";
                string accessToken = await GetAccessTokenAsync();

                toolStripStatusLabel1.Text = string.Empty;
                var esults = string.Empty;

                int counter = Convert.ToInt32(txtCounter.Text);
                int requestId = Convert.ToInt32(txtResponse.Text);

                for (int i = 0; i < counter; i++)
                {
                    toolStripStatusLabel1.Text =
                        "Poziv: " + (i + 1).ToString();

                    await DocumentImportToOnBaseAsync(baseUrl, accessToken, requestId + i);
                }

                toolStripStatusLabel1.Text =
                    "Završeno! Ukupno poziva: " + counter;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        #endregion


        #region DocumentImport

        private async Task DocumentImportToOnBaseAsync(string baseUrl, string accessToken, long requestID)
        {
            // Kreiranje objekta za serijalizaciju u JSON
            var data = new DocumentImportRequest
            {
                DocumentTypeName = "INS - Personal Identification Documents",
                DocumentDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                DefaultFileTypeName = "PDF",
                DataSource = "LS",

                FileName = new List<string> { @"D:\Temp\OnBase\Upload\lorem-file.png"},
                Keywords = new List<DocumentKeyword>
                {
                    new DocumentKeyword
                    {
                        KeywordName = "ClientAK",
                        KeywordValue = 12321111
                    },
                    new DocumentKeyword
                    {
                        KeywordName = "ClientName",
                        KeywordValue = "Petar Petrović"
                    },
                    new DocumentKeyword
                    {
                        KeywordName = "PhoneNum",
                        KeywordValue = "+381064123456"
                    },
                    new DocumentKeyword
                    {
                        KeywordName = "Address",
                        KeywordValue = "Kneza Miloša bb"
                    },
                    new DocumentKeyword
                    {
                        KeywordName = "EMail",
                        KeywordValue = "petar.petrovic@algotech.rs"
                    },
                    //new DocumentKeyword
                    //{
                    //    KeywordName = "LSG_RequestNo",
                    //    KeywordValue = "123-"+requestID.ToString()
                    //}
                }
            };
            //.

            // Serijalizacija u JSON format
            var json = JsonConvert.SerializeObject(data);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(300);

                    string url = baseUrl.TrimEnd('/') + "/AlgotechUnityREST-ckb/Api/DocumentsUniversal/DocumentImport";

                    // Header
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                    //client.DefaultRequestHeaders.Add("Set-Cookie", string.Format("Session=ID={0}", Guid.NewGuid()));

                    // OAuth2 Bearer autorizacija
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // Asinhroni poziv
                    using (HttpResponseMessage response = await client.PostAsync(url, content))
                    {
                        string responseBody =
                            await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception(
                                "DocumentImport error " +
                                (int)response.StatusCode + " " +
                                response.ReasonPhrase +
                                Environment.NewLine +
                                responseBody);
                        }

                        var DocumentsImportResult =
                            JsonConvert.DeserializeObject<DocumentsImportResult>(
                                responseBody);

                        // DocumentsImportResult.Status
                        MessageBox.Show(
                            DocumentsImportResult.Message,
                            "DocumentImport",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Request error: " + ex.Message);
                    //txtResults.Text = "Request error: " + ex.Message;

                    MessageBox.Show(
                        ex.Message,
                        "Request error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error: " + ex.Message);
                    //txtResults.Text = "Unexpected error: " + ex.Message;

                    MessageBox.Show(
                        ex.Message,
                        "Unexpected error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        private void txtResponse_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
