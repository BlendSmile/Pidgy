using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;      
using System.Net.Sockets;  

using System.Threading;

namespace ServerTest
{
    class Program
    {

        static void Main(string[] args) {
            Program main = new Program();
            main.server_start();  //starting the server

            Console.ReadLine();  
        }

        TcpListener server = new TcpListener(IPAddress.Any, 9999);   

        private void server_start() {
            server.Start();    
            accept_connection();  //accepts incoming connections
        }

        private void accept_connection() {
            server.BeginAcceptTcpClient(handle_connection, server);  //this is called asynchronously and will run in a different thread
        }

        private void handle_connection(IAsyncResult result) {  //the parameter is a delegate, used to communicate between threads
        
            accept_connection();  //once again, checking for any other incoming connections
            TcpClient client = server.EndAcceptTcpClient(result);  //creates the TcpClient

            NetworkStream ns = client.GetStream();

	    byte[] msg = new byte[256];
            ns.Read(msg, 0, msg.Length);
	    StringBuilder sb = new StringBuilder(Encoding.ASCII.GetString(msg));
	    if(sb[0] == '8') {
			sb.Remove(0, 1);
			Console.WriteLine(sb);
			ns.Write(Encoding.ASCII.GetBytes("" + AcceptLogin(sb.ToString(), "/path/to/file", "/path/to/file"))); //TODO: add hash and cryptography thing to the password + username file
	     }
        }
	public static int AcceptLogin(string msg, string pathToUsr, string pathToPass) {
		string[] usernameList = File.ReadAllLines(pathToUsr);
		string[] passList = File.ReadAllLines(pathToPass);

		//parse the login message
		string[] parsedStr = msg.Split(' ');
		string username = parsedStr[0].Replace(" ", "");
		string password = parsedStr[1].Trim();
		//find the username and passwird in the database

		int code = 1;

		for(int i = 0; i < usernameList.Length; i++) {
			if(username == usernameList[i]) { //if username found, return code 2
				code = 2;
				if(password == passList[i]) {
					code = 0; //if password correct too return code 0
				}
				break;
			}
		}
		return code;
	}
    }
}

