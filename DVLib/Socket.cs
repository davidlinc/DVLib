using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO;
namespace DVOSLib
{
	public delegate void subThreadEvent(ServerSubThread thread);
	public delegate void mainThreadEvent(ServerMainThread thread);
	public class ServerSubThread
	{
		internal Thread Thread;
		internal Server server;
		internal Socket socket;

		System.Timers.Timer timer = new System.Timers.Timer(30);
		List<InfoStream> sendList = new List<InfoStream>();
		public event subThreadEvent onEnd = (ServerSubThread Thread) => { }
; static int maxsize = 4096;
		bool end_ = false;
		public static void setMaxSize(int size)
		{
			maxsize = size;
		}

		public ServerSubThread(Server s, Socket socket)
		{
			server = s;

			this.socket = socket;
			onEnd += (ServerSubThread Thread) => { };
			Thread = new Thread(run);
			server.connected(this);

			timer.Elapsed += (object sender, ElapsedEventArgs e) => {
				if (sendList.Count > 0)
				{
					SendInfoStream(sendList[0]);
					sendList.RemoveAt(0);

				}
			};


		}
		public void SendCommand(Command command)
		{
			SendInfoStream(new InfoStream().write(command.name).write(command.arguments).asCommand());
		}
		public void SendCommand(string name, params Object[] objects)
		{
			SendInfoStream(new InfoStream().write(name).write(objects).asCommand());
		}
		public void addToSendList(InfoStream vs)
		{

			sendList.Add(vs);


		}
		public void sendFile(string path, string path2)
		{

			if (File.Exists(path))
			{
				InfoStream s = new InfoStream();
				FileStream f = new FileStream(path, FileMode.Open);
				s.writeFileStream(f, path2);
				SendInfoStream(s);
				f.Close();
			}


		}
		public void addToSendList(params InfoStream[] vs)
		{
			foreach (InfoStream bs in vs)
			{
				sendList.Add(bs);
			}

		}
		public void packToSendList(IEnumerable<InfoStream> list)
		{

			addToSendList(InfoStream.createList(list.ToArray()));


		}

		public void packToSend(IEnumerable<InfoStream> list)
		{

			SendInfoStream(InfoStream.createList(list.ToArray()));


		}

		public void SendNull()
		{
		}
		bool isWorking = false;
		public void WriteSendInfoStream(params Object[] objects)
		{
			SendInfoStream(new InfoStream().write(objects));
		}



