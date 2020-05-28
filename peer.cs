//this software is distributed under the gnu public license in hope
//that it will be useful, please don't misuse the source code.
//anyway! Have fun reading it 
//-Blend_Smile (Muhammad Alif)
//PS- i wrote this fully with vim in tty ^-^

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;
class peer {
	public static void Main(string[] args) {	
		string serverAddr = "192.168.0.0";
		int serverPort = 9000;
		
		string addr = "localhost";
		int port = 6060;
		int listenPort = 6061;

		//start listen for command from the server or for incoming udp connection 
		string[] whitelistAddr = {"114.125.21.221"}; //TODO:REMOVE THIS BEFORE COMMITING TO GITHUB OR EVERYONE CAN SEE YOUR PUBLIC ADDRESS
		Listener listener;
		Thread udpListener;
		Thread tcpListener;
		
		if(args.Length != 3) {
			Console.WriteLine("Usage: Peer <Target email/username> <ServerAddress> <ServerPort> <ListenPort>");
		} else {
			listener = new Listener(listenPort, whitelistAddr);
			tcpListener = new Thread(listener.listenForTcpConnection);
			udpListener = new Thread(listener.listenForUdpConnection);

			addr = args[0];
			port = Int32.Parse(args[1]);
			listenPort = Int32.Parse(args[2]);

			Console.WriteLine("Target Email/Username: " + addr);
			Console.WriteLine("Server Address: " + serverAddr);
			Console.WriteLine("Server Port: " + serverPort);
			Console.WriteLine("Listening on Port: " + listenPort);

			//start listening on port

			string addrIP = getIpFromUsername(addr, serverAddr, serverPort);
			Console.Write(addrIP); //for debugging purpose only, pls remove this later
			//try to connect to targetted peer, if it's inside a NAT, then it will probably not working
			if(isPeerAvailable(addr, port)) {
				//peer is available, we can communicate easily
				Console.WriteLine("peer is not behind a NAT, connecting...");
				Sender.sendUdpMessage("connected!", addrIP, port);
			} else {
				//peer is not available, it's probably behind a NAT or offline, however, we will try to connect with the skye protocol
				Console.WriteLine("the targetted address is behind a NAT");
				Console.WriteLine("trying to connect with the skype protocol...");
				requestToNotifyPeer(addr, serverAddr, serverPort);
				//wait for opposite connection
				IPEndPoint iep = new IPEndPoint(IPAddress.Any, listenPort);
				UdpClient c = new UdpClient(iep);
				c.Receive(ref iep);
				//check the source IP (for security)
				if(iep.Address.ToString() == addr) { //is the remote address is the address we want to connect with?
					//if yes, send back a UDP connection to the address
					Sender.sendUdpMessage("connected", addr, port);
					//connection established!
				}
			}
		}
	}	
	public static bool isPeerAvailable(string addr, int port) {
		try {
			TcpClient c = new TcpClient(addr, port);
			c.Connect(addr, port);
			return true;
			c.Close();
		} catch(SocketException e) {
			return false;
		}
	}
	public static string getIpFromUsername(string username ,string serverAddr, int serverPort) {
		//connect to server and request the targetted IP from the email/username using tcp
		TcpClient c = new TcpClient(serverAddr, serverPort);
		byte[] b = Encoding.ASCII.GetBytes("0-" + username); //encode the specified username to ascii bytes and use '0' as code
		NetworkStream ns = c.GetStream();
		ns.Write(b, 0, b.Length); //send the username as ascii byte
		//get the ip of sended username from server
		b = new Byte[256]; //to store response bytes
		Int32 bytes = ns.Read(b, 0, b.Length); //read from server
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < bytes; i++) {
			sb.Append(Convert.ToChar(b[i]));
		}
		return sb.ToString();
	}
	public static void requestToNotifyPeer(string username, string serverAddr, int serverPort) {
		//send request to server to notify a peer to server with TCP
		TcpClient c = new TcpClient(serverAddr, serverPort);
		c.Connect(serverAddr, serverPort);
		byte[] b = Encoding.ASCII.GetBytes("1-" + getIpFromUsername(username, serverAddr, serverPort)); //encode the specified username to ascii bytes and use '1' as code
		NetworkStream ns = c.GetStream();
		ns.Write(b, 0, b.Length); //send 
	}
}
public class Listener{
	int port;
	string[] whitelistedAddr;
	public Listener(int PORT, string[] WLADDR) {
		port = PORT;
		whitelistedAddr = WLADDR;
	}
	public void listenForTcpConnection() {
		TcpListener l = new TcpListener(IPAddress.Any, port);
		l.Start();
		Socket s = l.AcceptSocket();
		IPEndPoint iep = s.RemoteEndPoint as IPEndPoint;
		//check if the connected server is on the whitelisted address (for security)
		for(int i = 0; i < whitelistedAddr.Length; i++) {
			if(iep.Address.ToString() == whitelistedAddr[i]){
				//ip is on the whitelisted address! allow to connect
				Console.WriteLine("Connected with" + s.RemoteEndPoint);

				byte[] b = new byte[100];
				StringBuilder sb = new StringBuilder();
				int k = s.Receive(b);
				for(int x = 0; x < k; i++) {
					sb.Append(b[x]);
				}
				if(sb[0] == '1' && sb[1] == '-') {
					//received command to connect to an ip from server!
					sb.Remove(0, 2);		
				}
			} 
			if(iep.Address.ToString() != whitelistedAddr[i] && i == whitelistedAddr.Length - 1) {
				//ip is not listen on the whitelisted address, ignore...
				Console.WriteLine("Connected with " + s.RemoteEndPoint + " but it's not a whitelisted/trusted address for security reason");
			}
		}
	}

	public void listenForUdpConnection() {
		IPEndPoint iep = new IPEndPoint(IPAddress.Any, port);
		UdpClient c = new UdpClient(iep);

		//start listen for connection
		while(true) {
			byte[] b = c.Receive(ref iep);
			string s = Encoding.ASCII.GetString(b);
			Console.WriteLine(s);
		}
	}

}
public class Sender{
	public static void sendTcpMessage(string msg, string ip, int port) {
		TcpClient c = new TcpClient(ip, port);
		c.Connect(ip, port);
		Stream strm = c.GetStream();
		strm.Write(Encoding.ASCII.GetBytes(msg), 0, Encoding.ASCII.GetBytes(msg).Length);
	}
	public static void sendUdpMessage(string msg, string ip, int port) {
		Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		byte[] b = Encoding.ASCII.GetBytes(msg);
		s.Send(b);
	}
}
