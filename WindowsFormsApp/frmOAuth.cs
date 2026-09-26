using System;
using System.Data.SqlClient;
using System.Security.Cryptography;

using DataSystem.DAL.SqlHelper;

using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class frmOAuth : Form
    {
        public class OAuthClient
        {
            public int OAuthClientID { get; set; }
            public string ClientID { get; set; }
            public string ClientSecretHash { get; set; }
            public bool IsActive { get; set; }
            public string Description { get; set; }
            public string AllowedIP { get; set; }
        }

        public frmOAuth()
        {
            InitializeComponent();
        }

        private void frmOAuth_Load(object sender, EventArgs e)
        {
            GenerateOAuthCredentials();
        }

        private void butHash_Click(object sender, EventArgs e)
        {
            GenerateOAuthCredentials();
        }

        private void GenerateOAuthCredentials()
        {
            var clientSecret = GenerateClientSecret();
            var clientSecretHash = HashClientSecret(clientSecret);

            txtClientSecret.Text = clientSecret;
            txtClientHash.Text = clientSecretHash;
        }

        #region GenerateClientSecret
        /// <summary>
        /// Generiše novi kriptografski slučajan klijentski tajni ključ.
        /// </summary>
        /// <returns></returns>
        private static string GenerateClientSecret()
        {
            byte[] buffer = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            return Convert.ToBase64String(buffer);
        }
        #endregion

        #region 
        /// <summary>
        /// Heširanje klijentskog tajnog ključa. Primer 100000.P2xfrh9Es63grzk+fLueug==.pEHfLjUXJXl69mFxPJ3DqEIMwKtn8FyRVzdtAzTfNCQ=
        /// </summary>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        private static string HashClientSecret(string clientSecret)
        {
            byte[] salt = new byte[16];

            using (var Rng = RandomNumberGenerator.Create())
            {
                Rng.GetBytes(salt);
            }

            const int Iterations = 100000;

            using (var Pbkdf2 = new Rfc2898DeriveBytes(
                clientSecret,
                salt,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                byte[] hash = Pbkdf2.GetBytes(32);

                return string.Format(
                    "{0}.{1}.{2}",
                    Iterations,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hash));
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void butCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtClientSecret.Text))
            {
                //Clipboard.SetText("ClientID: " + txtClientID.Text + "\r\nClient secret: " + txtClientSecret.Text);
                Clipboard.SetText(txtClientHash.Text);
                
                MessageBox.Show("Client secret je kopiran u clipboard.", "Informacija", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Logika za upis OAuth klijenta u bazu podataka kada korisnik klikne na dugme OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butOK_Click(object sender, EventArgs e)
        {

            try
            {
                var client = new OAuthClient
                {
                    ClientID = txtClientID.Text.Trim(),
                    ClientSecretHash = txtClientHash.Text.Trim(),
                    IsActive = true,
                    Description = txtDescription.Text.Trim(),
                    AllowedIP = txtAllowedIP.Text.Trim()
                };

                ClientCreate(client);

                MessageBox.Show(
                    "Klijent je uspešno uveden u okruženje REST servisa.",
                    "Informacija",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Greška",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #region ClientCreate
        /// <summary>
        /// Upis OAuth klijenta u bazu podataka.
        /// </summary>
        /// <returns></returns>
        public int ClientCreate(OAuthClient obj)
        {
            if (!IsValidData(obj))
                return -1;

            var sqlHelper = new SqlHelper("connection_string");
            var sqlParameter = new SqlParameter[7];

            sqlParameter[0] = new SqlParameter("@ClientID", obj.ClientID);
            sqlParameter[1] = new SqlParameter("@ClientSecretHash", obj.ClientSecretHash);
            sqlParameter[2] = new SqlParameter("@IsActive", obj.IsActive);
            sqlParameter[3] = new SqlParameter("@Description", obj.Description );
            sqlParameter[4] = new SqlParameter("@AllowedIP", obj.AllowedIP);
            sqlParameter[5] = new SqlParameter("@CreatedDate", DateTime.Now);
            sqlParameter[6] = new SqlParameter("@ModifiedDate", DBNull.Value);

            return Convert.ToInt32(sqlHelper.ExecuteScalar("uspAlgUnity_OAuthClientCreate", sqlParameter));
        }
        #endregion

        #region IsValidData
        /// <summary>
        /// Provera podataka pre upisivanja
        /// </summary>
        /// <returns></returns>
        private bool IsValidData(OAuthClient obj)
        {
            if (obj.ClientID == string.Empty) {throw new Exception("ClientID je obavezan podataka za unos."); }
            if (obj.ClientSecretHash == string.Empty) { throw new Exception("Vrednost ClientSecretHash je obavezan podataka za unos.");}
            return true;
        }
        #endregion

    }
}
