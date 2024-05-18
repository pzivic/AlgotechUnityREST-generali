using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;

using Newtonsoft.Json;

namespace WindowsFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {
            //toolStripStatusLabel1.Visible= false;
            toolStripStatusLabel1.Text = "";

            //txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Request/SetStatus";
            txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Documents/SetDocumentKeyword?requestID=202304031";
        }
        #endregion

        #region  Klasa  Document
        public class Document
        {
            /// <summary>
            /// Identifikator dokumenta
            /// </summary>
            public int DocumentID { get; set; }
            /// <summary>
            /// Status dokumenta
            /// </summary>
            public string Status { get; set; }
        }

        #endregion

        #region butSubmit_Click
        private void butSubmit_Click(object sender, EventArgs e)
        {
            txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Request/SetStatus";
            var url = txtUrl.Text.Trim();
            var client = new HttpClient();

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var doc = new Document()
            {
                DocumentID = 1302,
                Status = "APPROVED"
            };

            // Serijalizacija u JSon format
            string obj = JsonConvert.SerializeObject(doc);
            HttpContent content = new StringContent(obj, Encoding.UTF8, "application/json");

            // Pozivanje servisa
            var response = client.PostAsync(url, content).Result;

            // Checking the response is successful or not which is sent using HttpClient
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Uspešno poslato.");

                toolStripStatusLabel1.Text = response.StatusCode.ToString();
                txtResults.Text = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                //Master.SetStatus("error", "Dogodila se greška u sistemu za dvofaktorsku autentifikaciju.");

                Debug.WriteLine("Nije uspešno poslato." +
                    " Status kod: " + response.StatusCode +
                    " Phrase: " + response.ReasonPhrase + "Content: " + response.RequestMessage.Content +
                    " Poruka servisa: " + response.Content.ReadAsStringAsync().Result);
            }
        }
        #endregion

        #region  Klasa  DocumentKeywords
        public class DocumentKeywords
        {
            public int DocumentID { get; set; }
            public string DocumentTypeName { get; set; }
            public string KeywordName { get; set; }
            public string KeywordValue { get; set; }
        }
        #endregion

        #region butSetDocumentKeyword_Click
        private void butSetDocumentKeyword_Click(object sender, EventArgs e)
        {
            txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Documents/SetDocumentKeyword?requestID=202304031";
            var url = txtUrl.Text.Trim();
            var client = new HttpClient();

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var doc = new DocumentKeywords()
            {
                DocumentTypeName = "MIG - Test primer",
                KeywordName = "InsurenceName",
                KeywordValue = "DDOR Novi Sad"
            };

            // Serijalizacija u JSon format
            string obj = JsonConvert.SerializeObject(doc);
            HttpContent content = new StringContent(obj, Encoding.UTF8, "application/json");

            // Pozivanje servisa
            var response = client.PostAsync(url, content).Result;

            // Provera response da li je uspešno ili ne 
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Uspešno poslato.");

                toolStripStatusLabel1.Text = response.StatusCode.ToString();
                txtResults.Text = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                //Master.SetStatus("error", "Dogodila se greška u sistemu za dvofaktorsku autentifikaciju.");

                Debug.WriteLine("Nije uspešno poslato." +
                    " Status kod: " + response.StatusCode +
                    " Phrase: " + response.ReasonPhrase + "Content: " + response.RequestMessage.Content +
                    " Poruka servisa: " + response.Content.ReadAsStringAsync().Result);
            }
        }
        #endregion

    }
}
