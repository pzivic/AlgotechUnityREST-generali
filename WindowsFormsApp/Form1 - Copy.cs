using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;

using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace WindowsFormsApp
{
    public partial class frmDocumentImport : Form
    {
        public frmDocumentImport()
        {
            InitializeComponent();
        }

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

        #region  Klasa  Document
        public class Document
        {

            public long? RequestID { get; set; }
            public DateTime DocumentDate { get; set; }
            public string DocumentTypeName { get; set; }
            public List<string> FileList { get; set; }

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
        }

        public class FileList<T>
        {
            private List<T> newList;

            public List<T> NewList
            {
                get { return newList; }
                set { newList = value; }
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
            //txtUrl.Text = @"http://localhost/AlgotechUnityREST/Api/Documents/SetDocumentKeyword?requestID=202304031";
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

        private void butSubmit_Click(object sender, EventArgs e)
        {
            DocumentImport();
        }

        #region DocumentImport
        private void DocumentImport()
        {
            //txtUrl.Text = @"http://localhost/AlgotechUnityREST-ckb/Api/Documents/DocumentImport";
            var url = txtUrl.Text.Trim();
            var client = new HttpClient();

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var list = new List<string> { @"D:\Temp\OnBase\Upload\dummy-file.pdf" };

            var doc = new Document()
            {
                RequestID = 2001,
                DocumentDate = DateTime.Now,
                DocumentTypeName = "SMS",
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
                    " Phrase: " + response.ReasonPhrase +
                    " Content: " + response.RequestMessage.Content +
                    " Poruka servisa: " + response.Content.ReadAsStringAsync().Result);

                var message = response.Content.ReadAsStringAsync().Result;

                var result = MessageBox.Show (message, "Poziv REST servisa",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        #endregion
    }
}
