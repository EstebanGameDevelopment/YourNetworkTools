package Main;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;
import java.net.ServerSocket;
import java.util.Vector;

import GameComms.*;

public class ServerGame extends Thread {

	public static final Boolean BASIC_MESSAGES    = false; 

	public static final String TOKEN_SEPARATOR_EVENTS    		= "%"; 
	public static final String TOKEN_SEPARATOR_PARTY    		= "@"; 
	public static final String TOKEN_SEPARATOR_PLAYERS_IDS		= ","; 

	public static final String URL_FREE_HOST = "https://www.yourvrexperience.com/yournetworkingtools/FreeHostRoomHTTP.php?";
	public static final String URL_COMPLETED_GAME = "https://www.yourvrexperience.com/yournetworkingtools/CompletedRoomHTTP.php?";
	
	private boolean m_isRunning = false;
	private ServerSocket m_serverSocket;
	
	private static ServerClients m_serverClients;
	
	private static String IpAddress;
	private static int PortAddress;
	
	public void StartServerGame(String _ipAddress, int _portAddress) throws Exception 
    {	
	    // int port = 8745;
		IpAddress = _ipAddress;
		PortAddress = _portAddress;
	    m_serverSocket = new ServerSocket(PortAddress);
	    System.out.println("+++++++++++++++++++Started Gamer Server(Version "+ Server.SERVER_VERSION + ") on port " + PortAddress + "!");
	
	    m_serverClients = new ServerClients();
        m_serverClients.start();

	    m_isRunning = true;
    }
	
	public void StopServerGame() 
    {
    	m_isRunning = false;
    	try { m_serverSocket.close(); } catch (Exception err) {};
    	try { m_serverClients.Destroy(); } catch (Exception err) {};    	
    }
	
	@Override
	public void run() 
	{
		if (m_serverSocket == null) return;
		
        while (m_isRunning)
        {
        	try 
        	{
	            try 
	            {
					Socket clientSocket = m_serverSocket.accept();
					m_serverClients.AddNewClient(clientSocket);
	            } catch (Exception err)
	            {
	            	err.printStackTrace();
	            }
	            if (ServerGame.BASIC_MESSAGES)
	            {
	            	System.out.println("Accepted connection from client");	
	            }	            
        	} catch (Exception err)
        	{}
        }    	
        System.out.println("ServerGame::COMPLETE TOTAL DISCONNECTION FROM EVERYTHING!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }
}



