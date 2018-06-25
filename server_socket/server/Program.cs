using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using library;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Management;

namespace server
{
    class Program
    {       
        public static byte[] Serialization(Product obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            byte[] msg = stream.ToArray();
            return msg;
        }

        public static Product DeSerialization(byte[] serializedAsBytes)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(serializedAsBytes, 0, serializedAsBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return (Product)formatter.Deserialize(stream);
        }

        public class ClientObject
        {
            protected internal string Id { get; private set; }
            protected internal NetworkStream Stream { get; private set; }
         
            TcpClient client;
         

            public ClientObject(TcpClient tcpClient)
            {              
                client = tcpClient;
            }
       
            public string Search(string search_line) 
            {
                str = null;
                foreach (string s in mass) { if (s.IndexOf(search_line) != -1) { str = s; } }
                return str;
            }

           
            public void Process()
            { 
                NetworkStream stream = null;
                OwerwritingMas();
                stream = client.GetStream();
                byte[] data = new byte[1024]; // буфер для получаемых данных
                Product product = new Product();
                Product product_ = new Product();
                int bytes = 0;
                while (true)
                {
                    try
                    {
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            product = DeSerialization(data);
                        }
                        while (stream.DataAvailable);
                    }
                    catch
                    { product.type_of_trans = 3; }

                    cl_z = product.ClientNumber;
                   

                    if (product.type_of_trans == 1)
                    {
                        if (Search(product.name) == null)
                        {
                            lock (Lock)
                            {                              
                                product_.type_of_trans = 1;
                                product_.name = "Ошибка! Элемент не найден.\n";
                                byte[] buf = new byte[1024];
                                buf = Serialization(product_);
                                stream.Write(buf, 0, buf.Length);
                            }
                        }

                        else
                        {

                           if (flag ==1 )
                           {
                               lock (Lock)
                               {
                                   Console.WriteLine("Мьютекс занят клиентом номер " + " " + cl_z + "." + " "+"Ожидание...");
                               }
                           }

                            mtx.WaitOne();
                            flag = 1;

                            lock (Lock)
                            {
                                Console.WriteLine("Клиент номер" + " " + product.ClientNumber + " " + "получил мьютекс.");
                            }
                            string f = Search(product.name);
                            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                            foreach (string s in mass)
                            {
                                if (s == f) { continue; }
                                else { sw.WriteLine(s); }
                            }
                            sw.Close();
                            mtx.ReleaseMutex();

                            flag = 0;
                            lock (Lock)
                            {
                                Console.WriteLine("Клиент номер" + " " + product.ClientNumber + " " + "освободил мьютекс.");
                            }
                            OwerwritingMas();
                            product_.type_of_trans = 1;
                            product_.name = "Элемент  удален из списка.\n";
                            byte[] buf = new byte[1024];
                            buf = Serialization(product_);
                            stream.Write(buf, 0, buf.Length);
                        }
                        product_ = new Product();
                    }

                    if (product.type_of_trans == 2)
                    {
                        if (flag == 1)
                        {
                            lock (Lock)
                            {
                                Console.WriteLine("Мьютекс занят клиентом номер " + " " + cl_z + ".");
                            }
                        }

                        mtx.WaitOne();
                        lock (Lock)
                        {
                            Console.WriteLine("Клиент номер" + " " + product.ClientNumber + " " + "получил мьютекс.");
                        }
                        flag = 1;
           
                        FileStream fstream = new FileStream(path, FileMode.OpenOrCreate);
                        byte[] array = null;
                        array = System.Text.Encoding.Default.GetBytes("\n" + product.name + "," + product.price.ToString() + "," + product.type_of_trans.ToString());
                        fstream.Write(array, 0, array.Length);
                        fstream.Close();

                        mtx.ReleaseMutex();

                        flag = 0;

                        lock (Lock)
                        {
                            Console.WriteLine("Клиент номер" + " " + product.ClientNumber + " " + "освободил мьютекс.");
                        }
                        OwerwritingMas();
                       
                        product_.type_of_trans = 2;
                        product_.name = "Элемент добавлен.\n";
                        byte[] buf = new byte[1024];
                        buf = Serialization(product_);
                        stream.Write(buf, 0, buf.Length);
                        product_ = new Product();
                    }

                    if (product.type_of_trans == 3)
                    {
                        
                        if (Search(product.name) == null)
                        {
                            product_ = new Product();
                            product_.type_of_trans = 3;
                            
                            byte[] buf = new byte[1024];
                            buf = Serialization(product_);
                            stream.Write(buf, 0, buf.Length);

                        }

                        else
                        {
                            product_ = new Product();
                            string[] words = Search(product.name).Split(',');
                            product_.name = words[0];
                            product_.price = Convert.ToInt32(words[1]);
                            product_.type_of_trans = 3;

                            byte[] buf = new byte[1024];
                            buf = Serialization(product_);
                            stream.Write(buf, 0, buf.Length);
                        }
                        
                    }                   
                }
               
            }
            
        }

        public static int flag = 0;
        public static int cl_z = 0;
        public static object Lock = new object();
            public static void OwerwritingMas() 
            {
                if (flag == 1)
                {
                    lock (Lock)
                    {
                        Console.WriteLine("Мьютекс занят клиентом номер " + " " + cl_z + ".");
                    }
                }
                mtx.WaitOne();
                flag = 1;
                StreamReader sr = new StreamReader(path, Encoding.Default);
                mass = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                mass = mass.Where(x => x !="").ToArray();
                sr.Close();
                mtx.ReleaseMutex();
                flag = 0;
            }

       
            static TcpListener listener;
            public static Mutex mtx = new Mutex();
            public static string path = @"E:\2 Распределенные системы\lab_2\fileLab.txt";
            public static string[] mass;
            public static string str;

            public class ServerObject
            {
                public void Listen()
                {                    
                        bool existed;
                     
                        string guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();
                        Mutex mutexObj = new Mutex(true, guid, out existed);
                        if (existed == false)
                        {
                            Console.WriteLine("Приложение уже было запущено. И сейчас оно будет закрыто.");
                            Thread.Sleep(3000);
                            return;
                        }
                    met:
                        try
                        {
                     
                            IPAddress localAddress = IPAddress.Parse("127.0.0.1");
                            listener = new TcpListener(localAddress, 8888);
                            listener.Start(1);
                            Console.WriteLine("Ожидание подключений...");

                            while (true)
                            {
                                TcpClient client = listener.AcceptTcpClient();
                                ClientObject clientObject = new ClientObject(client);

                                Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                                Thread.Sleep(1000);

                                clientThread.Start();

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            if (listener != null)
                                listener.Stop();
                        }
                        goto met;
                    }               
            }
            static void Main(string[] args)
            {      
              ServerObject n = new ServerObject();
                n.Listen();
            }
        }
    }
