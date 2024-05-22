// Predrag Živić
// 10.04.2023

using System;
using System.ComponentModel;
using System.Linq;
using Hyland.Unity;

namespace Algotech.UTL.AlgotechUnityUDF
{
    public class OnBaseConnection
    {
        #region Enumerator
        public enum Status : short
        {
            InActive = 0,
            Active = 1
        }
        #endregion

        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _appServerURL = System.Configuration.ConfigurationManager.AppSettings["AppServerURL"];
        private readonly string _dataSource = System.Configuration.ConfigurationManager.AppSettings["DataSource"];
        private readonly string _licenceType = System.Configuration.ConfigurationManager.AppSettings["LicenceType"];
        #endregion

        //#region OnBaseConnect
        ///// <summary>
        ///// Konektovanje OnBase sistema
        ///// </summary>
        ///// <returns></returns>
        //public Application OnBaseConnect()
        //{
        //    Application app = null;

        //    // Logijemo od momenta uspostavljanja konekcije
        //    var dateTimeStart = DateTime.Now;
        //    //_logger.Info("Pomoćna funkcija:: OnBaseConnect start " + dateTimeStart);
        //    _logger.Info(string.Format("Pomoćna funkcija:: OnBaseConnect url: {0}, datasource {1}; start: {2}.", _appServerURL, _dataSource, dateTimeStart));

        //    var userName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
        //    var userPassword = System.Configuration.ConfigurationManager.AppSettings["UserPassword"];
        //    OnBaseAuthenticationProperties authProps = Application.CreateOnBaseAuthenticationProperties(_appServerURL, userName, userPassword, _dataSource);

        //    _logger.Info(string.Format("Pokušaj uspostavljanja veze na klasičan način sa identitetom: {0}.", userName));

        //    authProps.IdentitySettings = authProps.CreateIdentitySettings("AlgotechUnityREST");
        //    authProps.LicenseType = LicenseType.Default;

        //    try
        //    {
        //        app = Application.Connect(authProps);
        //    }
        //    catch (InvalidLicensingException)
        //    {
        //        // Ne loguj kao grešku više warning jer nedostaju licence
        //        _logger.Warn(string.Format("Error: Invalid Licensing. Neuspešnо uspostavljanja veze sa identitetom: {0}.", userName));
        //    }

        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.Message);
        //    }

        //    if (app != null)
        //    {
        //        _logger.Info(string.Format("Konekcija je uspešno uspostavljena identitetom {0}. SessionID: {1}.", userName, app.SessionID));
        //    }

        //    _logger.Info("Pomoćna funkcija:: OnBaseConnect end " + DateTime.Now + " sec " + (DateTime.Now - dateTimeStart).TotalSeconds.ToString());

        //    return app;
        //}
        //#endregion


        #region OnBaseConnect
        /// <summary>
        /// Konektovanje OnBase sistema
        /// </summary>
        /// <returns></returns>
        public Application OnBaseConnect()
        {
            Application app = null;

            // Logijemo od momenta uspostavljanja konekcije
            var dateTimeStart = DateTime.Now;
            //_logger.Info("Pomoćna funkcija:: OnBaseConnect start " + dateTimeStart);
            _logger.Info(string.Format("Pomoćna funkcija:: OnBaseConnect url: {0}, datasource {1}; start: {2}.", _appServerURL, _dataSource, dateTimeStart));

            var isAppPoolConnection = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsAppPoolConnection"]);
            //var isDisconnectEnabled = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsDisconnectEnabled"]);

            #region Native Connection
            if (isAppPoolConnection == false)
            {
                var userName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
                var userPassword = System.Configuration.ConfigurationManager.AppSettings["UserPassword"];
                OnBaseAuthenticationProperties authProps = Application.CreateOnBaseAuthenticationProperties(_appServerURL, userName, userPassword, _dataSource);

                authProps.IdentitySettings = authProps.CreateIdentitySettings("AlgotechUnityREST");
                authProps.LicenseType = LicenseType.Default;
                //authProps.LicenseType = LicenseType.QueryMetering;
                //authProps.LicenseType = LicenseType.EnterpriseCoreAPI;


                _logger.Debug(string.Format("Pokušaj uspostavljanja veze na klasičan način sa identitetom: {0} i licencom: {1}.", userName, authProps.LicenseType));

                try
                {
                    app = Application.Connect(authProps);
                }
                catch (InvalidLicensingException)
                {
                    // Ne loguj kao grešku više warning jer nedostaju licence
                    _logger.Warn(string.Format("Error: Invalid Licensing. Neuspešna uspostavljanja veze sa identitetom: {0}.", userName));
                }

                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                }

                if (app != null)
                {
                    _logger.Info(string.Format("Konekcija je uspešno uspostavljena identitetom [{0}]. SessionID: [{1}].", userName, app.SessionID));
                }
            }
            #endregion

