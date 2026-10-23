// Predrag Živić
// 20.05.2024
// Pozivanje forme za Import dokumenata u OnBase

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class frmDocumentImport : Form
    {
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
            var clientId = "ALGOTECH_REST_CLIENT_ID";
            var clientSecret = "ALGOTECH_REST_SECRET";

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
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<OAuthTokenResponse>(responseBody);

                    if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                    {
                        throw new Exception("OAuth2 access token nije dobijen.");
                    }

                    return tokenResponse.AccessToken;
                }
            }
        }
        #endregion

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

            comboBox1.Items.Add(@"http://localhost/AlgotechUnityREST-ckb/Api/Documents/DocumentImport");
            comboBox1.Items.Add(@"http://sbonbase.algotech.test/AlgotechUnityREST-ckb/Api/Documents/DocumentImport");
            //comboBox1.Items.Add(@"http://sbonbase.algotech.test:84/Api/Documents/DocumentImport");
            comboBox1.SelectedIndex = 0;
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
            Cursor = Cursors.WaitCursor;

            toolStripStatusLabel1.Text = string.Empty;
            txtResults.Text = string.Empty;

            var task = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse1.Text));

            toolStripStatusLabel1.Text = toolStripStatusLabel1.Text + " TaskID: " + task.Id.ToString() + " Status: " + task.Status.ToString();
            Cursor = Cursors.Default;
        }

        #endregion

        #region butSubmitByCounter_Click
        /// <summary>
        /// Poziva DocumentImportToOnBaseAsync() više puta u petlji, broj poziva se određuje iz txtCounter.Text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void butSubmitByCounter_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                toolStripStatusLabel1.Text = string.Empty;
                txtResults.Text = string.Empty;

                int counter = Convert.ToInt32(txtCounter.Text);
                int requestId = Convert.ToInt32(txtResponse1.Text);

                for (int i = 0; i < counter; i++)
                {
                    toolStripStatusLabel1.Text =
                        "Poziv: " + (i + 1).ToString();

                    await DocumentImportToOnBaseAsync(requestId + i);
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

        private async Task DocumentImportToOnBaseAsync(long requestID)
        {
            var url = comboBox1.Text;

            // Pribavljanje OAuth2 access token-a
            var accessToken = await GetAccessTokenAsync();

            // Body
            var list = new List<string>
            {
                @"D:\Temp\OnBase\Upload\dummy-file.pdf"
            };

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

            // Serijalizacija u JSON format
            var json = JsonConvert.SerializeObject(data);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(300);

                    // Header
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                    client.DefaultRequestHeaders.Add("Set-Cookie", string.Format("Session=ID={0}&RequestID={1}", Guid.NewGuid(), requestID));

                    // OAuth2 Bearer autorizacija
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // Asinhroni poziv
                    using (HttpResponseMessage response = await client.PostAsync(url, content))
                    {
                        response.EnsureSuccessStatusCode();

                        var responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(response.Headers.ETag);
                        Console.WriteLine(response.Headers.Location);
                        Console.WriteLine(responseBody);

                        txtResults.Text = txtResults.Text + responseBody + "\r\n";

                        // Deserijalizacija JSON objekta
                        var documentsImportResult = JsonConvert.DeserializeObject<DocumentsImportResult>(responseBody);

                        txtRequestID.Text = documentsImportResult.RequestID.ToString();
                        txtDocBarcode.Text = documentsImportResult.DocBarcode;
                        txtMessage.Text = documentsImportResult.Message;

                        if (data.RequestID.ToString() != txtRequestID.Text)
                        {
                            MessageBox.Show(string.Format("Greška različit RequestID {0} {1}.", data.RequestID, txtRequestID.Text), "Greška");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Request error: " + ex.Message);
                    txtResults.Text = "Request error: " + ex.Message;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error: " + ex.Message);
                    txtResults.Text = "Unexpected error: " + ex.Message;
                }
            }
        }
        #endregion

        #region Main
        private async Task Simple()
        {
            // URL ka REST servisu
            var url = comboBox1.Text;

            // Podaci koje želimo poslati
            var data1 = new
            {
                Name = "John Doe",
                Email = "john.doe@example.com"
            };

            var data2 = new
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com"
            };

            // Serializacija podataka u JSON format
            var json1 = Newtonsoft.Json.JsonConvert.SerializeObject(data1);
            var json2 = Newtonsoft.Json.JsonConvert.SerializeObject(data2);
            var content1 = new StringContent(json1, Encoding.UTF8, "application/json");
            var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                try
                {
                    // Kreiranje dva asinhrona POST zahteva
                    var postTask1 = client.PostAsync(url, content1);
                    var postTask2 = client.PostAsync(url, content2);

                    // Čekanje da oba zahteva budu završena
                    var responses = await Task.WhenAll(postTask1, postTask2);

                    // Provera rezultata oba zahteva
                    foreach (var response in responses)
                    {
                        response.EnsureSuccessStatusCode();
                        var responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Uspešan odgovor: " + responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Došlo je do greške: " + e.Message);
                }
            }
        }
        #endregion

        #region button1_Click_1
        /// <summary>
        /// Poziva DocumentImportToOnBaseAsync() tri puta u isto vreme, sa različitim RequestID vrednostima
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                toolStripStatusLabel1.Text = string.Empty;
                txtResults.Text = string.Empty;

                txtResponse1.Text = (Convert.ToInt32(txtResponse1.Text) + 3).ToString();
                txtResponse2.Text = (Convert.ToInt32(txtResponse2.Text) + 3).ToString();
                txtResponse3.Text = (Convert.ToInt32(txtResponse3.Text) + 3).ToString();
                var task1 = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse1.Text));
                var task2 = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse2.Text));
                var task3 = DocumentImportToOnBaseAsync(Convert.ToInt32(txtResponse3.Text));

                // Use the Task.WhenAll method to execute multiple tasks concurrently.
                Task.WhenAll(task1, task2, task3);
                toolStripStatusLabel1.Text = task1.Status.ToString() + " " + task2.Status.ToString() + " " + task3.Status.ToString();

                txtCounter.Text = (Convert.ToInt16(txtCounter.Text) + 1).ToString();
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

        #region butUpload_Click
        //private void butUpload_Click(object sender, EventArgs e)
        //{
        //    using (var openFileDialog = new OpenFileDialog())
        //    {
        //        openFileDialog.Title = "Izaberite fajl za upload";

        //        if (openFileDialog.ShowDialog() != DialogResult.OK)
        //            return;

        //        string fullPath = openFileDialog.FileName;

        //        var baseUrl = "https://sbonbase.algotech.test/";
        //        var uploadEndpoint = "AlgotechUnityREST-ckb/Api/Upload/PostFormData";

        //        using (var httpClient = new HttpClient())
        //        using (var form = new MultipartFormDataContent())
        //        {
        //            httpClient.BaseAddress = new Uri(baseUrl);

        //            var fileContent = new ByteArrayContent(File.ReadAllBytes(fullPath));
        //            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

        //            form.Add(fileContent, "file", Path.GetFileName(fullPath));

        //            var response = httpClient.PostAsync(uploadEndpoint, form).Result;
        //            var result = response.Content.ReadAsStringAsync().Result;

        //            if (response.IsSuccessStatusCode)
        //            {
        //                toolStripStatusLabel1.Text = result.Replace("\"", string.Empty);
        //            }
        //            else
        //            {
        //                MessageBox.Show("Greška: " + response.StatusCode + " - " + result);
        //            }
        //        }
        //    }
        //}

        private void butUpload_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Izaberite fajl za upload";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string fullPath = openFileDialog.FileName;
                string accessToken = GetAccessToken();

                var baseUrl = "https://sbonbase.algotech.test/";
                var uploadEndpoint = "AlgotechUnityREST-demo/api/Upload/PostFile";

                using (var httpClient = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    httpClient.BaseAddress = new Uri(baseUrl);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var fileContent = new ByteArrayContent(File.ReadAllBytes(fullPath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                    form.Add(fileContent, "file", Path.GetFileName(fullPath));

                    var response = httpClient.PostAsync(uploadEndpoint, form).Result;
                    var result = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        toolStripStatusLabel1.Text = result.Replace("\"", string.Empty);
                    }
                    else
                    {
                        MessageBox.Show("Greška: " + response.StatusCode + " - " + result);
                    }
                }
            }
        }
        #endregion
        private string GetAccessToken()
        {
            var tokenUrl = "https://sbonbase.algotech.test/AlgotechUnityREST-ckb/oauth2/token";
            var clientID = "Algotech_TEST";
            var clientSecret = "FLqPf1JTqGmT+/DtVapinomm0VOgIbuiL630SCnnLAY=";

            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]               {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
            });

                var response = httpClient.PostAsync(tokenUrl, content).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Greška pri pribavljanju access token-a: " + response.StatusCode + " - " + result);

                var token = JsonConvert.DeserializeObject<OAuthTokenResponse>(result);

                if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
                    throw new Exception("Access token nije vraćen.");

                return token.AccessToken;
            }
        }
    }
}
