using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Peer {
	//start program
	static void Main(string[] args) {
		string serverAddr = args[0];

		//login
login:
		Console.WriteLine("Username:");
		string username = Console.ReadLine();
		if(username.Contains(" ")) {
			Console.WriteLine("no whitespace allowed!");
			goto login;			
		}
		Console.WriteLine("Password:");
		string password = Console.ReadLine();

		//send login data to server and receive response
		int result = Login(serverAddr, username, password + " ");
		//0 = succeed, 1 = username not found, 2 = password incorrect, 3 = internal server error
		if(result == 0) {
			Console.WriteLine("Welcome back, " + username + "!");
		}
		if(result == 1) {
			Console.WriteLine("Sorry, we can't find that username");
			Environment.Exit(0);
		}
		if(result == 2) {
			Console.WriteLine("Wrong password");
			Environment.Exit(0);
		}
		if(result == 3) {
			Console.WriteLine("Internal Server Error, it's not your fault, its ours, sorry (try again later)");
			Environment.Exit(0);
		}	
	}
	public static int Login(string addr, string username, string password) {
		int port = 9999;
		//create a TCP connection to server (code 8 + username + "whitespace" + password) 		
		TcpClient c = new TcpClient(); 
		c.Connect(addr, port); //connect
		Byte[] b = Encoding.ASCII.GetBytes("8" + username + " " + password); //convert to byte array
		NetworkStream ns = c.GetStream();
		ns.Write(b, 0, b.Length); //send the mesage

		//receive TCP response from server (0 - succeed, 1 - username not found, 2 - incorrect password)
		Int32 b2 = ns.Read(b, 0, b.Length);
		string response = Encoding.ASCII.GetString(b, 0, b2);//receive response

		//close connection 
		ns.Close();
		c.Close();

		return Int32.Parse(response[0].ToString());
	}
	/*public static int availablePort(string addr, int min, int max) {
		//find available port
		Random rnd = new Random();
		int port = rnd.Next(max);
		bool available = isPortAvailable(addr, port);

		while(!available) {
			rnd = new Random();
			port = rnd.Next(max);
			available = isPortAvailable(addr, port);
		}
		if(available) {
			return port;
		} else {
			return 0;
		}
	}
	public static bool isPortAvailable(string addr, int port) {
		//try to connect to the specified port to check if it's available
		try {
			//connect
			TcpClient c = new TcpClient(addr, port);
			return true;
		} catch (SocketException e) {
			return false;
		}
	}*/
}

