using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//TODO: add CR namespaces

namespace BusinessViewUtils
{
    static class Program
    {

        private static InfoStore infoStore;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Credentials());
        }

        static void NewFolder(String CUID) 
        {

            String COMMAND_TEXT = String.Format("SELECT SI_ID, SI_CUID, SI_KIND, " +
            "SI_PARENT_FOLDER_CUID, SI_PARENT_CUID, SI_NAME, SI_PATH " +
            "FROM CI_INFOOBJECTS, CI_APPOBJECTS WHERE SI_CUID='?'", CUID);
     
            // return first item from the collection
            InfoObject infoObject = Repository.Query(infoStore, COMMAND_TEXT)(1);
            //InfoObject infoObject = Repository.Query(infoStore, COMMAND_TEXT.Replace("?", CUID))(1);

            String path;
            int TotalFolders = infoObject.Properties("SI_PATH").Properties("SI_NUM_FOLDERS").Value;
    
            // For i = TotalFolders To 1 Step -1
            for (int i = TotalFolders; i > 0; i--) {
                String folderName = infoObject.Properties("SI_PATH").Properties("SI_FOLDER_NAME" + i).Value;
                path += "/" + folderName;
            }

            RepositoryDataSetTableAdapters.FolderTableAdapter folderAdapter;
            folderAdapter.Insert(InfoObject.CUID, InfoObject.Title, path);
            //Dim FA As New FolderAdapter
            //FA.Add InfoObject.CUID, InfoObject.Title, Path
    
        }

        static void Process()
        {

            try
            {
                infoStore = Repository.Authenticate("account", "password", "server:6400", "secEnterprise");

                ResetTables();
                ProcessPromptGroups();
                ProcessReports();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        static void ProcessPromptGroups()
        {

            String COMMAND_TEXT = "SELECT SI_ID, SI_CUID, SI_KIND, " +
                "SI_PARENT_FOLDER_CUID, SI_PARENT_CUID, SI_NAME, SI_DESCRIPTION, " +
                "WP_SI_METADATA_PARAMETER_PROMPTGROUPLOVDS,SI_METADATA_CHILDREN " +
                "FROM CI_APPOBJECTS WHERE SI_KIND='RepositoryPromptGroup'";
        
            InfoObjects infoObjects = Repository.Query(null, COMMAND_TEXT);
    
            foreach (Object infoObject in infoObjects) {

                RepositoryDataSetTableAdapters.PromptGroupTableAdapter PromptGroupAdapter;
                PromptGroupAdapter.Insert(infoObject.CUID, infoObject.Title, infoObject.ParentCUID, infoObject.Properties("SI_PARENT_FOLDER_CUID").Value);
                //Dim PGA As New PromptGroupAdapter;
                //PGA.Add infoObject.CUID, infoObject.Title, infoObject.ParentCUID, infoObject.Properties("SI_PARENT_FOLDER_CUID").Value;
        
                NewFolder InfoObject.Properties("SI_PARENT_FOLDER_CUID").Value;
        
            }

        }

        static void ProcessReports()
        {

            String COMMAND_COUNT_TEXT = "SELECT Count(SI_ID) FROM CI_INFOOBJECTS WHERE SI_KIND='CrystalReport' AND SI_INSTANCE=0";
    
            String COMMAND_TEXT = "SELECT TOP 2000 * FROM CI_INFOOBJECTS WHERE SI_KIND='CrystalReport' AND SI_INSTANCE=0";
    
            InfoObjects infoObjects = Repository.Query(infoStore, COMMAND_TEXT);
    
            foreach (InfoObject infoObject in infoObjects) {
                
                // cast generic InfoObject to a Report
                CrystalReportPluginLib.Report report = (CrystalReportPluginLib.Report)infoObject;

                // if report has a d/c parameter
                if ( report.HasDynamicCascadePrompt ) {
        
                    RepositoryDataSetTableAdapters.ReportTableAdapter reportAdapter;
                    reportAdapter.Insert(InfoObject.CUID, InfoObject.Title, InfoObject.ParentCUID, InfoObject.Properties("SI_PARENT_FOLDER_CUID").Value);

                    //Dim RA As New ReportAdapter;
                    //RA.Add InfoObject.CUID, InfoObject.Title, InfoObject.ParentCUID, InfoObject.Properties("SI_PARENT_FOLDER_CUID").Value;
        
                    NewFolder infoObject.Properties("SI_PARENT_FOLDER_CUID").Value;
            
                    foreach (CrystalReportPluginLib.ReportParameter Parameter in report.ReportParameters) {
            
                        if (Parameter.HasDynamicCascadePrompt) {
                
                            // TODO: replace this logic w/ regex
                            // eor://server:6400/ASQHPtIDw8hJj_xLQflb6fo
                            String PromptGroupCUID = Parameter.DynamicCascadePromptGroupID;

                            if ( PromptGroupCUID.Contains("eor://") ) {
                                // extract CUID from URI (e.g.  eor://server:6400/ASQHPtIDw8hJj_xLQflb6fo)
                                PromptGroupCUID = PromptGroupCUID.Substring(s.LastIndexOf("/", PromptGroupCUID.Length - 1) + 1);
                                //PromptGroupCUID = Right(PromptGroupCUID, Len(PromptGroupCUID) - InStrRev(PromptGroupCUID, "/"));
                            }
                            
                            RepositoryDataSetTableAdapters.ReportPromptGroupTableAdapter reportPromptGroupAdapter;
                            reportPromptGroupAdapter.Insert(Report.CUID, PromptGroupCUID);
                            // Dim RPGA As New ReportPromptGroupAdapter RPGA;
                            // RPGA.Add Report.CUID, PromptGroupCUID;
                    
                        }
                
                    }
            
                }
                
            }
        }

        static void ResetTables()
        {
            // TODO: logic to truncate all tables' data
            RepositoryDataSetTableAdapters.PromptGroupTableAdapter promptGroupAdapter;
            RepositoryDataSetTableAdapters.FolderTableAdapter folderAdapter;
            RepositoryDataSetTableAdapters.ReportTableAdapter reportAdapter;
            RepositoryDataSetTableAdapters.ReportPromptGroupTableAdapter reportPromptGroupAdapter;

            //Dim PGA As New PromptGroupAdapter
            //PGA.Clear
    
            //Dim FA As New FolderAdapter
            //FA.Clear
    
            //Dim RA As New ReportAdapter
            //RA.Clear
    
            //Dim RPGA As New ReportPromptGroupAdapter
            //RPGA.Clear
        }

    }
}
