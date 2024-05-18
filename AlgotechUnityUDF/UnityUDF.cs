// Predrag Živić
// 28.11.2022

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DataSystem.DAL.SqlHelper;

using Hyland.Unity;

namespace Algotech.UTL.AlgotechUnityUDF
{
    public class UnityUDF
    {
        #region Variable  
        // Logger
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region DocBarcode2DocumentID
        /// <summary>
        /// Za requestID i opciono documentType vraća DocumentID
        /// </summary>
        /// <param name="application"></param>
        /// <param name="requestID"></param>
        /// <param name="documentTypeName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long DocBarcode2DocumentID(Application application, string documentTypeName, string docBarcode)
        {
            var dateTimeStart = DateTime.Now;
            long documentID = 0;

            if (documentTypeName == string.Empty) { throw new ArgumentException("Parametar documentTypeName mora da bude prosleđen"); }
            if (docBarcode == string.Empty) { throw new ArgumentException("Parametar docBarcode mora da bude prosleđen"); }

            _logger.Info("Početak DocBarcode2DocumentID: " + dateTimeStart);

            DocumentQuery docQuery = application.Core.CreateDocumentQuery();

            // 
            _logger.Info("Provera RequsetID mora da bude jedinstven u okviru DocumentType");
            DocumentType documentType = application.Core.DocumentTypes.Find(documentTypeName);
            if (documentType == null)
            {
                _logger.Error(string.Format("Nije pronađen DocumentType '{0}'.", documentTypeName));
                throw new Exception(string.Format("Nije pronađen DocumentType '{0}'.", documentTypeName));
            }
            docQuery.AddDocumentType(documentType);
            docQuery.AddKeyword("DocBarcode", docBarcode, KeywordOperator.Equal, KeywordRelation.Or);

            DocumentList documents = docQuery.Execute(10);
            if (documents.Count == 0)
            {
                _logger.Error(string.Format("DocBarcode #{0} nije pronađen za DocumentType '{1}'!", docBarcode, documentTypeName));
                throw new Exception(string.Format("RequestID #{0} nije pronađen za DocumentType '{1}'!", docBarcode, documentTypeName));
            }
            if (documents.Count > 1)
            {
                _logger.Error(string.Format("DocBarcode #{0} nije jedinstven za DocumentType '{1}'!", docBarcode, documentTypeName));
                throw new Exception(string.Format("DocBarcode #{0} nije jedinstven za DocumentType '{1}'!", docBarcode, documentTypeName));
            }

            foreach (Document item in documents)
            {
                //Debug.Write(item.ID + " - " + item.Name + "\r\n");
                documentID = Convert.ToInt64(item.ID);
            }

            _logger.Info(string.Format("Za DocBarcode #{0} DocumentID je #{1}!", docBarcode, documentID));
            // Metrika end
            var dateTimeEnd = DateTime.Now;
            var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
            _logger.Info("Funkcija:: DocBarcode2DocumentID finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());

            return documentID;
        }
        #endregion

        #region DocBarcode2DocumentID
        /// <summary>
        /// Za docBarcode vraća DocumentID
        /// </summary>
        /// <param name="application"></param>
        /// <param name="docBarcode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long DocBarcode2DocumentID(Application application, string docBarcode)
        {
            var dateTimeStart = DateTime.Now;
            long documentID = 0;

            if (docBarcode == string.Empty) { throw new ArgumentException("Parametar docBarcode mora da bude prosleđen"); }

            _logger.Info("Početak DocBarcode2DocumentID: " + dateTimeStart);

            DocumentQuery docQuery = application.Core.CreateDocumentQuery();

            // Pronađi ključnu reč
            docQuery.AddKeyword("DocBarcode", docBarcode, KeywordOperator.Equal, KeywordRelation.Or);

            DocumentList documents = docQuery.Execute(10);
            if (documents.Count == 0)
            {
                _logger.Error(string.Format("DocBarcode #{0} nije pronađen!", docBarcode));
                throw new Exception(string.Format("DocBarcode #{0} nije pronađen!", docBarcode));
            }
            if (documents.Count > 1)
            {
                _logger.Error(string.Format("DocBarcode #{0} nije jedinstven'!", docBarcode));
                throw new Exception(string.Format("DocBarcode '{0}' nije jedinstven'!", docBarcode));
            }

            foreach (Document item in documents)
            {
                documentID = Convert.ToInt64(item.ID);
            }

            _logger.Info(string.Format("Za DocBarcode #{0} DocumentID je #{1}!", docBarcode, documentID));

            // Metrika end
            var dateTimeEnd = DateTime.Now;
            var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
            _logger.Info("Funkcija:: DocBarcode2DocumentID finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());

            return documentID;
        }
        #endregion

        #region DocumentIsExist
        /// <summary>
        /// Proverava da li dokument postoji
        /// </summary>
        /// <param name="application">OnBase Aplikacija</param>
        /// <param name="documentTypeName">OnBase dokument tip</param>
        /// <param name="docBarcode">Jedinstveni broj dokumenta</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public bool DocumentIsExist(Application application, string documentTypeName, string docBarcode)
        {
            if (documentTypeName == string.Empty) { throw new ArgumentException("Parametar documentTypeName mora da bude prosleđen"); }
            if (docBarcode == string.Empty) { throw new ArgumentException("Parametar docBarcode mora da bude prosleđen"); }

            DocumentQuery docQuery = application.Core.CreateDocumentQuery();
            DocumentType documentType = application.Core.DocumentTypes.Find(documentTypeName);
            if (documentType == null)
            {
                throw new Exception(string.Format("Nije pronađen documentTypeName '{0}'.", documentTypeName));
            }
            docQuery.AddDocumentType(documentType);
            docQuery.AddKeyword("DocBarcode", docBarcode, KeywordOperator.Equal, KeywordRelation.Or);

            DocumentList documents = docQuery.Execute(10);
            return documents.Count != 0;
        }

        /// <summary>
        /// Za docBarcode vraća DocumentID
        /// </summary>
        /// <param name="application"></param>
        /// <param name="docBarcode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool DocumentIsExist(Application application, string docBarcode)
        {
            var dateTimeStart = DateTime.Now;
            var retValue = false;

            if (docBarcode == string.Empty) { throw new ArgumentException("Parametar docBarcode mora da bude prosleđen"); }

            _logger.Info("Početak DocumentIsExist: " + dateTimeStart);

            DocumentQuery docQuery = application.Core.CreateDocumentQuery();

            // Pronađi ključnu reč
            docQuery.AddKeyword("DocBarcode", docBarcode, KeywordOperator.Equal, KeywordRelation.Or);

            DocumentList documents = docQuery.Execute(10);
            if (documents.Count == 0)
            {
                _logger.Error(string.Format("DocBarcode #{0} nije pronađen!", docBarcode));
                //throw new Exception(string.Format("DocBarcode #{0} nije pronađen!", docBarcode));
                retValue = false;
            }
            else
            {
                _logger.Error(string.Format("DocBarcode #{0} već postoji'!", docBarcode));
                //throw new Exception(string.Format("DocBarcode '{0}' nije jedinstven'!", docBarcode));
                retValue = true;
            }

            //long documentID = 0;
            //foreach (Document item in documents)
            //{
            //    documentID = Convert.ToInt64(item.ID);
            //    _logger.Info("docBarcode postoji -> documentID: " + documentID);
            //}

            // Metrika end
            var dateTimeEnd = DateTime.Now;
            var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
            _logger.Info("Funkcija:: DocumentIsExist finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());

            return retValue;
        }
        #endregion

        #region GetQueueID
        /// <summary>
        /// Vraća QueueID određenog radnog toka i naziva reda/faze
        /// </summary>
        /// <param name="application">Aplikacija sa konekcijom</param>
        /// <param name="lifeCycleName">Naziv radnog toka</param>
        /// <param name="queueName">Naziv reda u radnom toku</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public long GetQueueID(Application application, string lifeCycleName, string queueName)
        {
            var dateTimeStart = DateTime.Now;
            long queueID = 0;

            try
            {
                if (lifeCycleName == string.Empty) { throw new ArgumentException("Parametar lifeCycleName mora da bude prosleđen!"); }
                if (queueName == string.Empty) { throw new ArgumentException("Parametar queueName mora da bude prosleđen!"); }

                // Metrika
                _logger.Info("Funkcija:: GetQueueID start " + dateTimeStart);

                // LifeCycles i pridajajući Queue
                var lifeCycle = application.Workflow.LifeCycles.Find(lifeCycleName);
                if (lifeCycle == null)
                {
                    throw new Exception(string.Format("LifeCycle '{0}' nije pronađen!", lifeCycleName));
                }
                Debug.Write(lifeCycle.ID + " - " + lifeCycle.Name);
                //.

                // Za svaki queue u radnom toku LifeCycle
                var queues = application.Workflow.Queues.ToArray();

                //string[] names = { "Steve", "Bill", "Bill Gates", "Ravi", "Mohan", "Salman", "Boski" };
                //var result = Array.Find(names, element => element == queueName); // returns "Bill"

                foreach (Hyland.Unity.Workflow.Queue queue in queues.Where(queue => queue.LifeCycle.ID == lifeCycle.ID && queue.Name == queueName))
                {
                    Debug.Write(queue.ID + " - " + queue.Name + " [" + queue.LifeCycle.Name + "]");
                    _logger.Info(queue.ID + " - " + queue.Name + " [" + queue.LifeCycle.Name + "]");

                    if (queue.Name == queueName)
                    {
                        queueID = queue.ID;
                        _logger.Info("Našli smo queueID= " + queueID);
                        break;
                    }
                }
                //.
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod pomoćne funkcije GetQueueID koja vraća QueueID radnog toka {0} i naziva reda/faze {1}.", lifeCycleName, queueName));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Funkcija:: GetQueueID finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }

            return queueID;
        }
        #endregion

        #region DocumentExport
        public string DocumentExport(Application application, long documentID)
        {
            if (documentID == 0)
                throw new ArgumentException("Morate da prosledite documentiD.");

            var uploadFolder = System.Configuration.ConfigurationManager.AppSettings["UploadFolder"];

            Document document = application.Core.GetDocumentByID(documentID);
            if (document == null)
                throw new Exception("Document was not found");

            var fileName = String.Empty;

            // Export to a file
            DefaultDataProvider defaultDataProvider = application.Core.Retrieval.Default;
            using (PageData pageData = defaultDataProvider.GetDocument(document.DefaultRenditionOfLatestRevision))
            {
                using (Stream stream = pageData.Stream)
                {
                    // Naziv datoteke
                    fileName = string.Format(uploadFolder + @"\{0}.{1}", Guid.NewGuid().ToString(), pageData.Extension);
                    Utility.WriteStreamToFile(stream, fileName);
                    var pageDataExtension = pageData.Extension;
                }
            }
            //.

            return fileName;
        }
        #endregion

        #region GetDocumentTypeID
        // Predrag Živić
        // 02.11.2023

        /// <summary>
        /// Vraća id tipa dokumenata za prosleđeni <documentTypeName>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long GetDocumentTypeID(Application application, string documentTypeName)
        {
            long retValue = 0;
            try
            {
                //// Idemo kroz kolekciju DocumentType i uzimamo naziv
                //foreach (DocumentType docType in _application.Core.DocumentTypes)
                //{
                //    if (docType.Name == documentTypeName)
                //    {
                //        retValue = docType.ID;
                //    }
                //}

                // Pretraga liste
                var result = application.Core.DocumentTypes.Find(x => x.Name == documentTypeName);
                retValue = result.ID;

                return retValue;
            }

            catch (SessionNotFoundException ex)
            {
                application.Diagnostics.Write(ex);
                throw new Exception("Uniti API sessija nije pronađena.", ex);
            }

            catch (UnityAPIException ex)
            {
                application.Diagnostics.Write(ex);
                throw new Exception("Dogodila se Unity API greška.", ex);
            }

            catch (Exception ex)
            {
                application.Diagnostics.Write(ex);
                throw new Exception("Nepoznata greška", ex);
            }
        }
        #endregion

        #region GetFullPath
        /// <summary>
        /// Vraća putanju datoteke
        /// </summary>
        /// <param name="diskGroupID">Disk grupa u kojoj se nalazi dokument</param>
        /// <param name="documentID">OnBase Document Handle</param>
        /// <returns></returns>
        public string GetFullPath(long diskGroupID, long documentID)
        {
            var dateTimeStart = DateTime.Now;
            var ret = string.Empty;
            try
            {
                _logger.Info("Metoda GetFullPath");

                var sqlHelper = new SqlHelper("connection_string_onbase");
                var sqlString = string.Empty;

                //sqlString = string.Format("SELECT RTRIM(hsi.physicalplatter.lastuseddrive) + " +
                //    "RTRIM(hsi.itemdatapage.filepath) AS FullPath FROM hsi.itemdata " +
                //    "INNER JOIN hsi.itemdatapage ON hsi.itemdata.itemnum = hsi.itemdatapage.itemnum " +
                //    "INNER JOIN hsi.physicalplatter ON hsi.itemdatapage.logicalplatternum = hsi.physicalplatter.logicalplatternum " +
                //    "WHERE (hsi.itemdata.itemnum={0}) AND(hsi.physicalplatter.diskgroupnum={1}) AND(hsi.physicalplatter.physicalplatternum=1)", documentID, diskGroupID);

                sqlString = string.Format("SELECT TOP(1) RTRIM(hsi.physicalplatter.lastuseddrive) + " +
                    "RTRIM(hsi.itemdatapage.filepath) AS FullPath FROM hsi.itemdata " +
                    "INNER JOIN hsi.itemdatapage ON hsi.itemdata.itemnum = hsi.itemdatapage.itemnum " +
                    "INNER JOIN hsi.physicalplatter ON hsi.itemdatapage.logicalplatternum = hsi.physicalplatter.logicalplatternum " +
                    "WHERE (hsi.itemdata.itemnum={0}) AND (hsi.physicalplatter.diskgroupnum={1}) AND (hsi.physicalplatter.physicalplatternum=1) AND (hsi.itemdatapage.itempagenum=0) " +
                    "ORDER BY hsi.itemdatapage.docrevnum DESC", documentID, diskGroupID);

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                ret = sqlHelper.ExecuteScalar(sqlString).ToString();
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod preuzimaja podataka o putanji datoteke documentID.", documentID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Funkcija:: GetFullPath finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }
            return ret;

        }
        #endregion

        //#region GetDiskGroupID
        ///// <summary>
        ///// Vraća GetDiskGroupID
        ///// </summary>
        ///// <returns></returns>
        ///// <exception cref="Exception"></exception>
        //public long GetDiskGroupID(string diskGroupName)
        //{
        //    var dateTimeStart = DateTime.Now;
        //    long ret = 0;
        //    try
        //    {
        //        _logger.Info("Metoda GetDiskGroupID");

        //        var sqlHelper = new SqlHelper("connection_string_onbase");
        //        var sqlString = string.Empty;

        //        // hsi.diskgroup.diskgroupname, hsi.physicalplatter.lastuseddrive 
        //        sqlString = string.Format("SELECT hsi.diskgroup.diskgroupnum " +
        //            "FROM hsi.diskgroup " +
        //            "INNER JOIN hsi.physicalplatter ON hsi.diskgroup.diskgroupnum = hsi.physicalplatter.diskgroupnum " +
        //            "WHERE hsi.physicalplatter.physicalplatternum = 1 AND hsi.diskgroup.diskgroupname='{0}'", diskGroupName);

        //        _logger.Info(string.Format("SQL string: {0}", sqlString));

        //        ret = Convert.ToInt64(sqlHelper.ExecuteScalar(sqlString));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Logger
        //        _logger.Error(string.Format("Greška kod preuzimaja podataka o disk grupi {0}.", diskGroupName));
        //        _logger.Error(ex.Message);
        //        _logger.Error(ex.StackTrace);
        //        //.

        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        // Metrika end
        //        var dateTimeEnd = DateTime.Now;
        //        var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
        //        _logger.Info("Funkcija:: GetDiskGroupID finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
        //    }
        //    return ret;
        //}
        //#endregion

        #region IsValidGroupType
        /// <summary>
        /// Vraća putanju datoteke
        /// </summary>
        /// <param name="groupType">OnBase DocumetType </param>
        /// <param name="docType">Keyword docType</param>
        /// <returns></returns>
        public bool IsValidGroupType(string documentTypeName, string groupType)
        {
            var dateTimeStart = DateTime.Now;
            var ret = false;
            try
            {
                _logger.Info("Metoda IsValidGroupType");

                var sqlHelper = new SqlHelper("connection_string_mssql");
                var sqlString = string.Empty;

                sqlString = string.Format("SELECT COUNT(*) FROM GroupType_Doctype WHERE DocumentTypeName='{0}' AND GroupType='{1}'", documentTypeName, groupType);

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                // Ako postoji u data setu onda je kombinacija dobra
                ret = Convert.ToBoolean(sqlHelper.ExecuteScalar(sqlString));
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod provere podataka DocumentTypeName '{0}' GroupType '{1}'.", documentTypeName, groupType));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Funkcija:: IsValidGroupType finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }
            return ret;

        }
        #endregion

        #region GetKeywordDataType
        /// <summary>
        /// Vraća ID tipa podatka (DataType) ključne reči.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public KeywordDataType GetKeywordDataType(Application application, string keywordTypeName)
        {
            try
            {
                _logger.Info("Funckija GetKeywordDataType");
                _logger.Info(string.Format("keywordTypeName: {0}.", keywordTypeName));

                KeywordDataType keywordTypeDataType = KeywordDataType.Undefined;
                foreach (KeywordType keywordType in application.Core.KeywordTypes)
                {
                    if (keywordType.Name == keywordTypeName)
                    {
                        keywordTypeDataType = keywordType.DataType;
                    }
                }

                return keywordTypeDataType;
            }

            catch (Exception ex)
            {
                // Logger
                _logger.Error("Nepoznata greška");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region GetFilesFullPath
        /// <summary>
        /// Vraća putanju datoteke
        /// </summary>
        /// <param name="diskGroupID">Disk grupa u kojoj se nalazi dokument</param>
        /// <param name="documentID">OnBase Document Handle</param>
        /// <returns></returns>
        public Dictionary<string, string> GetFilesFullPath(long diskGroupID, long documentID)
        {
            var dateTimeStart = DateTime.Now;

            try
            {
                _logger.Info("Metoda GetFullPath");

                var sqlHelper = new SqlHelper("connection_string_onbase");
                var sqlString = string.Empty;

                var lastRevisionNum = GetLastRevisionNum(diskGroupID, documentID);

                // Sve datoteke određenog dokumenta poslednje revizije sortirano po broju revizije
                sqlString = string.Format("SELECT hsi.itemdatapage.itempagenum AS PageNum, RTRIM(hsi.physicalplatter.lastuseddrive) + RTRIM(hsi.itemdatapage.filepath) AS FullPath FROM hsi.itemdata " +
                    "INNER JOIN hsi.itemdatapage ON hsi.itemdata.itemnum = hsi.itemdatapage.itemnum " +
                    "INNER JOIN hsi.physicalplatter ON hsi.itemdatapage.logicalplatternum = hsi.physicalplatter.logicalplatternum " +
                    "WHERE (hsi.itemdata.itemnum = {0}) AND " +
                    "(hsi.physicalplatter.diskgroupnum = {1}) AND " +
                    "(hsi.physicalplatter.physicalplatternum=1) AND " +
                    "(hsi.itemdatapage.docrevnum={2})" +
                    "ORDER BY hsi.itemdatapage.itempagenum", documentID, diskGroupID, lastRevisionNum);

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                var reader = sqlHelper.ExecuteReader(sqlString);

                string pageNum;
                string filePath;
                var list = new Dictionary<string, string>();
                while (reader.Read())
                {
                    pageNum = Convert.ToString(reader["PageNum"]);
                    filePath = Convert.ToString(reader["FullPath"]);
                    list.Add(pageNum, filePath);
                }
                reader.Close();

                return list;
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod preuzimaja podataka o putanji datoteke documentID.", documentID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;

                _logger.Info("Funkcija:: GetFullPath finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }

        }
        #endregion

        #region GetLastRevisionNum
        /// <summary>
        /// Vraća broj poslednje revizije dokumenta
        /// </summary>
        /// <param name="diskGroupID">Disk grupa u kojoj se nalazi dokument</param>
        /// <param name="documentID">OnBase Document Handle</param>
        /// <returns></returns>
        public short GetLastRevisionNum(long diskGroupID, long documentID)
        {
            var dateTimeStart = DateTime.Now;
            short ret = 0;
            try
            {
                _logger.Info("Metoda GetLastRevisionNum");

                var sqlHelper = new SqlHelper("connection_string_onbase");
                var sqlString = string.Empty;

                sqlString = string.Format("SELECT MAX(hsi.itemdatapage.docrevnum) AS LastVersion FROM hsi.itemdata " +
                    "INNER JOIN hsi.itemdatapage ON hsi.itemdata.itemnum = hsi.itemdatapage.itemnum " +
                    "INNER JOIN hsi.physicalplatter ON hsi.itemdatapage.logicalplatternum = hsi.physicalplatter.logicalplatternum " +
                    "WHERE (hsi.itemdata.itemnum = {0}) AND (hsi.physicalplatter.diskgroupnum = {1}) AND (hsi.physicalplatter.physicalplatternum = 1)", documentID, diskGroupID);

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                ret = Convert.ToInt16(sqlHelper.ExecuteScalar(sqlString));
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod preuzimaja podataka o poslednjoj verziji datoteke.", documentID));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;
                _logger.Info("Funkcija:: GetFullPath finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }
            return ret;

        }
        #endregion

        #region GetSequenceValue
        /// <summary>
        /// Vraća vrednost sekvencer
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        public long GetSequenceValue(string prefix)
        {
            var dateTimeStart = DateTime.Now;

            try
            {
                _logger.Info("Metoda GetSequenceValue");
                _logger.Info("Konektujem Sql");

                var sqlHelper = new SqlHelper("connection_string_mssql");

                // Napravi sql string
                var sqlString = string.Format(@"SELECT NEXT VALUE FOR RefNo_{0} AS seq", prefix);

                _logger.Info(string.Format("SQL string: {0}", sqlString));

                // Pukni upit
                var reader = sqlHelper.ExecuteReader(sqlString);
                long seq = 0;
                if (reader.Read())
                {
                    seq = Convert.ToInt64(reader["seq"]);
                }
                reader.Close();
                reader.Dispose();
                //.

                //return string.Format("{0}-{1}/{2}", prefix, seq, DateTime.Now.Year);
                return seq;
            }
            catch (Exception ex)
            {
                // Logger
                _logger.Error(string.Format("Greška kod preuzimaja podataka o sekvenci za [{0}].", prefix));
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                //.

                throw new Exception(ex.Message);
            }
            finally
            {
                // Metrika end
                var dateTimeEnd = DateTime.Now;
                var diffInSeconds = (dateTimeEnd - dateTimeStart).TotalSeconds;

                _logger.Info("Funkcija:: GetFullPath finally " + dateTimeEnd + " sec " + diffInSeconds.ToString());
            }

        }
        #endregion

        #region RemoveKeywordRecord
        public bool RemoveKeywordRecord(Application app, Document document, string keywordTypeName)
        {
            _logger.Info("Pokušaj ažuriranja zapisa ključnih reči...");

            // Keyword tip
            KeywordType keywordType = app.Core.KeywordTypes.Find(keywordTypeName);
            if (keywordType == null)
            {
                return false;
                throw new Exception("Tip ključne reči nije pronađen");
            }
            // Idemo da uklonimo ključnu reč
            var forceLockDocument = System.Configuration.ConfigurationManager.AppSettings["ForceLockDocumentOnDelete"] == "True";
            if (forceLockDocument == true)
            {
                _logger.Info(string.Format("Zaključavanje dokumenta : {0}.", document.ID));
                using (DocumentLock documentLock = document.LockDocument())
                {
                    if (documentLock.Status != DocumentLockStatus.LockObtained)
                    {

                        _logger.Warn(string.Format("Zaključavanje dokumenta DocumentID: {0} nije uspelo!", document.ID));
                        throw new Exception("Failed to lock document");
                    }
                }
            }
            //.
            
            _logger.Info("Pokušaj CreateKeywordModifier...");
            KeywordModifier keywordModifier = document.CreateKeywordModifier();

            // Pronalazimo zapis u dokumentu
            _logger.Info("Pokušaj da se pronađe zapis...");
            KeywordRecord keywordRecord = document.KeywordRecords.Find(keywordType);
            if (keywordRecord == null)
            {
                return false;
                throw new Exception("Zapis ključne reči nije pronađen");
            }
            //.

            if (keywordRecord != null)
            {
                // Find the old keyword from the record of the keyword type.
                _logger.Info("Pronađite staru ključnu reč iz zapisa tipa ključne reči i radi izmenu....");
                var keyword = keywordRecord.Keywords.Find(keywordTypeName);
                keywordModifier.RemoveKeyword(keyword);
            }

            keywordModifier.ApplyChanges();

            //.

            _logger.Info("Ključna reč je uklonjena.");
            return true;
        }
        #endregion
    }
}
