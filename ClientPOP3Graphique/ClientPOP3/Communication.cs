using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientPOP3
{
    public static class Communication
    {
        private static ClientPOP3 clientPOP3;

        private static TcpClient socketClient;

        private static StreamReader sr;
        private static StreamWriter sw;

        #region Méthodes de lecture et écriture d'une ligne dans la socket de communication
        private static string LireLigne()
        {
            // Lecture d'une ligne dans le Stream associé à la socket (en provenance du serveur POP3)
            string ligne = sr.ReadLine();
            // Affichage dans la fenêtre VERBOSE
            clientPOP3.WriteVerbose("recu  >> " + ligne);
            return ligne;
        }
        private static void EcrireLigne(string ligne)
        {
            // Ecriture d'une ligne dans le Stream associé à la socket (à destination du serveur POP3)
            sw.WriteLine(ligne);
            // Affichage dans la fenêtre VERBOSE
            clientPOP3.WriteVerbose("envoi << " + ligne);
        }
        #endregion

        public static void Initialise(ClientPOP3 client)
        {
            // Besoin d'un accès à la vue pour les affichages, utilisateur et verbose
            clientPOP3 = client;

            // Connexion au serveur
            socketClient = new TcpClient();   // équivaut à la primitive Socket (avec mode TCP)
            socketClient.Connect(Preferences.nomServeur, Preferences.port);

            // Mise en place des Streams pour lecture et écriture par ligne sur la socket
            sr = new StreamReader(socketClient.GetStream(), Encoding.UTF8); // caractères accentués dans les mails
//            sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
            sw = new StreamWriter(socketClient.GetStream(), Encoding.Default)
            {
                AutoFlush = true
            };
        }

        public static void Identification()
        {
            string ligne, tampon;

            /* réception banniere +OK ... */
            ligne = LireLigne();
            if (!ligne[0].Equals('+'))
            {
                MessageBox.Show("Pas de banniere. Abandon");
                Environment.Exit(1);
            };

            /* envoi identification */
            tampon = "USER " + Preferences.username;
            EcrireLigne(tampon);
            ligne = LireLigne();
            if (!ligne[0].Equals('+'))
            {
                MessageBox.Show("USER rejeté. Abandon");
                Environment.Exit(1);
            };

            /* envoi mot de passe */
            tampon = "PASS " + Preferences.password;
            EcrireLigne(tampon);
            ligne = LireLigne();
            if (!ligne[0].Equals('+'))
            {
                MessageBox.Show("PASS rejeté. Abandon");
                Environment.Exit(1);
            }

            clientPOP3.SetMaximumN(getStat()[0]);

        }

        public static void Quit()
        {
            string ligne, tampon;

            /* envoi QUIT pour arrêter l'échange avec le serveur */
            tampon = "QUIT";
            EcrireLigne(tampon);
            ligne = LireLigne(); // lecture du +OK

            // Fermeture de la socket de communication
            socketClient.Close();
        }

        public static void Stat()
        {
            int[] stat = getStat();
            if(stat != null)
            {
                int nombreMessages = stat[0];
                int tailleBoite = stat[1];
                clientPOP3.WriteAffichage("Il y a " + nombreMessages.ToString() + " messages dans la boite.");
                clientPOP3.WriteAffichage("La taille totale est de " + tailleBoite.ToString() + " octets.");
            }
            
        }

        public static int[] getStat()
        {
            string ligne, tampon;
            int[] lesValeurs = null;
            /* envoi STAT pour récupérer nb messages */
            tampon = "STAT";
            EcrireLigne(tampon);
            /* réception de +OK nombreMessages tailleBoite */
            ligne = LireLigne();
            if (!ligne[0].Equals('+'))
            {
                MessageBox.Show("ERR : STAT a échoué");
            }
            else
            {
                /* découpage pour récupérer nombreMessages et tailleBoite, et les afficher pour l'utilisateur */
                lesValeurs = new int[2];
                String[] lesValeursTexte = ligne.Split(' ');
                lesValeurs[0] = Int32.Parse(lesValeursTexte[1]);
                lesValeurs[1] = Int32.Parse(lesValeursTexte[2]);
            }
            return lesValeurs;
        }

        public static List<String> getRetr(int n)
        {
            List<String> retr = null;
            // *** bloc a decommenter  : ctrl+e puis u  ***
            string ligne, tampon;
            tampon = "RETR " + n.ToString();
            EcrireLigne(tampon);
            /* réception de +ok .... */
            ligne = LireLigne();
            if (!ligne[0].Equals('+'))
            {
                MessageBox.Show("err : list a échoué");
            }
            else
            {
                /* lecture liste ligne par ligne jusqu'au "." final seul sur une ligne */
                retr = new List<string>();
                ligne = LireLigne();
                while (!ligne.Equals("."))
                {
                    string ligneFinale = ligne.ToString();
                    if (!ligneFinale.Equals("") && ligneFinale[0] == '.') ligneFinale = ligneFinale.Substring(1);
                    retr.Add(ligneFinale);
                    ligne = LireLigne();
                }
            }
            return retr;
        }

        public static void Retr(int n)
        {
            List<String> retr = getRetr(n);
            if(retr != null)
            {
                clientPOP3.WriteAffichage("Affichage du RETR " + n.ToString() + ":");
                foreach (String ligne in retr)
                {
                    clientPOP3.WriteAffichage(ligne);
                    clientPOP3.WriteVerbose(ligne);
                }
            }
        }

        /* Récupère et affiche la liste des messages */
        public static void List()
        {
            //// *** BLOC A DECOMMENTER  : Ctrl+e puis u  ***
            string ligne, tampon;
            tampon = "LIST";
            EcrireLigne(tampon);
            /* réception de +OK .... */
            ligne = LireLigne();
            if (!ligne[0].Equals('+'))
            {
                MessageBox.Show("ERR : LIST a échoué");
            }
            else
            {
                /* lecture liste ligne par ligne jusqu'au "." final seul sur une ligne */
                ligne = LireLigne();
                while (!ligne.Equals("."))
                {
                    /*** A COMPLETER  
                     * - afficher la ligne pour l'utilisateur
                     * - lire la ligne suivante
                     * ***/
                }
            }
        }
        public static void Top()
        {
            string ligne, tampon;
            tampon = "TOP 1 5";
            EcrireLigne(tampon);
            /* réception de +OK .... */
            ligne = LireLigne();
            if (ligne[0].Equals('-'))
            {
                MessageBox.Show("ERR : TOP a échoué");
            }
            else
            {
                /* lecture liste ligne par ligne jusqu'au "." final seul sur une ligne */
                while (!ligne.Equals("Content"))
                {
                    ligne = LireLigne();
                    if (ligne.StartsWith("Date:") || ligne.StartsWith("From:") || ligne.StartsWith("Subject:"))
                    {
                        
                        string date = ligne.Replace("Date:", "Ecrit le:");
                        string from = ligne.Replace("From:", "Expediteur:");
                        string subject = ligne.Replace("Subject:", "Sujet:");

                        MessageBox.Show(date, from);
                        MessageBox.Show(subject);
                    }
                    
                }
            }
        }
    }
}