		public void SendInfoStream(InfoStream stream)
		{

			isWorking = true;
			byte[] meg2 = stream.getTosend();


			socket.Send(meg2);
			foreach (FileStream file in stream.files)
			{
				byte[] bs = new byte[9];
				bs[0] = (byte)InfoType.FILE;
				long i = file.Length;
				BitConverter.GetBytes(i).CopyTo(bs, 1);

				socket.Send(bs);
				long sent = 0;
				while (sent < i)
				{
					byte[] filePart = new byte[maxsize];
					int r = file.Read(filePart, 0, maxsize);
					sent += r;
					socket.Send(filePart.Take(r).ToArray());
				}
				file.Position = 0;

			}
			isWorking = false;

		}
		public void start()
		{
			Thread.Start();
			timer.Start();
		}
		public void Close()
		{
			try {
				server.threads.Remove(this);
				onEnd(this);
				end_ = true;
				timer.Stop();
				timer.Dispose();

				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				this.Thread.Abort(); }
			catch {

			}


		}
		void putInfo(object info)
		{
			server.putInfo(info);
			if (info is List<object>)
			{
				foreach (object obj in (List<object>)info)
				{
					if (obj is FileStream)
					{
						toReceive.Add((FileStream)obj);
					}
				}
			}
		}
		public void readinfo(byte[] bytes)
		{
			object obj = new InfoStream(bytes).readInfo();
			if (obj is List<object>[])
			{
				foreach (object o in (List<object>[])obj)
				{
					putInfo(o);
					server.receive(o, server, this);

					server.count++;


				}

			}
			else
			{
				putInfo(obj);
				server.receive(obj, server, this);
				if (obj is Command)
				{
					server.receiveCommand((Command)obj, this);
				}

				server.count++;
			}
		}
		public static byte[] mix(byte[] a, byte[] b)
		{
			byte[] r = new
				byte[a.Length + b.Length];
			a.CopyTo(r, 0);
			b.CopyTo(r, a.Length);
			return r;
		}
		internal List<FileStream> toReceive = new List<FileStream>();
		public void run()
		{
			bool start = false;
			byte[] info_b = new byte[0];
			int total = 0;
			bool startFile = false;
			long fileSize = 0;
			long fileReceive = 0;
			while (!end_ && socket.Connected)
			{
				try
				{
					byte[] bytes = new byte[maxsize];



					int i = socket.Receive(bytes);
					bytes = bytes.Take(i).ToArray();
					server.bytes += i;



					if (start)
					{
						i += info_b.Length;
						bytes = mix(info_b, bytes);
						start = false;

					}
					if (!start && !startFile)
					{

						if (i <= 5)
						{
							start = true;
							info_b = bytes;
						}
						else
						{
							if (bytes[0] == (byte)InfoType.HEAD)
							{
								total = (bytes[1] << 24) | (bytes[2] << 16) | (bytes[3] << 8) | (bytes[4]);


								if (total <= i)
								{
									while (total <= i)
									{
										if (i == 0)
										{
											break;
										}
										byte[] result = new byte[total];
										for (int j = 0; j < total; j++)
										{
											result[j] = bytes[j];

										}
										readinfo(result);
										i -= total;
										byte[] nb = new byte[i];
										for (int j = 0; j < i; j++)
										{
											nb[j] = bytes[total + j];

										}
										bytes = nb;

										if (i >= 5)
										{
											total = (bytes[1] << 24) | (bytes[2] << 16) | (bytes[3] << 8) | (bytes[4]);
										}
										else if (i > 0)
										{


											break;

										}


									}
									if (i > 0)
									{
										start = true;
										info_b = bytes;
									}
									if (i == 0)
									{
										start = false;
									}
								}

								else
								{

									start = true;
									info_b = bytes;
								}
							}

							else if (bytes[0] == (byte)InfoType.FILE)
							{
								if (i < 9)
								{
									start = true;
									info_b = bytes;
								}
								else
								{
									startFile = true;
									fileReceive = 0;
									fileSize = BitConverter.ToInt64(bytes, 1);
									bytes = bytes.Skip(9).ToArray();

									i -= 9;
								}
							}



						}


					}
					if (startFile)
					{
						FileStream f = null;
						if (toReceive.Count > 0)
						{
							f = toReceive[0];
						}
						long rest = fileSize - fileReceive;
						if (i <= rest)
						{
							if (f != null)
							{
								f.Write(bytes, 0, i);
								f.Flush();
							}

							fileReceive += i;
						}
						else
						{
							if (f != null)
							{
								f.Write(bytes, 0, (int)rest);
								f.Flush();
							}

							fileReceive += (int)rest;
							info_b = bytes.Skip((int)rest).ToArray();
							start = true;
						}

						if (fileReceive == fileSize)
						{
							startFile = false;
							if (f != null)
							{

								f.Close();
								toReceive.RemoveAt(0);
							}


						}
					}


					if (i < 0)
					{

						throw (new Exception());
					}
					else
					{



					}

				}
				catch
				{
					Close();
				}




			}






			Close();
		}
	}
	public class ServerMainThread
	{
		internal Thread Thread;
		internal Server server;
		bool end_ = false;
		public event mainThreadEvent onEnd = (ServerMainThread Thread) => { };
		public ServerMainThread(Server s)
		{
			this.server = s;
			Thread = new Thread(run);
		}
		public void start()
		{
			Thread.Start();
		}
		public void end()
		{
			end_ = true;
			try
			{
				server.sever.Close();
				Thread.Abort();

			}
			catch
			{

			}

		}

		void run()
		{
			while (!end_)
			{
				try
				{
					Socket socket = server.sever.Accept()
					; ServerSubThread subThread = new ServerSubThread(server, socket);
					server.threads.Add(subThread);
					subThread.start();
				} catch
				{

				}

			}

			end();
			onEnd(this);
		}
	}
	public delegate void InfoEvent(object info, Server
		server, ServerSubThread thread);
	public delegate void ClientInfoEvent(object info, Client client);
	public delegate bool CommandExecute<T>(string name, object[] objects, T client);

