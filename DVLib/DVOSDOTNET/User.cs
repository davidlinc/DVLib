using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
namespace DVOSLib
{
	public delegate void PostText(string text);
	public delegate void login(string text,string code);
	public delegate void ReceiveMessage(MessageBox bos, Message message, bool isNew)
		;
	public class Message
	{
		public Message(DateTime time,string name,string text)
		{
			this.time = time;
			this.text = text;
			this.name = name;
		}
	public DateTime time { get; private set; }
	public	string text { get; private set; }
		public string name { get; private set; }

		public override string ToString()
		{
			return time.ToString()+"\n"+name+": "+text;
		}
	}
	
	public class MessageBox: IxmlObject<MessageBox>
	{
		internal List<string> users = new List<string>();
	 internal	List<Message> texts = new List<Message>();

	public	IOrderedEnumerable<Message> getMessages()
		{
			return (from Message m in texts orderby m.time ascending select m);
		}
		public MessageBox()
		{
		}
		public bool isWith(params string[] names)
		{
			List<string> s = new List<string>()
			;
			foreach(string name in names)
			{
				s.Add(name);
			}
			if (names.Length == users.Count)
			{
				foreach (string name in users)
				{

					if (name != null )
					{
						bool flag = false;
						for(int i=0;i<s.Count;i++)
						{
							if (s[i].Equals(name))
							{
								flag = true;
								s.RemoveAt(i);
								break;
							}
						}
						if(!flag)
						return false;
					}
					else
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		public void addUser(User user)
		{
			if(user!=null&&user.name!=null)
			users.Add(user.name);
		}
		public void addUser(string user)
		{
		if(user!=null)
				users.Add(user);
		}
		public Message addMessage(string User,string text)
		{
			Message m = new Message(DateTime.Now, User, text);
			texts.Add(m);
			return m;
		}
	public	MessageBox readXml(XmlElement element)
		{
			users.Clear();
			texts.Clear();
			foreach(XmlElement element1 in element.ChildNodes)
			{
				if(element1.Name.Equals("Users"))
				{
					foreach(XmlElement element2 in element1.ChildNodes)
					{
						
					
							users.Add(element2.GetAttribute("Name"));
						
					}
				}
				else if(element1.Name.Equals("Texts"))
				{
					foreach (XmlElement element2 in element1.ChildNodes)
					{
						texts.Add(new Message(DateTime.Parse(element2.GetAttribute("Time")), element2.GetAttribute("Name"), element2.GetAttribute("String")))
							;
					}
				}
			}
			return this;
		}

	public	XmlElement writeXml(XmlElement element)
		{
			XmlDocument xml = element.OwnerDocument;
			XmlElement us = xml.CreateElement("Users");
			XmlElement ts = xml.CreateElement("Texts");
			element.AppendChild(us);
			element.AppendChild(ts);
			foreach(string user in users)
			{
				XmlElement u = xml.CreateElement("User");
				u.SetAttribute("Name", user);
				us.AppendChild(u);
			}
			foreach(Message s in texts)
			{
				XmlElement u = xml.CreateElement("Text");
				u.SetAttribute("String", s.text);
				u.SetAttribute("Name", s.name);
				u.SetAttribute("Time", s.time.ToString());
				ts.AppendChild(u);
			}
			return element;
		}
	}
	public class UserClient
	{
		public bool success { get; private set; }
		public event ReceiveMessage onMessage = (MessageBox s,Message mes,bool isNew) =>{};
		public event PostText onTextPost;
		public event login onLogin;

		public Client client { get; private set; }
	public string username { get; private set; }
		public string lastName { get; private set; }
		public string lastCode { get; private set; }
		public string lastBox = "";
		public string notice { get; private set; }
	public bool logined { get; private set; }
		public bool b { get; private set; }
		List<MessageBox> messages = new List<MessageBox>();
		List<string> users = new List<string>();
		public int BoxCount { get { return messages.Count; } }
	public	List<string> texts = new List<string>();

		public void cleanBox()
		{
			messages.Clear();
		}
		public string[] getUserNames()
		{
			return users.ToArray();
		}
		
	
	
		public MessageBox findBox(params string[] names)
		{
			foreach (MessageBox box in messages)
			{
				if (box.isWith(names))
				{
					return box;
				}
			}
			MessageBox box1 = new MessageBox();
			foreach (string s in names)
			{
			
				box1.addUser(s);
			}
			messages.Add(box1);
			return box1;
		}
		public UserClient(IPAddress ip,int port) 
		{

			lastCode = "";
			lastName = "";
			client = new Client(ip, port);
			client.onReiceive += onReiceive;
			b = false;
			logined = false;
			success = client.success;
			onTextPost = (string text) => { texts.Add(text); };
			onLogin = (string s, string c) => { logined = true; };
		}
	
		public void login(string name,string key)
		{
			InfoStream info = new InfoStream();
			info.setInfoType(InfoType.OBJECTS);
			info.writeString("Login");
			info.writeString(name);
			info.writeString(key);
			client.SendInfoStream(info);
		}
		public void register(string name, string key)
		{
	InfoStream info = new InfoStream();
			info.setInfoType(InfoType.OBJECTS);
			info.writeString("Register");
			info.writeString(name);
			info.writeString(key);
			client.SendInfoStream(info);
		}

	
		public void onReiceive(object info, Client client)
		{
			if (info is List<object>)
			{
				List<object> info_ = (List<object>)info;

				if (info_.Count > 0)
				{
					if (info_[0].Equals("ReturnB"))
					{
						b = (bool)info_[1];
					}
					else if (info_[0].Equals("ReturnLogin"))
					{
						username = (string)info_[1];
						lastCode= (string)info_[2];
						lastName=(string)info_[1];
						onLogin(username, lastCode);
					}
					else if (info_[0].Equals("ReturnNotice"))
					{
						notice = (string)info_[1];
					}
					else if (info_[0].Equals("GetFile"))
					{
					client.	sendFile((string)info_[1], (string)info_[2]);
					}
					else if (info_[0].Equals("NewMessage"))
					{
						MessageBox box = findBox((string[])info_[1]);
						if (box != null)
						{
							Message mes = new Message(DateTime.Parse((string)info_[2]), (string)info_[3], (string)info_[4]);
							box.texts.Add(mes);
							onMessage(box,mes,(bool)info_[5]);
						}
						
					}
					else if (info_[0].Equals("ReturnUsers"))
					{
						string[] vs = (string[])info_[1];
						users.Clear(); foreach (string s in vs)
						{
							users.Add(s);
						}
					}


				}

			}
		
		}
		public void sendTo(string name,string text)
		{
			client.SendInfoStream(new InfoStream().write("SendTo", name,text));
		}
		public void getBox(string name)
		{
			client.SendInfoStream(new InfoStream().write("GetBox", name));
		}
		public void loadFromXml(XmlElement element)
		{
		
			messages.Clear();
			foreach (XmlElement element1 in element.ChildNodes)
			{
				 if (element1.Name.Equals("MessageBoxs"))
				{
					foreach (XmlElement element2 in element1.ChildNodes)
					{
						messages.Add(new MessageBox().readXml(element2));

					}
				}
				 else if(element1.Name.Equals("User"))
				{
					lastCode = element1.GetAttribute("Key");
					lastName = element1.GetAttribute("Name");
					lastBox= element1.GetAttribute("Box");
				}
			}
		}
		public void quit()
		{
			client.SendInfoStream(new InfoStream().write("Quit",null));
			logined = false;
			notice=username = "未登录";
		}
		public void reSet(string newPassword)
		{
			client.SendInfoStream(new InfoStream().write("Reset",newPassword));
		}
		public void save(string path)
		{
			XmlWriterSettings xmlSetting = new XmlWriterSettings();
			xmlSetting.Encoding = new UTF8Encoding(false);
			xmlSetting.Indent = true;
			XmlDocument xmldoc = toXml();
			XmlWriter writer = XmlWriter.Create(path, xmlSetting);
			xmldoc.Save(writer);
			writer.Close();
		}
		public XmlDocument toXml()
		{
			XmlDocument document = new XmlDocument();
			XmlElement e = document.CreateElement("UserServer");
			XmlElement user = document.CreateElement("User");
			XmlElement Mes = document.CreateElement("MessageBoxs");
			e.AppendChild(user);
			e.AppendChild(Mes);
			document.AppendChild(e);
			user.SetAttribute("Name", lastName);
			user.SetAttribute("Key", lastCode);
			user.SetAttribute("Box", lastBox);
			foreach (MessageBox box in messages)
			{
				Mes.AppendChild(box.writeXml(document.CreateElement("MessageBox")));
			}
			return document;
		}
		public void getUsers()
		{
			client.SendInfoStream(new InfoStream().write("GetUsers", null));
		}
		public void getFile(string path, string path2)
		{

		client.	SendInfoStream(new InfoStream().write("GetFile", path, path2));


		}
		public void load(string path)
		{
			XmlDocument d = new XmlDocument();
			d.Load(path);
			loadFromXml(d.DocumentElement);
		}
		public void BBB()
		{
			client.SendInfoStream(new InfoStream().write("GetB",null));
		}
		public void B()
		{
			client.SendInfoStream(new InfoStream().write("SetB", null));
		}
		public void BB()
		{
			client.SendInfoStream(new InfoStream().write("SetBB", null));
		}
		public void say(string text)
		{
			InfoStream info = new InfoStream();
			info.setInfoType(InfoType.OBJECTS);
			info.writeString("Say");
			info.writeString(text);
			client.SendInfoStream(info);
		}
		public override string ToString()
		{
			return "服务器";
		}
		public void close()
		{
			client.Closed();
			logined = false;
			notice = "已断开连接";
		}
	}
	
	public class UserServer:UserManager
	{
	public Server server { get;private set; }
		public event PostText onTextPost;
		Dictionary<ServerSubThread, User> Users=new Dictionary<ServerSubThread, User>();
		public bool b = false;
		List<MessageBox> messages=new List<MessageBox>();
		List<string> texts = new List<string>();
		
		public List<MessageBox> getBoxs(string name)
		{
			List<MessageBox> boxs = new List<MessageBox>();
			foreach(MessageBox box in messages)
			{
				foreach(string s in box.users)
				{
                if(s.Equals(name))
				{
					boxs.Add(box);
						break;
				}
				}
				
			}
			return boxs;
		}
		public MessageBox findBox(params string[] names)
		{
			foreach(MessageBox box in messages)
			{
				if(box.isWith(names))
				{
					return box;
				}
			}
			MessageBox box1 = new MessageBox();
			foreach(string s in names)
			{
			
				box1.addUser(s);
			}
			messages.Add(box1);
			return box1;
		}
		public UserServer(int port)
		{
			server = new Server(port);
			server.onInfoReceived +=onReiceive;
			onTextPost = (string text) => { texts.Add(text); };

		}
		public string [] getUsers()
		{
			string[] us = new string[users.Count];
			int i = 0;
			foreach(User u in users)
			{
				us[i] = u.name;
				i++;
			}
			return us;
		}
		public InfoStream getUserList()
		{
			return new InfoStream().write("ReturnUsers", getUsers());
		}
		
		public List<InfoStream> getsendBox(MessageBox box)
		{
			List<InfoStream> infos = new List<InfoStream>();
			
			foreach(Message m in box.texts)
			{
				infos.Add(getMessageToClientBox(box.users.ToArray(),m))
				;
			
			}


			return infos;
		}
		public InfoStream getMessageToClientBox(string[] box, Message message)
		{
			return new InfoStream().write("NewMessage", box, message.time.ToString(), message.name, message.text,false);
		}

	
		public void sendMessageToClientBox(ServerSubThread thread,string [] box,Message message,bool newSend)
		{
			thread.SendInfoStream(new InfoStream().write("NewMessage",box,message.time.ToString(),message.name,message.text,newSend));
		}
		public bool sendMessage(string a, string b, string text)
		{

			if (a != null && b != null)
			{
				MessageBox box = findBox(a, b);

				if (box != null)
				{
					onTextPost(DateTime.Now + " " + a + "=>" + "{" + a + "," + b + "} :" + text);
					Message m = box.addMessage(a, text);
					foreach (ServerSubThread sub in getThreads(getUser(b)))
					{
						sendMessageToClientBox(sub, box.users.ToArray(), m, true);
					}
					if (a != b)
						foreach (ServerSubThread sub in getThreads(getUser(a)))
						{
							sendMessageToClientBox(sub, box.users.ToArray(), m, true);
						}
					return true;
				}
			}


			User au = getUser(a);
			User bu = getUser(b);
			if(au!=null&&(bu!=null||b.Equals("")))
			{
				sendMessage(au, bu, text);
			}

			return false;
		}
		public bool sendMessage(User a,User b,string text)
		{
			if(a!=null&&b!=null)
			{
				return sendMessage(a.name, b.name,text);
			}
			
			return false;
		}
		public void getFile(ServerSubThread thread, string path, string path2)
		{

		thread.	SendInfoStream(new InfoStream().write("GetFile", path, path2));


		}
		public override void loadFromXml(XmlElement element)
		{
			users.Clear();
			messages.Clear();
			foreach (XmlElement element1 in element.ChildNodes)
			{
				if(element1.Name.Equals("Users"))
				{
					foreach (XmlElement element2 in element1.ChildNodes)
					{
						users.Add(new User(element2));

					}
					}else if (element1.Name.Equals("MessageBoxs"))
				{
					foreach (XmlElement element2 in element1.ChildNodes)
					{
						messages.Add(new MessageBox().readXml(element2));

					}
				}
				else if (element1.Name.Equals("B"))
				{

					b = bool.Parse(element1.GetAttribute("B"));
				}
			}
		}
		public override XmlDocument toXml()
		{
			XmlDocument document = new XmlDocument();
			XmlElement e = document.CreateElement("UserServer");
			XmlElement users = document.CreateElement("Users");
			XmlElement b = document.CreateElement("B");
			b.SetAttribute("B", this.b.ToString());
			XmlElement Mes = document.CreateElement("MessageBoxs");
			e.AppendChild(users);
			e.AppendChild(Mes);
			e.AppendChild(b);
			document.AppendChild(e);
			foreach (User user in this.users)
			{
				users.AppendChild(user.writeXml(document.CreateElement("user")));
			}
			foreach(MessageBox box in messages)
			{
				Mes.AppendChild(box.writeXml(document.CreateElement("MessageBox")));
			}
			return document;
		}
		public List<ServerSubThread> getThreads(User user)
		{
			
			List<ServerSubThread> threads = new List<ServerSubThread>();
			if(user==null)
			{
				return threads;
			}
			foreach(ServerSubThread server
				in Users.Keys)
			{
if(Users[server].Equals(user))
				{
					threads.Add(server);
				}
			}
			return threads;
		}
		public void onReiceive(object info, Server
		server, ServerSubThread thread)
		{
			if(info is List<object> )
			{
				List<object> info_ = (List<object>)info;

				if (info_.Count >0)
				{
					if (info_[0].Equals("Login"))
					{
						User user = getUser((string)info_[1]);
						if (user != null && user.login((string)info_[2]))
						{
							if(!Users.ContainsKey(thread))
							{
                             Users.Add(thread, user);
							thread.onEnd += (ServerSubThread thread1)
								  =>
							  {
								  Users.Remove(thread1);
							  };
							}
							else if(Users[thread]!=user)
							{
								Users[thread] = user;
							}
							
							onTextPost(DateTime.Now.ToString() + ":用户" + user.name + "登录成功");

						
								
						
							;

							foreach(MessageBox box in getBoxs(user.name))
							{
								List<InfoStream> infoStreams = getsendBox(box);
								thread.packToSend(infoStreams );
								onTextPost("Messages:"+infoStreams.Count);
							}	thread.SendInfoStream(
								InfoStream.createList(
									new InfoStream().write("ReturnLogin", user.name, user.passkey), getUserList(),
								new InfoStream().write("ReturnNotice", "登录成功"))

								);
						
						}
						else if(user!=null)
						{
							onTextPost(DateTime.Now.ToString() + ":用户" + user.name + "登录失败");
							server.SendInfoStream(thread, new InfoStream().write("ReturnNotice", "登录失败:密码错误"));
						}
						else
						{
							onTextPost(DateTime.Now.ToString() + ":用户" + (string)info_[1] + "登录失败");
							server.SendInfoStream(thread, new InfoStream().write("ReturnNotice", "登录失败: 用户未注册"));

						}

					}
					else if (info_[0].Equals("Register"))
					{
						if (Register((string)info_[1], (string)info_[2]))
						{
							onTextPost(DateTime.Now.ToString() + ":用户" + (string)info_[1] + "注册成功");
							server.SendInfoStream(thread, new InfoStream().write("ReturnNotice", "注册成功"));

						}
						else
						{
							onTextPost(DateTime.Now.ToString() + ":用户" + (string)info_[1] + "注册失败");
							server.SendInfoStream(thread, new InfoStream().write("ReturnNotice", "注册失败:用户已注册"));

						};
						foreach(ServerSubThread thread1 in server.threads)
						{
						thread1.SendInfoStream(	getUserList());
						}

					}
					else
if (info_[0].Equals("Say"))
					{
						if (Users.ContainsKey(thread))
						{
							User u = Users[thread];
							sendMessage(u.name, "", (string)info_[1]);
							if(u!=null)
						{
							onTextPost(DateTime.Now.ToString() + " " + u.name+": " + (string)info_[1]);

						}
						}
					
						
					}
					
else 
					if (info_[0].Equals("GetB"))
					{
						
						if (Users.ContainsKey(thread))
						{User u = Users[thread];
							onTextPost(DateTime.Now.ToString() + " " + u.name + ":请求获取B态 ");
							server.SendInfoStream(thread,new InfoStream().write("ReturnB",b));
						}

					}

				else if (info_[0].Equals("Quit"))
					{

						if (Users.ContainsKey(thread))
						{
							User u = Users[thread];
							Users.Remove(thread);
							server.SendInfoStream(thread, new InfoStream().write("ReturnNotice", "已退出登录"));
								onTextPost(DateTime.Now.ToString() + " " + u.name + "退出登录");
						}

					}
					else if (info_[0].Equals("Reset"))
					{

						if (Users.ContainsKey(thread))
						{
							User u= Users[thread];
							u.passkey = (string)info_[1];
							onTextPost(DateTime.Now.ToString() + " " + u.name + "更改密码为:"+u.passkey);
							server.SendInfoStream(thread, new InfoStream().write("ReturnNotice", "更改成功"));
						}

					}
					else if (info_[0].Equals("SetBB"))
					{if (Users.ContainsKey(thread))
						{
						User u = Users[thread];
						
							onTextPost(DateTime.Now.ToString() + " " + u.name + ":BB");
							b = false;
							foreach(ServerSubThread sub in Users.Keys)
							{
	server.SendInfoStream(sub , new InfoStream().write("ReturnB", b));
						
							}
						}
					}
					else if (info_[0].Equals("GetFile"))
					{
						thread.sendFile((string)info_[1], (string)info_[2]);
					}
					else if (info_[0].Equals("SetB"))
					{
					
						if (Users.ContainsKey(thread))
						{	User u = Users[thread];
							onTextPost(DateTime.Now.ToString() + " " + u.name + ":B");
							b = true;
							foreach (ServerSubThread sub in Users.Keys)
							{
								server.SendInfoStream(sub, new InfoStream().write("ReturnB", b));

							}
						}
					}
                    else if(info_[0].Equals("GetUsers"))
					{
						
						if (Users.ContainsKey(thread))
						{User u = Users[thread];
							onTextPost(DateTime.Now.ToString() + " " + u.name + "请求用户名单");
							thread.SendInfoStream(getUserList()); 
						}
					}
					else if (info_[0].Equals("Kill"))
					{

						thread.Close();
					}
					else if (info_[0].Equals("GetBox"))
					{

						if (Users.ContainsKey(thread))
						{
							User u = Users[thread];
							MessageBox box = findBox(u.name, (string)info_[1]);
					
							
							onTextPost(DateTime.Now.ToString() + " " + u.name + "请求聊天记录:"+ (string)info_[1]);
			               if(box!=null)
							{
								thread.addToSendList(getsendBox(box).ToArray());
                               ;
							}
						}
					}
					else if (info_[0].Equals("SendTo"))
					{
						
						if (Users.ContainsKey(thread))
						{User u = Users[thread];
							User u2 = getUser((string)info_[1]);
							sendMessage(u, u2, (string)info_[2]);
						}
					}
				}
				
			}
		}

	


		

		public void load(string path)
		{
			XmlDocument d = new XmlDocument();
			d.Load(path);
			loadFromXml(d.DocumentElement);
		}
	}
	public class UserManager
		{
	internal	List<User> users=new List<User>();

		public  bool Register(string name,string key)
		{
			
			return addUser(new User(name,key));
		}
		public bool addUser(User user)
		{
			if(user==null)
			{
				return false;
			}
			foreach(User user1 in users)
			{
				if(user1.name==user.name)
				{
					return false;
				}
			}
			users.Add(user);
			return true;
		}
		public User getUser(string name)
		{
			foreach(User user in users)
			{
				if(user.name==name)
					{
					return user;
				}
			}
			return null;
		}

		public virtual void loadFromXml(XmlElement element)
		{
			users.Clear();
			foreach(XmlElement element1 in element.ChildNodes)
			{
				users.Add(new User(element1));
			}
		}
		public void save(string path)
		{
			XmlWriterSettings xmlSetting = new XmlWriterSettings();
			xmlSetting.Encoding = new UTF8Encoding(false);
			xmlSetting.Indent = true;
			XmlDocument xmldoc = toXml();
			XmlWriter writer = XmlWriter.Create(path, xmlSetting);
			xmldoc.Save(writer);
			writer.Close();
		}
		public virtual XmlDocument toXml()
		{
			XmlDocument document = new XmlDocument();
			XmlElement e = document.CreateElement("Users");
			document.AppendChild(e);
			foreach(User user in users)
			{
				e.AppendChild(user.writeXml(document.CreateElement("user")));
			}
			return document;
			
		}

	}
public	class User:IxmlObject<User>
	{
	internal	string name;
		internal string passkey;
		internal bool isOnline = false;
		public void setKey(string key)
		{
			passkey = key;
		}

		public User(XmlElement x)
		{
			readXml(x);
		}
		public User(string name,string key)
		{
			this.name = name;
			passkey = key;
		}
		public bool login(string key)
		{
			if(key.Equals(passkey))
			{
				isOnline = true;return true;
			}
			else
			{
				return false;
			}
			
		}

		public void logout()
		{
			isOnline = false;
		}
	public	XmlElement writeXml(XmlElement element)
		{
		
			element.SetAttribute("name", name);
			element.SetAttribute("key", passkey);
			return element;
		}

	public	User readXml(XmlElement element)
		{
			name =
element.GetAttribute("name");
			passkey = element.GetAttribute("key");
			return this;
		}
	}
}
