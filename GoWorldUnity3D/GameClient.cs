﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace GoWorldUnity3D
{
    public class GameClient
    {
        public static GameClient Instance = new GameClient();
        private TcpClient tcpClient;
        private DateTime startConnectTime = DateTime.MinValue;

        public string Host { get; private set; }
        public int Port { get; private set; }

        public GameClient()
        {
        }

        public override string ToString()
        {
            return "GameClient<" + this.Host + ":" + this.Port + ">";
        }

        public void Connect(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            this.disconnectTCPClient();
        }

        public void Disconnect()
        {
            this.Host = "";
            this.Port = 0;
            this.disconnectTCPClient();
        }

        private void disconnectTCPClient()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Close();
                this.tcpClient = null;
                this.startConnectTime = DateTime.MinValue;
                this.debug("Disconnected");
            }
        }

        private void debug(string msg)
        {
            Console.WriteLine(this + " - " + msg);
        }

        public void Tick()
        {
            if (this.Host == "")
            {
                this.disconnectTCPClient();
            }
            else
            {
                this.assureTCPClientConnected();
                this.tryRecvNextPacket();
            }
        }

        private void tryRecvNextPacket()
        {
            if (this.tcpClient.Available == 0)
            {
                return; 
            }

            this.debug("Available: " + this.tcpClient.Available);
        }

        private void assureTCPClientConnected()
        {
            if (this.tcpClient != null)
            {
                this.checkConnectTimeout();
                return;
            }

            // no tcpClient == not connecting, start new connection ...
            this.debug("Connecting ...");
            this.tcpClient = new TcpClient();
            this.tcpClient.NoDelay = true;
            this.tcpClient.SendTimeout = 5000;
            this.tcpClient.ReceiveBufferSize = 8192;
            this.startConnectTime = DateTime.Now;
            this.tcpClient.BeginConnect(this.Host, this.Port, this.onConnected, null);
            
        }

        private void checkConnectTimeout()
        {
            Debug.Assert(this.tcpClient != null);
            if (this.tcpClient.Connected)
            {
                return;
            }

            if (DateTime.Now - this.startConnectTime > TimeSpan.FromSeconds(5))
            {
                this.disconnectTCPClient();
            }
        }

        private void onConnected(IAsyncResult ar)
        {
            if (this.tcpClient.Connected)
            {
                this.debug("Connected " + this.tcpClient.Connected);
            }
            else
            {
                this.debug("Connect Failed!");
                this.disconnectTCPClient();
            }
        }
    }
}