	public delegate void CommandEvent(Command info, Server server, ServerSubThread thread);
	public delegate void ClientCommandEvent(Command info, Client client);


	public class CommandExecuter<T>
	{

		Dictionary<string, CommandExecute<T>> commands = new Dictionary<string, CommandExecute<T>>();
		 
		public CommandExecuter()
		{
			register();
		}
		public virtual void register()
		{

		}
		public virtual void registerCommand(string name,CommandExecute<T> command)
		{
			commands.Add(name, command);
		}

		public virtual bool execute(T executer, Command command)
		{
			if(command.cancel)
			{
				return false;
			}
			if (commands.ContainsKey(command.name))
			{
				CommandExecute<T> t = commands[command.name];
				return t(command. name,command.arguments , executer);
			}
			return false;
		}
		public virtual bool execute(T executer,string name, params object[] objects)
		{
			if(commands.ContainsKey(name))
			{
				CommandExecute<T> t = commands[name];
				return t(name,objects,executer);
			}
			return false;
		}

	}
	public class Server :CommandExecuter<ServerSubThread>
	{
	 internal	Socket sever;
		internal ServerMainThread mainThread;
		internal List<Object> infos = new List<object>();
		internal List<ServerSubThread> threads = new List<ServerSubThread>();
		public int bytes { get; internal set; }
		public int count { get; internal set; }

		public ServerSubThread [] getThreads()
		{
			return threads.ToArray();
		}
		public int threadCount { get { return threads.Count; } }
		public event InfoEvent onInfoReceived = new InfoEvent((object obj,Server server1,ServerSubThread thread)=> { });
		public event CommandEvent commandReceived = new CommandEvent((Command obj, Server server1, ServerSubThread thread) => { });

		public event InfoEvent clientConnected = new InfoEvent((object obj, Server server1, ServerSubThread thread) => { });
		Object lastInfo;
		public static void setMaxSize(int size)
		{
			ServerSubThread.setMaxSize(size);
		}
		public void connected(ServerSubThread thread)
		{
			clientConnected(thread, this, thread);
		}
		internal void receive(object info, Server
		server, ServerSubThread thread)
		{
			onInfoReceived(info, server, thread);
		}
		public void receiveCommand(Command command,ServerSubThread thread)
		{
			
			commandReceived(command, this, thread);
			execute(thread, command);
		}
		public Server(int port):base()
		{
			sever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress ip = IPAddress.Any;
			IPEndPoint iPEnd = new IPEndPoint(ip, port);
			sever.Bind(iPEnd);
			sever.Listen(20);
			bytes = 0;

		}
		public Object getInfo()
		{
			if (infos.Count > 0)
			{
				Object ob = infos[0];
				infos.RemoveAt(0);
				return ob;
			}
			return null;
		}
		public ServerSubThread getLast()
		{
			if(threads.Count > 0)
			{
				return threads[threads.Count - 1];
			}
			return null;
		}
		public void SendInfoStream(ServerSubThread thread,InfoStream stream)
		{
			thread.SendInfoStream(stream);
		}
		public void WriteSendInfoStream(ServerSubThread thread,params Object[] objcts)
		{
			thread.SendInfoStream(new InfoStream().write(objcts));
		}
		private void SendCallback(IAsyncResult asyncConnect)
		{

		}
		public void putInfo(Object info)
		{
			lastInfo = info;
			infos.Add(info);
		}
		public void endMainThread()
		{
			if(mainThread !=null)
			{
				
				mainThread.end();
			}

		}
		public void Close()
		{
			try {endMainThread();
			ServerSubThread[] threads1 = new ServerSubThread[threads.Count];
			threads.CopyTo(threads1);
			foreach(ServerSubThread serverSub in threads1)
			{
				serverSub.Close();
			}	 }
			catch
			{

			}
			
		}
		public void startMainThread()
		{
			if(mainThread ==null)
			{
				mainThread = new ServerMainThread(this);
				mainThread.start();
			}
		}
	}

	public class Client :CommandExecuter<Client>
	{

		public Socket socket { get; private set; }
		public Object lastInfo = null;
		public bool receive = false;
		public readonly int port;
		public long bytes { get; internal set; }
		public string error="";
		public int count { get; internal set; }
		Thread thread;
		public static int maxSize =4096;
		public bool success { get; private set; }