            #region AppPoolConnection 
            if (isAppPoolConnection == true)
            {
                // Vrti kroz dostupne app.pool identitete
                var obj = new Algotech.UTL.AlgotechUnityUDF.AppPool();
                var listAppPool = obj.GetAppPools((short)AppPool.Status.Active).OrderBy(l => l.AppPoolIdentity);
                foreach (var appPool in listAppPool)
                {
                    var userName = appPool.AppPoolIdentity;
                    var userPassword = appPool.AppPoolPassword;

                    OnBaseAuthenticationProperties authProps = Application.CreateOnBaseAuthenticationProperties(_appServerURL, userName, userPassword, _dataSource);

                    authProps.IdentitySettings = authProps.CreateIdentitySettings("AlgotechUnityREST");
                    //authProps.LicenseType = LicenseType.Default;
                    //authProps.LicenseType = LicenseType.EnterpriseCoreAPI;
                    //authProps.LicenseType = LicenseType.QueryMetering;
                    authProps.LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), _licenceType);

                    // Vidi web.config
                    var timeOut = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["APPLICATION_TIMEOUT"]);

                    _logger.Debug(string.Format("Pokušaj uspostavljanja veze sa identitetom: {0} i licencom: {1}.", userName, authProps.LicenseType));
                    try
                    {
                        app = Application.Connect(authProps);
                        if (timeOut != 0)
                        {
                            app.Timeout = timeOut;
                        } 
                    }
                    catch (SystemLockedOutException)
                    {
                        Console.Out.WriteLine("Warn: The system is currently in lockout mode.");
                        Console.Out.WriteLine();
                        _logger.Warn("Warn: The system is currently in lockout mode.");
                    }
                    catch (InvalidLoginException)
                    {
                        Console.Out.WriteLine("Warn: Invalid Login Credentials.");
                        Console.Out.WriteLine();
                        _logger.Warn("Warn: Invalid Login Credentials.");
                    }

                    catch (MaxConcurrentLicensesException)
                    {
                        Console.Out.WriteLine("Warn: All concurrent licenses for this user group have been consumed.");
                        Console.Out.WriteLine();
                        _logger.Warn("Warn: All concurrent licenses for this user group have been consumed.");
                    }

                    catch (InvalidLicensingException)
                    {
                        Console.Out.WriteLine(string.Format("Warning: Invalid Licensing. Neuspešna uspostavljanja veze sa identitetom: {0} proverite licencu.", appPool.AppPoolIdentity));
                        Console.Out.WriteLine();
                        _logger.Warn(string.Format("Warning: Invalid Licensing. Neuspešna uspostavljanja veze sa identitetom: {0} proverite licencu.", appPool.AppPoolIdentity));
                    }

                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message);
                    }

                    if (app != null)
                    {
                        _logger.Info(string.Format("Konekcija je uspešno uspostavljena identitetom [{0}]. SessionID: [{1}] TimeOut: [{2}].", appPool.AppPoolIdentity, app.SessionID, app.Timeout));
                        break;
                    }
                }
            }
            #endregion

            _logger.Info("Pomoćna funkcija:: OnBaseConnectByAppPool end " + DateTime.Now + " sec " + (DateTime.Now - dateTimeStart).TotalSeconds.ToString());

            return app;
        }
        #endregion

        #region OnBaseDisconnect 
        /// <summary>
        /// Diskonektovanje OnBase
        /// </summary>
        /// <param name="app"></param>
        public void OnBaseDisconnect(Application app)
        {
            try
            {
                app.Disconnect();
            }
            catch (SessionNotFoundException)
            {
                _logger.Error("Error: Aktivna sesija nije pronađena.");
            }
            finally
            {
                app.Dispose();
            }

            _logger.Info("Konekcija je zatvorena.");
        }
        #endregion
      
    }
}
