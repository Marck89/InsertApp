using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Net;
using System.Data.OracleClient;
using System.Configuration;




namespace InsertApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string file = "";
        public static List<string[]> elements;
        public string idSuccess = "";
        public static List<String> elIns;
        InsertApp.Utility.myLog4Net l4n = new InsertApp.Utility.myLog4Net();
        public MainWindow()
        {
            InitializeComponent();
        }



        /*
         * primo tasto:
         * cerca il file nel File System
         * se esiste esegue la funzione parseFile
         * */
        public void AddFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Open File";
            fileDialog.DefaultExt = ".txt"; // Required file extension 
            fileDialog.Filter = "All Files|*.*";
            fileDialog.Multiselect = true;
            fileDialog.FileName = "";

            string value = System.Configuration.ConfigurationManager.AppSettings["connString"];
            

            try
            {
                fileDialog.InitialDirectory = @"C:\Desktop";//directory iniziale 
            }
            catch
            {
                result.ScrollToEnd();
                result.AppendText("ERRORE FILE SYSTEM\n");
                l4n.myLogger("ERRORE FILE SYSTEM\n", "info");

            }
            fileDialog.ShowDialog();
            file = fileDialog.FileName;
            if (file != "")
            {
                ParseFile();
            }
        }



        //funzione parse file
        public void ParseFile()
        {
            
            try
            {
                elements = parseCSV(); //esegue parseCSV
                int lung = elements.Count();
                result.ScrollToEnd();
                result.AppendText("\nelementi trovati: " + lung.ToString() + "\n");
                l4n.myLogger("\nelementi trovati: " + lung.ToString() + "\n", "info");
            }
            catch (Exception ex)
            {
                result.ScrollToEnd();
                result.AppendText("\nERRORE: " + ex.Message.ToString() + "\n");
                l4n.myLogger("\nERRORE: " + ex.Message.ToString() + "\n", "info");
                
            }
        }



        //parsing del file excel
        public List<string[]> parseCSV()
        {
            List<string[]> parsedData = new List<string[]>();
            try
            {
                //WaitAsynchronouslyAsync("FOUND");
                result.ScrollToEnd();
                result.AppendText("\nFILE TROVATO: " + file + "\n");
                l4n.myLogger("\nFILE TROVATO: " + file + "\n", "info");
                using (StreamReader readFile = new StreamReader(file))
                {
                    String line;
                    
                    string[] row;
                    while ((line = readFile.ReadLine()) != null)
                    {  
                        //byte[] stringa = Encoding.Unicode.GetBytes(line);
                        //Encoding.ASCII.GetString(stringa);
                        //row = Encoding.ASCII.GetString(stringa).Split('b');
                        row = line.Split(';');
                        parsedData.Add(row);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                result.ScrollToEnd();
                result.AppendText("\nERRORE: " + ex.Message.ToString() + "\n");
                l4n.myLogger("\nERRORE: " + ex.Message.ToString() + "\n", "info");
            }
            return parsedData;
        }



  



        //Funzione per la insert

        public void insertData(object sender, RoutedEventArgs e)
        {


            if (devcid.Text != "" && operatorid.Text != "" && statusB.Text != "" && sregionid.Text != "" && param1.Text != "" && plantc.Text != "" && plantt.Text != "" && devl.Text != "")
            {

            //valori che ricevo via form
            int devCID = Convert.ToInt32(devcid.Text);
            int opID = Convert.ToInt32(operatorid.Text);
            string devgroup = devg.Text; //puop essere null
            string status = statusB.Text;
            int sellingregionid = Convert.ToInt32(sregionid.Text);
            int p1 = Convert.ToInt32(param1.Text);
            int plantcode = Convert.ToInt32(plantc.Text);
            int planttype = Convert.ToInt32(plantt.Text);
            int deviceloc = Convert.ToInt32(devl.Text);
            OracleConnection con = new OracleConnection();
            string id, localizzazione, desc, devname, sn;
            int devicecode;
            string toInsert = string.Empty;
            int index = 0;
            int giaPresenti = 0;
            int success = 0; //flag se la insert va a buon fine
            //*********************************************
       
                    try
                    {
                        string connectionStr = System.Configuration.ConfigurationManager.AppSettings["connString"]; //connessione al db
                        con.ConnectionString = connectionStr;
                        con.Open();
                    }
                    catch (Exception ex)
                    {
                        result.ScrollToEnd();
                        result.AppendText("ERRORE DI CONNESSIONE AL DB: " + ex.Message + "\n");
                        l4n.myLogger("ERRORE DI CONNESSIONE AL DB: " + ex.Message + "\n", "info");
                    }

                    elements.ForEach(delegate(string[] row){ //creazione della lista ToInsert (dev1, dev2, dev3, ....... , devn)

                        if (index == 0) //se è il primo elemento
                        {
                            toInsert = toInsert + row[0]; // (dev1....)

                        }
                        else //se non è il primo elemento
                        {
                            toInsert = toInsert + "," + row[0];  // (........ , devn ....) 

                        }
                        index++;

                    });


                    elements.ForEach(delegate(string[] row)
                    {
                        //******************** INIZIALIZZO PARAMETRI PER LA INSERT ****************************************************
                        id = row[0];
                        devicecode = Convert.ToInt32(id);
                        localizzazione = row[1];
                        desc = id + "-" + localizzazione;
                        //controllo lunghezza devicename
                        devname = "";
                        
                        if (id.Length == 1)
                        {
                            devname = "SU0001PKM_000" + id;
                        }
                        if (id.Length == 2)
                        {
                            devname = "SU0001PKM_00" + id;
                        }
                        if (id.Length == 3)
                        {
                            devname = "SU0001PKM_0" + id;
                        }
                        if (id.Length == 4)
                        {
                            devname = "SU0001PKM_" + id;
                        }

                                         
                        sn = id;

                        /*
                         * table = devices
                         * */


                        var commandSelect = "select * from devices where devicecode in ("+ toInsert +" ) and deviceclassid = :devclassid";
                        using (OracleCommand commandSel = new OracleCommand(commandSelect, con))
                        {
                            commandSel.Parameters.AddWithValue("devclassid", devCID);
                            try
                            {
                                int numrow = commandSel.ExecuteNonQuery(); //eseguo la query

                                if (numrow != -1) //se è presente anche solo un elemento 
                                {
                                    result.ScrollToEnd();
                                    result.AppendText("Elementi duplicati");
                                    l4n.myLogger("Elementi duplicati", "info");
                                    giaPresenti = 1; //metto a 1 il flag 
                                }
                                else //se nessun elemento è gia presente
                                {
                                    giaPresenti = 0; //metto a 0 il flag     
                                }
                            }
                            catch (Exception ex)
                            {
                                result.ScrollToEnd();
                                result.AppendText("ERRORE NELL'INSERT: " + ex.Message + "\n");
                                l4n.myLogger("ERRORE NELL'INSERT: " + ex.Message + "\n", "info");
                            }
                        }


                        if (giaPresenti == 0) //se nella query precedente è risultato che nessun elemento era presente procedo
                        {
                            //query
                            var commandText = "insert into devices (OPERATORID,DEVICECLASSID,DEVICECODE,DESCRIPTION,DEVICENAME, DEVICEGROUP,SALEDEVICEID,STATUS, DEVICESERIALNUMBER, SELLINGREGIONID, PARAM1, PLANTCODE,PLANTTYPE,DEVLOC) values(:opid,:devclid,:devcode,:descr,:devname,:devgroup,saledeviceid_seq.nextval,:status,:devsn,:sellregid,:param1,:plantcode,:planttype,:devloc)";


                            using (OracleCommand command = new OracleCommand(commandText, con))
                            {
                                //parametri
                                command.Parameters.AddWithValue("opid", opID);
                                command.Parameters.AddWithValue("devclid", devCID);
                                command.Parameters.AddWithValue("devcode", devicecode);
                                command.Parameters.AddWithValue("descr", desc);
                                command.Parameters.AddWithValue("devname", devname);
                                command.Parameters.AddWithValue("devgroup", devgroup);
                                command.Parameters.AddWithValue("devsn", sn);
                                command.Parameters.AddWithValue("status", status);
                                command.Parameters.AddWithValue("sellregid", sellingregionid);
                                command.Parameters.AddWithValue("param1", p1);
                                command.Parameters.AddWithValue("plantcode", plantcode); //
                                command.Parameters.AddWithValue("planttype", planttype); //
                                command.Parameters.AddWithValue("devloc", deviceloc); //

                                try
                                {
                                    //eseguo la query
                                    int numrow = command.ExecuteNonQuery();

                                    if (numrow == 1)
                                    {
                                        success = 1;
                                        if (idSuccess == "") //creo la lista
                                        {
                                            idSuccess = sn; //se è il primo elemento
                                        }
                                        else
                                        {
                                            idSuccess = idSuccess + "," + sn; //se non è il primo elemento
                                        }
                                    }
                                    else
                                    {
                                    }

                                }
                                catch (Exception ex)
                                {
                                    result.ScrollToEnd();
                                    result.AppendText("ERRORE NELL'INSERT: " + ex.Message + "\n");
                                    l4n.myLogger("ERRORE NELL'INSERT: " + ex.Message + "\n", "info");
                                    success = 0;
                                }
                            }
                        }
                        
                        //*************************************************************************************************************
                    });
                    if (success == 1)
                    {
                        con.Close();
                        result.ScrollToEnd();
                        result.AppendText("\nOperazione completata\n");
                        l4n.myLogger("\nOperazione completata\n", "info");
                        success = 0;
                    }
                }
                else
                {
                    result.ScrollToEnd();
                    result.AppendText("\nCompletare i campi\n");
                    l4n.myLogger("\nCompletare i campi\n", "info");

                }
            


        }

        public void CreateFileSystem(object sender, RoutedEventArgs e)
        {
                     
            string ipString = ipBox.Text; //indirizzo ip
            string[] res;
            string pathdest = @"";
            string pathresult;
            string pathpart = @"";
            IPAddress ipAddress;
            if (IPAddress.TryParse(ipString, out ipAddress)){       //controllo sull'indirizzo ip
                //string pathdest = "";
                string pathfinale = @"";
                elIns = GetElements();
                if (elIns.Count != 0)
                {
                    pathdest = @"\\" + ipString + System.Configuration.ConfigurationManager.AppSettings["pathInsert"];

                    elIns.ForEach(delegate(String row)
                    {
                        res = row.Split(';');
                        pathfinale = @"\\" + res[0] + "\\" + res[1] + "\\";
                        pathresult = pathdest + pathfinale;

                        //path = ip/c$/hsmnrt/rivendite/[deviceid]/[saledeviceid]/[param1]/hsmntr/serialize + 3 directory
                        if (!Directory.Exists(pathdest + devcid.Text))
                        {
                            try
                            {
                                Directory.CreateDirectory(pathdest + devcid.Text);

                            }
                            catch (Exception ex)
                            {
                                result.ScrollToEnd();
                                result.AppendText(ex.Message + "\n");
                                l4n.myLogger(ex.Message + "\n", "info");
                            }
                            pathpart = pathdest + devcid.Text + "\\";

                            //pathpart = pathdest + devcid.Text + System.IO.Path.PathSeparator;
                        }
                        else
                        {
                            pathpart = pathdest + devcid.Text + "\\";
                        }

                        if (!Directory.Exists(pathpart + res[0]))
                        {
                            try
                            {
                                Directory.CreateDirectory(pathpart + res[0]);
                            }
                            catch (Exception ex)
                            {
                                result.ScrollToEnd();
                                result.AppendText(ex.Message + "\n");
                                l4n.myLogger(ex.Message + "\n", "info");
                            }
                                pathpart = pathpart + res[0] + "\\";
                        }
                        else
                        {
                            pathpart = pathpart + res[0] + "\\";
                        }

                        if (!Directory.Exists(pathpart + res[1]))
                        {
                            try
                            {
                                Directory.CreateDirectory(pathpart + res[1]);
                                pathpart = pathpart + res[1] + "\\";
                                Directory.CreateDirectory(pathpart + "HsmNtr\\");
                                pathpart = pathpart + "HsmNtr\\";
                                Directory.CreateDirectory(pathpart + "Serialize\\");
                                pathpart = pathpart + "Serialize\\";
                                Directory.CreateDirectory(pathpart + "ScontriniTurni\\");
                                Directory.CreateDirectory(pathpart + "xchk\\");
                                Directory.CreateDirectory(pathpart + "xStorno\\");
                            }
                            catch (Exception ex)
                            {
                                result.ScrollToEnd();
                                result.AppendText(ex.Message + "\n");
                                l4n.myLogger(ex.Message +"\n", "info");
                            }
                        }
                    });

                }
                else
                {
                    result.ScrollToEnd();
                    result.AppendText("La query non ha restituito nessun elemento, riprovare\n");
                    l4n.myLogger("La query non ha restituito nessun elemento, riprovare\n", "info");

                }
            } else
            {
                result.ScrollToEnd();
                result.AppendText("Inserire un ip valido");
                l4n.myLogger("Inserire un ip valido", "info");
            }

        }




        public List<String> GetElements()
        {

            List<String> resultList = new List<String>();
            OracleConnection con = new OracleConnection();
          //  idSuccess = System.Configuration.ConfigurationManager.AppSettings["testStringInsert"]; 
            string pathfinale = "";


            try
            {
                string connectionStr = System.Configuration.ConfigurationManager.AppSettings["connString"]; 
                con.ConnectionString = connectionStr;
                con.Open();
            }
            catch (Exception ex)
            {
                result.ScrollToEnd();
                result.AppendText("ERRORE DI CONNESSIONE AL DB: " + ex.Message + "\n");
                l4n.myLogger("ERRORE DI CONNESSIONE AL DB: " + ex.Message + "\n", "info");
            }

            var commandText = "SELECT SALEDEVICEID, PARAM1 FROM devices WHERE DEVICECLASSID = :devclid AND DEVICECODE IN (2001,2006,2007,2009,2015,2017,2018,2022,2023,2024,2056,2058,2059,2037,2039,2041,2043,2045,2046,2048,2062,2050,2053,2054,2055,2065,2066,2067)";
            using (OracleCommand command = new OracleCommand(commandText, con))
            {
                //parametri

                try
                {
                    command.Parameters.AddWithValue(":devclid", devcid.Text);


                    OracleDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            resultList.Add(reader["SALEDEVICEID"].ToString()+ ";" + reader["PARAM1"].ToString());
                        }

                    }

                }
                catch (Exception ex)
                {
                    result.ScrollToEnd();
                    result.AppendText("ERRORE NELL'INSERT: " + ex.Message + "\n");
                    l4n.myLogger("ERRORE NELL'INSERT: " + ex.Message + "\n", "info");
                }
            }
            con.Close();
            return resultList;

        }

        
       

                   
        

















    }






    
        
}