		public event ClientInfoEvent onReiceive = (object info,Client client) => { };
		public event ClientCommandEvent onCommand = (Command info, Client client) => { };
		public static void setMaxSize(int size)
		{
			maxSize = size;
		}
		List<Object> infos=new List<object>();
		bool end = false;
		public Object getInfo()
		{
			if(infos.Count>0)
			{
				Object ob = infos[0];
				infos.RemoveAt(0);
				return ob;
			}
			return null;
		}
		public void SendCommand(string name, params Object[] objects)
		{
			SendInfoStream(new InfoStream().write(name).write(objects).asCommand());
		}
		public void SendCommand(Command command)
		{
			SendInfoStream(new InfoStream().write(command.name).write(command.arguments).asCommand());
		}

		public void putInfo(object info)
		{
			lastInfo = info;
			infos.Add(info);
			if(info is List<object>)
			{
				foreach(object obj in (List<object>)info)
				{
					if(obj is FileStream)
					{
						toReceive.Add((FileStream)obj);
					}
				}
			}
		}
		public static readonly IPAddress Local=IPAddress.Parse("127.0.0.1");
		public static readonly IPAddress Server = IPAddress.Parse("110.42.64.59");

		public static readonly int LocalPort = 25565;
		public static readonly int ServerPort =1895;

		public void cleanCount()
		{
			count =0;
			bytes = 0;
		}
	
		public void sendFile(string path, string path2)
		{
			path = path.Trim('‪');
			path2= path2.Trim('‪');
			if (File.Exists(path))
			{
				InfoStream s = new InfoStream();
				FileStream f = new FileStream(path, FileMode.Open);
				s.writeFileStream(f, path2);
				SendInfoStream(s);
				f.Close();
			}


		}
		public Client(IPAddress ip, int port,int wait) : base()
		{
			bytes = 0;
			count = 0;
			this.port = port;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			//服务器端口
			IPEndPoint endpoint = new IPEndPoint(ip, port);
			//异步连接,连接成功调用connectCallback方法
			IAsyncResult result = socket.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), socket);
			//这里做一个超时的监测，当连接超过5秒还没成功表示超时
			success = result.AsyncWaitHandle.WaitOne(wait, true);
			if (!success)
			{
				//超时
				Closed();
			}
			else
			{
				//与socket建立连接成功，开启线程接受服务端数据。
				thread = new Thread(new ThreadStart(ReceiveSocket));
				thread.IsBackground = true;
				thread.Start();
				success = true;
			}
		}
		public Client(IPAddress ip,int port):base()
		{
			bytes = 0;
			count = 0;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.port = port;
			//服务器端口
			IPEndPoint endpoint = new IPEndPoint(ip, port);
			//异步连接,连接成功调用connectCallback方法
			IAsyncResult result = socket.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), socket);
			//这里做一个超时的监测，当连接超过5秒还没成功表示超时
			 success = result.AsyncWaitHandle.WaitOne(5000, true);
			if (!success)
			{
				//超时
				Closed();
			}
			else
			{
				//与socket建立连接成功，开启线程接受服务端数据。
			thread = new Thread(new ThreadStart(ReceiveSocket));
				thread.IsBackground = true;
				thread.Start();
				success = true;
			}
		}


		
		private void ConnectCallback(IAsyncResult asyncConnect)
		{

		}

		public static byte[] mix(byte[] a,byte[]b )
		{
			byte[] r = new
			byte[a.Length + b.Length];
			a.CopyTo(r, 0);
			b.CopyTo(r, a.Length);
			return r;
		}

			
			 void readinfo(byte[] bytes)
			{
				object obj = new InfoStream(bytes).readInfo();
				if (obj is List<object>[])
				{
					foreach (object o in (List<object>[])obj)
					{
						putInfo(o);
						onReiceive(o, this);
						count++;
					}

				}
				else
				{
					putInfo(obj);
					onReiceive(obj, this);
				if(obj is Command)
				{
					
					onCommand((Command)obj, this);
					
					execute(this, (Command)obj);
				}
					count++;
				}
			
		}
	internal	List<FileStream> toReceive = new List<FileStream>();
		private void ReceiveSocket()
		{
			bool start = false;
	
			byte[] info_b=new byte[0];
					bool startFile = false;
			long fileSize=0;
			long fileReceive=0;
			//在这个线程中接受服务器返回的数据
			while (!end&&socket.Connected)
			{

			
				try
				{
					//接受数据保存至bytes当中
					byte[] bytes = new byte[maxSize];
					int i = socket.Receive(bytes);
					bytes = bytes.Take(i).ToArray();

					this.bytes += i;
			
					if(i<0)
					{
						Closed();
						throw (new Exception());
					}
				else if(i>0)
					{
if(start)
					{
						i += info_b.Length;
bytes = mix(info_b, bytes);
start = false;

					}
					if(!start&&!startFile)
					{
						
						if(i<=5)
						{
							start = true;
							info_b = bytes;
						}
						else
						{

if(bytes[0]==(byte)InfoType.HEAD)
								{int
total = (bytes[1] << 24) | (bytes[2] << 16) | (bytes[3] << 8) | (bytes[4]);

						
	if(total<=i)
							{
while(total<=i)
						{
									if(i==0)
									{
										break;
									}
							byte[] result = new byte[total];
							for(int j=0;j<total; j++)
							{
								result[j] = bytes[j];
								
							}
							readinfo(result);
							i -= total;
								byte[] nb = new byte[i];
								for (int j = 0; j < i; j++)
								{
									nb[j] = bytes[total+j];

								}
								bytes = nb;

								if(i>=5)
								{
	                            total = (bytes[1] << 24) | (bytes[2] << 16) | (bytes[3] << 8) | (bytes[4]);
								}
								else if(i>0)
								{
									
										
										break;

								}
							

							}
if(i>0)
								{
									start = true;
									info_b = bytes;
								}
if(i==0)
								{
									start = false;
								}
						}

	else
							{
								
								start = true;
								info_b = bytes;
							}
								}
else if(bytes[0]==(byte)InfoType.FILE)
								{
if(i<9)
									{
										start = true;
										info_b = bytes;
									}
else
									{
										startFile = true;
										fileReceive = 0;
										fileSize = BitConverter.ToInt64(bytes, 1);
										bytes = bytes.Skip(9).ToArray();
						
										i -= 9;
									}
								}
							}
						
						
					}
					if(startFile)
						{
							FileStream f=null;
							if (toReceive.Count > 0)
							{
								f = toReceive[0];
							}
								long rest = fileSize - fileReceive;
							if(i<=rest)
							{
								if(f!=null)
								{   f.Write(bytes, 0, i);
								f.Flush();
								}
                        
								fileReceive += i;
							}
							else
							{
								if (f!=null)
								{
	                               f.Write(bytes, 0, (int)rest);
									f.Flush();
								}
								
								fileReceive += (int)rest;
								info_b = bytes.Skip((int)rest).ToArray();
								start = true;
							}

							if(fileReceive==fileSize)
							{
								startFile = false;
								if(f!=null)
								{
									
									f.Close();
									toReceive.RemoveAt(0);
								}
							
								
							}
						}

					if (i < 0)
					{

						end = true;
								break;
					}
					else
					{

					
						
					}
					
					}
					

					
				}
				catch (Exception e)
				{
					error = e.ToString();
					end = true;
					break;
				}
			}
			Closed();
			
		}

		//关闭Socket
		public void Closed()
		{
			try
			{
      
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();      
			thread.Abort();
			end = true;
			}
			catch
			{

			}
			
		}
		//向服务端发送一条字符
		public void WriteSendInfoStream(params Object[] objects)
		{
			SendInfoStream(new InfoStream().write(objects));
		}
		public void SendInfoStream(InfoStream stream)
		{
			byte[] meg2 = stream.getTosend();


			socket.Send(meg2);
			foreach (FileStream file in stream.files)
			{
				byte[] bs = new byte[9];
				bs[0] = (byte)InfoType.FILE;
				long i = file.Length;
				BitConverter.GetBytes(i).CopyTo(bs, 1);

				socket.Send(bs);
				long sent = 0;
				while (sent < i)
				{
					byte[] filePart = new byte[maxSize];
					int r = file.Read(filePart, 0, maxSize);
					sent += r;
					socket.Send(filePart.Take(r).ToArray());
				}
				file.Position = 0;

			}


		}
		private void SendCallback(IAsyncResult asyncConnect)
		{

		}
	}
}
